using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SBPlayer : Photon.MonoBehaviour, IPunObservable {

	Transform bodyObject;
	Rigidbody body;

	Transform bodyProxy;

	public Rigidbody HRotBody;
	public Rigidbody VRotBody;
	public Transform eyes;
	public GameObject targetObj;
	public Transform groundCheck;

	public float maxHTorque = 1.0f;
	public float maxVTorque = 1.0f;

	float maxForce = 300.0f;
	float maxRightForce = 600.0f;
	float trainSpeed = 35.0f;
	float stopDrag = 6.0f;
	float runninDrag = 2.0f;

	float jumpForce = 200.0f;

	float gravityScale = 3.0f;

	public bool localInput = true;

	float RHorizontal;
	float RVertical;
	float LHorizontal;
	float LVertical;
	float RTrigger;
	float LTrigger;
	Vector3 bodyForce;
	bool jumping;
	bool sendJump;
	bool jumpConsumed = true;
	bool sprint;
	float sprintBoost = 3.0f;

	PhotonView m_PhotonView;

	public Transform ballSocket;

	public float ballSpeed = 50.0f;
	
	float startThrowAngle = 10.0f * Mathf.Deg2Rad;
	float maxThrowAngle = 50.0f * Mathf.Deg2Rad;
	float throwAngle;
	bool aiming;
	float throwAngleIncRate = 40.0f * Mathf.Deg2Rad;
	float maxThrowVel = 50.0f;

	float mouseSens = 3.0f;
	float VLookMaxAngle;
	float VLookMinAngle;

	bool grounded;

	// Use this for initialization
	void Awake() {
		
		m_PhotonView = GetComponent<PhotonView>();

		VLookMaxAngle = VRotBody.GetComponent<HingeJoint>().limits.max;
		VLookMinAngle = VRotBody.GetComponent<HingeJoint>().limits.min;
	}

	IEnumerator Start () {

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		bodyObject = transform.Find ("Body");
		body = bodyObject.GetComponent<Rigidbody> ();

		bodyProxy = transform.Find ("BodyProxy");

		eyes = /*transform.Find ("BodyPuller").*/bodyObject.transform.Find ("Eyes");

		if (!m_PhotonView.isMine) {

			//our rigidbody replica is moved cinematically via Photon Transform View
			body.isKinematic = true;

			//Disable all mesh renderer children. We don't want to see the other players exact replica, but bodyProxy instead
			Array.ForEach<Renderer>(bodyObject.GetComponentsInChildren<Renderer> (), x => x.enabled = false);
			//Disable colliders also. bodyProxy is jointed to replica and it is our physical representation of other players
			Array.ForEach<Collider>(bodyObject.GetComponentsInChildren<Collider> (), x => x.enabled = false);

			//Disable Camera of other players' replicas
			eyes.gameObject.SetActive (false);

			yield return new WaitForSeconds(0.5f);

			//move the bodyproxy where the body is
			Rigidbody rb = bodyProxy.GetComponent<Rigidbody>();
			rb.isKinematic = true;
			rb.position = bodyObject.position;
			rb.rotation = bodyObject.rotation;
			rb.isKinematic = false;
			
		} else {
			print ("net: " + PhotonNetwork.sendRate + " " + PhotonNetwork.sendRateOnSerialize);

			Destroy (bodyProxy.gameObject);

			Physics.gravity *= gravityScale;
		}

		yield break;
	}


	void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		
	}
	
	void Update () {

		if (!m_PhotonView.isMine)
		{
			UpdateBodyRenderers();
			return;
		}
		
		if (Input.GetKeyDown(KeyCode.Space))
		{
			jumping = true;
		}

		if (localInput)
		{
			GetInputs();
		}

		//View management. Body rotation follow view
		UpdateView();

		//Place crosshair on ground
		RaycastHit hit;
		Physics.Raycast(eyes.transform.position, eyes.transform.forward, out hit);
		targetObj.transform.position = hit.point;

		if (!aiming && (Input.GetButtonDown("Fire1")))
		{
			aiming = true;
			throwAngle = startThrowAngle;
		}

		if(aiming)
		{
			throwAngle = Mathf.Clamp(throwAngle, startThrowAngle, maxThrowAngle);

			if(Input.GetButtonUp("Fire1"))
			{
				aiming = false;
				
				Vector3 vel = ComputeThrowVelocity(hit.point, ballSocket.position, throwAngle);
				GetComponentInChildren<CatchBall>().ThrowBall(vel);
			}

			throwAngle += Time.deltaTime * throwAngleIncRate;
		}
	}


	void FixedUpdate() {

		if (photonView.isMine)
		{
			if(!IsBodyGrounded(body) && jumping)
			{
				jumping = false;
			}

			ApplyBodyForces(body, bodyForce, sprint, jumping);

			if(jumping)
			{
				jumping = false;
				sendJump = true;
			}


			HRotBody.AddRelativeTorque(Vector3.up * RHorizontal * maxHTorque);
			VRotBody.AddRelativeTorque(Vector3.right * RVertical * maxVTorque);
		}
		else
		{
			ApplyBodyForces(bodyProxy.GetComponent<Rigidbody>(), bodyForce, sprint, sendJump);
			sendJump = false;
			jumpConsumed = true;
		}	
		
	}
	
	void ApplyBodyForces(Rigidbody _body, Vector3 _force, bool _sprint, bool _jump)
	{
		_force.Normalize();
		_force *= maxForce;
		
		if (_sprint)
		{
			_force *= sprintBoost;
		}

		if (_force.magnitude > 0)
		{
			_body.drag = runninDrag;
		}
		else
		{
			_body.drag = stopDrag;
		}
		
		
		if (IsBodyGrounded(_body))
		{
			//Reduced turning speed based on current velocity
			//the faster I run the less I can turn
			if (_body.velocity.magnitude > 0.01f)
			{
				Vector3 rightOfVelocity = Vector3.Cross(Vector3.up, _body.velocity);

				float fwForce = Vector3.Dot(_force, _body.velocity.normalized);
				float rightForce = Vector3.Dot(_force, rightOfVelocity.normalized);

				float rightForceLimit = Mathf.Lerp(maxRightForce, 0, Mathf.InverseLerp(0, trainSpeed, _body.velocity.magnitude));

				rightForce = Mathf.Clamp(rightForce, -rightForceLimit, rightForceLimit);

				_force = fwForce * _body.velocity.normalized + rightForce * rightOfVelocity.normalized;
			}

			_body.AddForce(_force);
		}
		else
		{
			_body.drag = 0;
		}

		if(_jump)
		{
			_body.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting) {

			localInput = false;

			GetInputs();

			stream.SendNext (bodyForce);
			stream.SendNext (sendJump);
			stream.SendNext (sprint);

			sendJump = false;

		}
		else
		{
			
			bodyForce = (Vector3)stream.ReceiveNext ();
			bool jump = (bool)stream.ReceiveNext();
			sprint = (bool)stream.ReceiveNext();

			if (jumpConsumed)
			{
				sendJump = jump;
				jumpConsumed = false;
			}

		}
	}

	void GetInputs()
	{
		RHorizontal = Input.GetAxis("RHorizontal");
		RVertical = Input.GetAxis("RVertical");

		LHorizontal = Input.GetAxis("LHorizontal");
		LVertical = Input.GetAxis("LVertical");

		RTrigger = Input.GetAxis("RTrigger");
		LTrigger = Input.GetAxis("LTrigger");

		sprint = Input.GetKey(KeyCode.LeftShift);
		
		if (Input.GetKey(KeyCode.A))
			LHorizontal = -1;
		if (Input.GetKey(KeyCode.D))
			LHorizontal = 1;
		if (Input.GetKey(KeyCode.W))
			LVertical = 1;
		if (Input.GetKey(KeyCode.S))
			LVertical = -1;

		bodyForce = bodyObject.right * LHorizontal
				+ bodyObject.forward * LVertical;
	}

	void UpdateCursorVisibility()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (Cursor.visible)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
			else
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}
	}

	void UpdateBodyRenderers()
	{
		if (Input.GetKeyDown(KeyCode.M))
		{
			Array.ForEach<Renderer>(
				bodyObject.GetComponentsInChildren<Renderer>(), a => a.enabled = !a.enabled);
		}
	}

	bool IsBodyGrounded(Rigidbody _body)
	{
		RaycastHit hit;
		return Physics.Raycast(_body.transform.position, -Vector3.up, out hit, 0.1f);
	}

	Vector3 ComputeThrowVelocity(Vector3 startPos, Vector3 endPos, float angle)
	{
		Vector3 ballToTarget = startPos - endPos;
		float y = Vector3.Dot(ballToTarget, Vector3.up);
		Plane plane = new Plane(Vector3.up, Vector3.zero);
		Vector3 ballToTargetProj = plane.ClosestPointOnPlane(ballToTarget);
		float x = ballToTargetProj.magnitude;
		float delta = Mathf.Pow(ballSpeed, 4) -
			Physics.gravity.magnitude * (Physics.gravity.magnitude * x * x + 2 * y * ballSpeed * ballSpeed);
		if (delta < 0)
		{
			Debug.LogError("negative delta");
		}
		//throwAngle = Mathf.Atan((speed * speed - Mathf.Sqrt(delta)) / (Physics.gravity.magnitude * x
		ballSpeed = Mathf.Sqrt(x * x * Physics.gravity.magnitude / (x * Mathf.Sin(2 * angle) - 2 * y * Mathf.Pow(Mathf.Cos(angle), 2)));
		Vector3 vel = Vector3.up * ballSpeed * Mathf.Sin(angle) + ballToTargetProj.normalized * ballSpeed * Mathf.Cos(angle);
		//print(angle * Mathf.Rad2Deg);
		if (vel.magnitude > maxThrowVel)
		{
			vel = vel.normalized * maxThrowVel;
		}

		return vel;
	}

	void UpdateView()
	{
		float mouseX = mouseSens * Input.GetAxis("Mouse X");
		float mouseY = -mouseSens * Input.GetAxis("Mouse Y");
		HRotBody.transform.Rotate(0, mouseX, 0);
		VRotBody.transform.Rotate(mouseY, 0, 0);
		Vector3 euler = VRotBody.transform.eulerAngles;
		float vAngle = euler.x;
		if (vAngle > 180.0f)
		{
			vAngle -= 360.0f;
		}
		euler.x = Mathf.Clamp(vAngle, VLookMinAngle, VLookMaxAngle);
		VRotBody.transform.eulerAngles = euler;
		eyes.transform.rotation = HRotBody.transform.rotation * VRotBody.transform.rotation;
		body.rotation = HRotBody.transform.rotation;
	}
}

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
			Array.ForEach<MeshRenderer>(bodyObject.GetComponentsInChildren<MeshRenderer> (), x => x.enabled = false);
			//Disable colliders also. bodyProxy is jointed to replica and it is our physic representation of other players
			Array.ForEach<Collider>(bodyObject.GetComponentsInChildren<Collider> (), x => x.enabled = false);

			Array.ForEach<TrailRenderer>(bodyObject.GetComponentsInChildren<TrailRenderer> (), x => x.enabled = false);

			//Disable Camera of other players' replicas
			eyes.gameObject.SetActive (false);

			yield return new WaitForSeconds(0.5f);

			Rigidbody rb = transform.Find("BodyProxy").GetComponent<Rigidbody>();
			rb.isKinematic = true;
			rb.position = transform.Find("Body").position;
			rb.rotation = transform.Find("Body").rotation;
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
			if(Input.GetKeyDown(KeyCode.M))
			{
				Array.ForEach<MeshRenderer>(
					bodyObject.GetComponentsInChildren<MeshRenderer>(), a => a.enabled = !a.enabled);
			}
			return;
		}

		//Ground Check for jumping
		RaycastHit hit;

		grounded = Physics.Raycast(body.transform.position, -Vector3.up, out hit, (body.transform.position - groundCheck.position).magnitude);

		if (Input.GetKeyDown(KeyCode.Space) && grounded)
		{
			body.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
		}

		if (localInput)
		{
			GetInputs();
		}

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

		float mouseX = mouseSens * Input.GetAxis("Mouse X");
		float mouseY = -mouseSens * Input.GetAxis("Mouse Y");

		HRotBody.transform.Rotate(0, mouseX, 0);
		VRotBody.transform.Rotate(mouseY, 0, 0);

		Vector3 euler = VRotBody.transform.eulerAngles;

		float vAngle = euler.x;
		if(vAngle > 180.0f)
		{
			vAngle -= 360.0f;
		}

		euler.x = Mathf.Clamp(vAngle, VLookMinAngle, VLookMaxAngle);

		VRotBody.transform.eulerAngles = euler;

		eyes.transform.rotation = HRotBody.transform.rotation * VRotBody.transform.rotation;

		body.rotation = HRotBody.transform.rotation;
		
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

				Vector3 ballToTarget = hit.point - ballSocket.position;

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

				//throwAngle = Mathf.Atan((speed * speed - Mathf.Sqrt(delta)) / (Physics.gravity.magnitude * x));

				ballSpeed = Mathf.Sqrt(x * x * Physics.gravity.magnitude / (x * Mathf.Sin(2 * throwAngle) - 2 * y * Mathf.Pow(Mathf.Cos(throwAngle), 2)));

				Vector3 vel = Vector3.up * ballSpeed * Mathf.Sin(throwAngle) + ballToTargetProj.normalized * ballSpeed * Mathf.Cos(throwAngle);

				print(throwAngle * Mathf.Rad2Deg);

				GetComponentInChildren<CatchBall>().ThrowBall(vel);

			}

			throwAngle += Time.deltaTime * throwAngleIncRate;
		}

		
		
	}


	void FixedUpdate() {

		if (photonView.isMine)
		{

			Vector3 force = bodyObject.right * LHorizontal
				+ bodyObject.forward * LVertical;

			force.Normalize();
			force *= maxForce;

			if(sprint)
			{
				force *= sprintBoost;
			}

			if(force.magnitude > 0)
			{
				body.drag = runninDrag;
			}
			else
			{
				body.drag = stopDrag;
			}

			if (!grounded)
				body.drag = 0;

			//Reduced turning speed based on current velocity
			//the faster I run the less I can turn
			if(body.velocity.magnitude > 0.01f)
			{
				Vector3 rightOfVelocity = Vector3.Cross(Vector3.up, body.velocity);

				float fwForce = Vector3.Dot(force, body.velocity.normalized);
				float rightForce = Vector3.Dot(force, rightOfVelocity.normalized);

				float rightForceLimit = Mathf.Lerp(maxRightForce, 0, Mathf.InverseLerp(0, trainSpeed, body.velocity.magnitude));

				rightForce = Mathf.Clamp(rightForce, -rightForceLimit, rightForceLimit);

				force = fwForce * body.velocity.normalized + rightForce * rightOfVelocity.normalized;
			}

			body.AddForce(force
				
				//+ Vector3.up * RTrigger * maxForce
				//- Vector3.up * LTrigger * maxForce
			);

			HRotBody.AddRelativeTorque(Vector3.up * RHorizontal * maxHTorque);
			VRotBody.AddRelativeTorque(Vector3.right * RVertical * maxVTorque);
		}
		else
		{
			bodyProxy.GetComponent<Rigidbody>().AddForce(
				bodyObject.right * LHorizontal * maxForce
				+ bodyObject.forward * LVertical * maxForce
				//+ Vector3.up * RTrigger * maxForce
				//- Vector3.up * LTrigger * maxForce
			);
		}	
		
	}
	

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{

		if (stream.isWriting) {

			localInput = false;

			GetInputs();

			stream.SendNext (RHorizontal);
			stream.SendNext (RVertical);
			stream.SendNext (LHorizontal);
			stream.SendNext (LVertical);
			stream.SendNext (RTrigger);
			stream.SendNext (LTrigger);
			stream.SendNext (sprint);

		} else {
			
			RHorizontal = (float)stream.ReceiveNext ();
			RVertical = (float)stream.ReceiveNext ();
			LHorizontal = (float)stream.ReceiveNext ();
			LVertical = (float)stream.ReceiveNext ();
			RTrigger = (float)stream.ReceiveNext ();
			LTrigger = (float)stream.ReceiveNext ();
			sprint = (bool)stream.ReceiveNext();

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


		if(!grounded)
		{
			LHorizontal = 0;
			LVertical = 0;
		}
	}
}

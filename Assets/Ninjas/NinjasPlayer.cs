using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NinjasPlayer : Photon.MonoBehaviour, IPunObservable
{

	Transform bodyObject;
	Rigidbody body;

	Transform bodyProxy;

	public Rigidbody HRotBody;
	public Rigidbody VRotBody;
	public Transform eyes;
	public GameObject targetObj;
	public Transform groundCheck;

	float mouseSens = 3.0f;

	public float maxHTorque = 1.0f;
	public float maxVTorque = 1.0f;

	float maxForce = 500.0f;
	float maxRightForce = 600.0f;
	float trainSpeed = 35.0f;
	float stopDrag = 10.0f;
	float runninDrag = 7.0f;

	float sprintBoost = 1.8f;
	public float sprintPool;  //0 to 1
	float sprintEmptyRate = 1.0f;
	float sprintFillRate = 0.2f;

	float jumpForce = 150.0f;

	float punchCooldown = 0.5f;
	float punchDashForce = 200.0f;

	float punchedRecovery = 0.5f;

	float ballSpeed = 50.0f;
	float startThrowAngle = 10.0f * Mathf.Deg2Rad;
	float maxThrowAngle = 50.0f * Mathf.Deg2Rad;
	float throwAngleIncRate = 40.0f * Mathf.Deg2Rad;
	float maxThrowVel = 50.0f;
	public Transform ballSocket;

	float gravityScale = 1.0f;

	public bool localInput = true;

	float RHorizontal;
	float RVertical;
	float LHorizontal;
	float LVertical;
	//float RTrigger;
	//float LTrigger;
	Vector3 bodyForce;
	bool jumping;
	bool sendJump;
	bool jumpConsumed = true;
	bool sprint;
	float lastSprintInputTime;
	int life = 5;

	PhotonView m_PhotonView;



	float throwAngle;
	bool aiming;
	bool punching;
	bool sendPunch;
	Vector3 punchDir;

	bool punched;
	float punchTime;
	float punchedTime;

	float VLookMaxAngle;
	float VLookMinAngle;

	CatchBall ballCatcher;

	float jointOriginalSpring;
	float jointOriginalDamp;

	// Use this for initialization
	void Awake()
	{

		m_PhotonView = GetComponent<PhotonView>();

		VLookMaxAngle = VRotBody.GetComponent<HingeJoint>().limits.max;
		VLookMinAngle = VRotBody.GetComponent<HingeJoint>().limits.min;

		ballCatcher = GetComponentInChildren<CatchBall>();

		life = 0;
	}

	IEnumerator Start()
	{

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		bodyObject = transform.Find("Body");
		body = bodyObject.GetComponent<Rigidbody>();

		bodyProxy = transform.Find("BodyProxy");

		eyes = /*transform.Find ("BodyPuller").*/bodyObject.transform.Find("Eyes");

		if (!m_PhotonView.isMine)
		{

			//our rigidbody replica is moved cinematically via Photon Transform View
			body.isKinematic = true;

			//Disable all mesh renderer children. We don't want to see the other players exact replica, but bodyProxy instead
			Array.ForEach<Renderer>(bodyObject.GetComponentsInChildren<Renderer>(), x => x.enabled = false);
			//Disable colliders also. bodyProxy is jointed to replica and it is our physical representation of other players
			Array.ForEach<Collider>(bodyObject.GetComponentsInChildren<Collider>(), x => x.enabled = false);

			//Destroy animated character
			Destroy(bodyObject.GetComponentInChildren<SBAnimator>().gameObject);
			//Destroy ball catcher
			Destroy(bodyObject.GetComponentInChildren<CatchBall>().gameObject);

			//Disable Camera of other players' replicas
			eyes.gameObject.SetActive(false);

			yield return new WaitForSeconds(0.5f);

			//move the bodyproxy where the body is
			Rigidbody rb = bodyProxy.GetComponent<Rigidbody>();
			rb.isKinematic = true;
			rb.position = bodyObject.position;
			rb.rotation = bodyObject.rotation;
			rb.isKinematic = false;

		}
		else
		{
			print("net: " + PhotonNetwork.sendRate + " " + PhotonNetwork.sendRateOnSerialize);

			Destroy(bodyProxy.gameObject);
			bodyProxy = null;

			Physics.gravity *= gravityScale;
		}

		yield break;
	}


	void OnPhotonInstantiate(PhotonMessageInfo info)
	{

	}

	void Update()
	{

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
			GetMovementInputs();
		}

		//View management. Body rotation follow view
		UpdateView();

		//Place crosshair on ground
		RaycastHit hit;
		Physics.Raycast(eyes.transform.position, eyes.transform.forward, out hit);
		targetObj.transform.position = hit.point;

		if (punching)
		{
			if (Time.time - punchTime > punchCooldown)
			{
				punching = false;
				GetComponentInChildren<SBAnimator>().StopPunch();
				GetComponentInChildren<Puncher>().DisablePunch();
				bodyObject.gameObject.layer = LayerMask.NameToLayer("PlayerColliding");
			}
			return;
		}

		if (!aiming && (Input.GetButtonDown("Fire1")))
		{
			if (ballCatcher.HasBall())
			{
				aiming = true;
				throwAngle = startThrowAngle;
			}
			else
			{
				GetComponentInChildren<SBAnimator>().PlayPunch();
				GetComponentInChildren<Puncher>().EnablePunch();
				bodyObject.gameObject.layer = LayerMask.NameToLayer("PlayerNotColliding");
				punchTime = Time.time;
				punching = true;
				punchForceApplied = false;
				sendPunch = true;
				punchDir = GetComponentInChildren<SBAnimator>().transform.forward;
				
			}
		}

		if (aiming)
		{
			if (ballCatcher.HasBall())
			{
				throwAngle = Mathf.Clamp(throwAngle, startThrowAngle, maxThrowAngle);

				if (Input.GetButtonUp("Fire1"))
				{
					aiming = false;

					Vector3 vel = ComputeThrowVelocity(hit.point, ballSocket.position, throwAngle);
					ballCatcher.ThrowBall(vel);
				}

				throwAngle += Time.deltaTime * throwAngleIncRate;
			}
			else
			{
				aiming = false;
			}
		}
	}

	bool punchForceApplied;

	void FixedUpdate()
	{

		Vector3 force = bodyForce;

		if (photonView.isMine)
		{

			if (!IsBodyGrounded(body))
			{
				if (jumping)
				{
					jumping = false;
				}

				force = Vector3.zero;
			}

			if (punching)
			{
				force = Vector3.zero;
			}

			if (punched)
			{
				force = Vector3.zero;
				if (Time.time - punchedTime > punchedRecovery)
				{
					punched = false;
					GetComponentInChildren<SBAnimator>().StopPunched();
				}
			}

			ApplyBodyForces(body, force, sprint, jumping, punching && !punchForceApplied, punchDir);

			if (punching && !punchForceApplied)
			{
				punchForceApplied = true;
			}

			if (jumping)
			{
				jumping = false;
				sendJump = true;
			}


			HRotBody.AddRelativeTorque(Vector3.up * RHorizontal * maxHTorque);
			VRotBody.AddRelativeTorque(Vector3.right * RVertical * maxVTorque);
		}
		else
		{
			if (!IsBodyGrounded(bodyProxy.GetComponent<Rigidbody>()))
			{
				force = Vector3.zero;
			}

			if (punching)
			{
				force = Vector3.zero;
				if (Time.time - punchTime > punchCooldown)
				{
					punching = false;
					GetComponentInChildren<SBAnimator>().StopPunch();
				}
			}

			if (punched)
			{
				force = Vector3.zero;
				if (Time.time - punchedTime > punchedRecovery)
				{
					punched = false;
					GetComponentInChildren<SBAnimator>().StopPunched();
					bodyProxy.transform.Find("KinematicBodyProxy").gameObject.SetActive(true);
					//TODO: bodyproxy joint needs to be enabled

					if (bodyProxy != null)
					{
						ConfigurableJoint joint = bodyProxy.GetComponent<ConfigurableJoint>();

						jointOriginalSpring = joint.xDrive.positionSpring;
						jointOriginalDamp = joint.xDrive.positionDamper;

						JointDrive drive = new JointDrive();
						drive.positionSpring = jointOriginalSpring;
						drive.positionDamper = jointOriginalDamp;
						drive.maximumForce = Mathf.Infinity;

						joint.xDrive = drive;
						joint.yDrive = drive;
						joint.zDrive = drive;
					}
				}
			}

			ApplyBodyForces(bodyProxy.GetComponent<Rigidbody>(), force, sprint, sendJump, punching && !punchForceApplied, punchDir);

			if (punching && !punchForceApplied)
			{
				punchForceApplied = true;
			}

			sendJump = false;
			jumpConsumed = true;
		}

	}

	void ApplyBodyForces(Rigidbody _body, Vector3 _force, bool _sprint, bool _jump, bool _punch, Vector3 _punchDir)
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

		if (!IsBodyGrounded(_body))
		{
			_body.drag = 0;
		}

		if (_jump)
		{
			_body.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
		}

		if (_punch && IsBodyGrounded(_body))
		{
			_body.AddForce(_punchDir.normalized * punchDashForce, ForceMode.Impulse);
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{

			localInput = false;

			GetMovementInputs();

			stream.SendNext(bodyForce);
			stream.SendNext(sendJump);
			stream.SendNext(sprint);
			stream.SendNext(sendPunch);
			stream.SendNext(punchDir);
			stream.SendNext(life);

			sendJump = false;
			sendPunch = false;

		}
		else
		{

			bodyForce = (Vector3)stream.ReceiveNext();
			bool jump = (bool)stream.ReceiveNext();
			sprint = (bool)stream.ReceiveNext();
			bool punch = (bool)stream.ReceiveNext();
			punchDir = (Vector3)stream.ReceiveNext();
			life = (int)stream.ReceiveNext();

			if (jumpConsumed)
			{
				sendJump = jump;
				jumpConsumed = false;
			}

			if (punch)
			{
				punching = true;
				punchTime = Time.time;
				GetComponentInChildren<SBAnimator>().PlayPunch();
				punchForceApplied = false;
			}

		}
	}

	void GetMovementInputs()
	{
		RHorizontal = Input.GetAxis("RHorizontal");
		RVertical = Input.GetAxis("RVertical");

		LHorizontal = Input.GetAxis("LHorizontal");
		LVertical = Input.GetAxis("LVertical");

		//RTrigger = Input.GetAxis("RTrigger");
		//LTrigger = Input.GetAxis("LTrigger");

		float deltaTime = Time.time - lastSprintInputTime;
		lastSprintInputTime = Time.time;

		bool sprintKey = Input.GetKey(KeyCode.LeftShift);

		if(sprintKey)
		{
			sprintPool -= sprintEmptyRate * deltaTime;

			if(sprintPool <= 0.0f)
			{
				sprintPool = 0.0f;
				sprint = false;
			}
			else
			{
				sprint = true;
			}
		}
		else
		{
			sprintPool += sprintFillRate * deltaTime;
			sprintPool = Mathf.Clamp01(sprintPool);
			sprint = false;
		}

		if (Input.GetKey(KeyCode.A))
			LHorizontal = -1;
		if (Input.GetKey(KeyCode.D))
			LHorizontal = 1;
		if (Input.GetKey(KeyCode.W))
			LVertical = 1;
		if (Input.GetKey(KeyCode.S))
			LVertical = -1;

		if (bodyObject == null)
		{
			return;
		}

		bodyForce = bodyObject.right * LHorizontal
				+ bodyObject.forward * LVertical;
	}

	void UpdateCursorVisibility()
	{
		return;
		/*
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

		if(Input.GetButtonDown("Fire1"))
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		*/
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

		/*
		float delta = Mathf.Pow(ballSpeed, 4) -
			Physics.gravity.magnitude * (Physics.gravity.magnitude * x * x + 2 * y * ballSpeed * ballSpeed);
		if (delta < 0)
		{
			Debug.LogError("negative delta");
		}
		throwAngle = Mathf.Atan((speed * speed - Mathf.Sqrt(delta)) / (Physics.gravity.magnitude * x
		*/

		float sqrtArg = x * x * Physics.gravity.magnitude / (x * Mathf.Sin(2 * angle) - 2 * y * Mathf.Pow(Mathf.Cos(angle), 2));

		if (sqrtArg < 0)
		{
			ballSpeed = maxThrowVel;
		}
		else
		{
			ballSpeed = Mathf.Sqrt(sqrtArg);
		}

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

	public void GetPunched()
	{
		punched = true;
		punchedTime = Time.time;

		//Play Punched Animation
		GetComponentInChildren<SBAnimator>().PlayPunched();

		//Drop the ball if have it
		if (ballCatcher != null && ballCatcher.HasBall())
		{
			ballCatcher.DropBall();
		}

		//disable bodyproxy joints
		if (bodyProxy != null)
		{
			ConfigurableJoint joint = bodyProxy.GetComponent<ConfigurableJoint>();

			jointOriginalSpring = joint.xDrive.positionSpring;
			jointOriginalDamp = joint.xDrive.positionDamper;

			JointDrive drive = new JointDrive();
			drive.positionSpring = jointOriginalSpring;
			drive.positionDamper = jointOriginalDamp;
			drive.maximumForce = 0.0f;

			joint.xDrive = drive;
			joint.yDrive = drive;
			joint.zDrive = drive;
		}

		life--;

		if (life == 0)
		{
			PhotonNetwork.Destroy(body.GetComponentInParent<DualStickFly>().gameObject);
			Vector3 position = GameObject.Find("Spawn").transform.position;
			GameObject newPlayerObject = PhotonNetwork.Instantiate("TestPlayer", position, Quaternion.identity, 0);
		}
	}
}

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

	public float maxHTorque = 1.0f;
	public float maxVTorque = 1.0f;

	public float maxForce = 1.0f;

	public bool localInput = true;

	float RHorizontal;
	float RVertical;
	float LHorizontal;
	float LVertical;
	float RTrigger;
	float LTrigger;

	PhotonView m_PhotonView;

	public Transform ballSocket;

	public float speed = 50.0f;

	float startThrowAngle = 10.0f * Mathf.Deg2Rad;
	float maxThrowAngle = 50.0f * Mathf.Deg2Rad;
	float throwAngle;
	bool aiming;
	float throwAngleIncRate = 40.0f * Mathf.Deg2Rad;

	float mouseSens = 3.0f;
	float VLookMaxAngle;
	float VLookMinAngle;

	// Use this for initialization
	void Awake() {
		
		m_PhotonView = GetComponent<PhotonView>();

		VLookMaxAngle = VRotBody.GetComponent<HingeJoint>().limits.max;
		VLookMinAngle = VRotBody.GetComponent<HingeJoint>().limits.min;
	}

	IEnumerator Start () {

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
		}

		yield break;
	}


	void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		
	}
	
	// Update is called once per frame
	void Update () {

		if (!m_PhotonView.isMine)
		{
			if(Input.GetKeyDown(KeyCode.Space))
			{
				Array.ForEach<MeshRenderer>(
					bodyObject.GetComponentsInChildren<MeshRenderer>(), a => a.enabled = !a.enabled);
			}
			return;
		}
		
		if(localInput)
		{
			GetInputs();
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

				Vector3 ballToTarget = hit.point - ballSocket.position;

				float y = Vector3.Dot(ballToTarget, Vector3.up);

				Plane plane = new Plane(Vector3.up, Vector3.zero);

				Vector3 ballToTargetProj = plane.ClosestPointOnPlane(ballToTarget);
				float x = ballToTargetProj.magnitude;

				float delta = Mathf.Pow(speed, 4) -
					Physics.gravity.magnitude * (Physics.gravity.magnitude * x * x + 2 * y * speed * speed);

				if (delta < 0)
				{
					Debug.LogError("negative delta");
				}

				//throwAngle = Mathf.Atan((speed * speed - Mathf.Sqrt(delta)) / (Physics.gravity.magnitude * x));

				speed = Mathf.Sqrt(x * x * Physics.gravity.magnitude / (x * Mathf.Sin(2 * throwAngle) - 2 * y * Mathf.Pow(Mathf.Cos(throwAngle), 2)));

				Vector3 vel = Vector3.up * speed * Mathf.Sin(throwAngle) + ballToTargetProj.normalized * speed * Mathf.Cos(throwAngle);

				print(throwAngle * Mathf.Rad2Deg);

				GetComponentInChildren<CatchBall>().ThrowBall(vel);

			}

			throwAngle += Time.deltaTime * throwAngleIncRate;
		}

		
	}


	void FixedUpdate() {

		if (photonView.isMine)
		{

			body.AddForce(
				bodyObject.right * LHorizontal * maxForce
				+ bodyObject.forward * LVertical * maxForce
				+ Vector3.up * RTrigger * maxForce
				- Vector3.up * LTrigger * maxForce
			);

			HRotBody.AddRelativeTorque(Vector3.up * RHorizontal * maxHTorque);
			VRotBody.AddRelativeTorque(Vector3.right * RVertical * maxVTorque);
		}
		else
		{
			bodyProxy.GetComponent<Rigidbody>().AddForce(
				bodyObject.right * LHorizontal * maxForce
				+ bodyObject.forward * LVertical * maxForce
				+ Vector3.up * RTrigger * maxForce
				- Vector3.up * LTrigger * maxForce
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

		} else {
			
			RHorizontal = (float)stream.ReceiveNext ();
			RVertical = (float)stream.ReceiveNext ();
			LHorizontal = (float)stream.ReceiveNext ();
			LVertical = (float)stream.ReceiveNext ();
			RTrigger = (float)stream.ReceiveNext ();
			LTrigger = (float)stream.ReceiveNext ();
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

		if (Input.GetKey(KeyCode.A))
			LHorizontal = -1;
		if (Input.GetKey(KeyCode.D))
			LHorizontal = 1;
		if (Input.GetKey(KeyCode.W))
			LVertical = 1;
		if (Input.GetKey(KeyCode.S))
			LVertical = -1;
	}
}

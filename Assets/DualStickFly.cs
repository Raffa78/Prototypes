using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DualStickFly : Photon.MonoBehaviour, IPunObservable {

	Rigidbody body;

	public Rigidbody HRotBody;
	public Rigidbody VRotBody;
	public Transform eyes;

	public float maxHTorque = 1.0f;
	public float maxVTorque = 1.0f;

	public float maxForce = 1.0f;

	[Header("Wing Settings")]
	public GameObject LeftWing; 
	public GameObject RightWing;
	public AnimationCurve forceProfile;
	public float blowWingMaxAngle = 45;
	bool canBlow = true;
	bool blowing = false;
	float lastBlowTime;
	float profileDuration;
	float[] integralCurve;

	float RHorizontal;
	float RVertical;
	float LHorizontal;
	float LVertical;
	float RTrigger;
	float LTrigger;

	PhotonView m_PhotonView;

	// Use this for initialization
	void Awake() {
		
		body = GetComponent<Rigidbody> ();
		m_PhotonView = GetComponent<PhotonView>();
	}

	void Start () {
		profileDuration = forceProfile.keys [forceProfile.length - 1].time;
		print (profileDuration);
		IntegrateCurve ();

		PhotonNetwork.sendRate = 15;
		PhotonNetwork.sendRateOnSerialize = 15;

		if (!m_PhotonView.isMine) {
			
			GetComponentInChildren<Camera> ().enabled = false;

			GameObject newPlayerObject = PhotonNetwork.Instantiate( "PlayerFeedback", Vector3.zero, Quaternion.identity, (byte)1 );
			newPlayerObject.transform.parent = transform;
			newPlayerObject.transform.localPosition = Vector3.zero;
		}
	}
	
	// Update is called once per frame
	void Update () {
		return;
		
		RHorizontal = Input.GetAxis ("RHorizontal");
		RVertical = Input.GetAxis ("RVertical");
		LHorizontal = Input.GetAxis ("LHorizontal");
		LVertical = Input.GetAxis ("LVertical");
		RTrigger = Input.GetAxis ("RTrigger");
		LTrigger = Input.GetAxis ("LTrigger");

		eyes.transform.rotation = HRotBody.transform.rotation * VRotBody.transform.rotation;

		GetComponent<Rigidbody> ().rotation = HRotBody.transform.rotation;




	}

	void FixedUpdate() {
		
		//HRotBody.AddRelativeTorque (Vector3.up * RHorizontal * maxHTorque);
		//VRotBody.AddRelativeTorque (Vector3.right * RVertical * maxVTorque);

		BasicFlight ();
		//IcarusFlight ();
	}

	void BasicFlight() {
		
		body.AddForce (
			transform.right * LHorizontal * maxForce
			+ transform.forward * LVertical * maxForce 
			+ Vector3.up * RTrigger * maxForce
			- Vector3.up * LTrigger * maxForce
		);
	}


	void IcarusFlight() {

		
		Vector3 force = HRotBody.transform.right * LHorizontal
		                + HRotBody.transform.forward * LVertical
		                + Vector3.up * RTrigger
		                - Vector3.up * LTrigger;

		if (force.magnitude > 1.0f) {
			force.Normalize ();
		}

		if (canBlow && force.magnitude > 0.1) {
			
			canBlow = false;
			blowing = true;
			lastBlowTime = Time.time;
		}
			
		if (blowing) {
			
			float t = Time.time - lastBlowTime;

			if (t > profileDuration) {
				
				blowing = false;
				canBlow = true;

			} else {

				body.AddForce(force * maxForce * forceProfile.Evaluate(t));
				LeftWing.transform.localEulerAngles = new Vector3(0, 0, blowWingMaxAngle * Integral(t));
				RightWing.transform.localEulerAngles = new Vector3(0, 0, -blowWingMaxAngle * Integral(t));
			}
		}
	}

	public void IntegrateCurve()
	{
		float x_high = profileDuration;
		float x_low = 0.0f;
		float N_steps = 50.0f;

		float h = (x_high - x_low) / N_steps;
		float res = 0.0f;

		integralCurve = new float[(int)N_steps];
		float max = 0.0f;

		for (int i = 0; i < N_steps; i++)
		{
			res += forceProfile.Evaluate(x_low + i * h);

			if (max < res * h)
				max = res * h;
			
			integralCurve [i] = res * h;
		}

		//normalize to max
		for (int i = 0; i < N_steps; i++) {

			integralCurve [i] = integralCurve [i] / max;
		}

		return;
	}

	float Integral(float time){
		
		float N_steps = integralCurve.Length;
		float x = time * N_steps / profileDuration;

		int x_low = Mathf.FloorToInt (x);
		int x_high = Mathf.CeilToInt (x);

		if (x_low != x_high) {
			
			return Mathf.Lerp (
				integralCurve [x_low],
				integralCurve [x_high],
				x - x_low);
			
		}

		return integralCurve [x_high];
	}


	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting) {

			RHorizontal = Input.GetAxis ("RHorizontal");
			RVertical = Input.GetAxis ("RVertical");
			LHorizontal = Input.GetAxis ("LHorizontal");
			LVertical = Input.GetAxis ("LVertical");
			RTrigger = Input.GetAxis ("RTrigger");
			LTrigger = Input.GetAxis ("LTrigger");

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
}

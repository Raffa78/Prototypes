using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DualStickFly : MonoBehaviour {

	Rigidbody body;

	public Rigidbody HRotBody;
	public Rigidbody VRotBody;
	public Transform eyes;

	public float maxHTorque = 1.0f;
	public float maxVTorque = 1.0f;

	public float maxForce = 1.0f;

	[Header("Wing Blow Settings")]
	public AnimationCurve forceProfile;
	bool canBlow = true;
	bool blowing = false;
	float lastBlowTime;
	float profileDuration;

	float RHorizontal;
	float RVertical;
	float LHorizontal;
	float LVertical;
	float RTrigger;
	float LTrigger;


	// Use this for initialization
	void Awake() {
		
		body = GetComponent<Rigidbody> ();
	}

	void Start () {
		profileDuration = forceProfile.keys [forceProfile.length - 1].time;
		print (profileDuration);
	}
	
	// Update is called once per frame
	void Update () {
		
		RHorizontal = Input.GetAxis ("RHorizontal");
		RVertical = Input.GetAxis ("RVertical");
		LHorizontal = Input.GetAxis ("LHorizontal");
		LVertical = Input.GetAxis ("LVertical");
		RTrigger = Input.GetAxis ("RTrigger");
		LTrigger = Input.GetAxis ("LTrigger");

		eyes.transform.rotation = HRotBody.transform.rotation * VRotBody.transform.rotation;


	}

	void FixedUpdate() {

		HRotBody.AddRelativeTorque (Vector3.up * RHorizontal * maxHTorque);
		VRotBody.AddRelativeTorque (Vector3.right * RVertical * maxVTorque);

		IcarusFlight ();
	}

	void BasicFlight() {
		
		body.AddForce (
			HRotBody.transform.right * LHorizontal * maxForce
			+ HRotBody.transform.forward * LVertical * maxForce 
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
			}
		}
	}
}

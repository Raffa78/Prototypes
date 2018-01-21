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
		
	}
	
	// Update is called once per frame
	void Update () {
		
		RHorizontal = Input.GetAxis ("RHorizontal");
		RVertical = Input.GetAxis ("RVertical");
		LHorizontal = Input.GetAxis ("LHorizontal");
		LVertical = Input.GetAxis ("LVertical");
		RTrigger = Input.GetAxis ("RTrigger");
		LTrigger = Input.GetAxis ("LTrigger");

		print (LHorizontal + " " + LVertical + " " + RTrigger + " " + LTrigger);

		eyes.transform.rotation = HRotBody.transform.rotation * VRotBody.transform.rotation;


	}

	void FixedUpdate() {

		HRotBody.AddRelativeTorque (Vector3.up * RHorizontal * maxHTorque);
		VRotBody.AddRelativeTorque (Vector3.right * RVertical * maxVTorque);

		body.AddForce (
			HRotBody.transform.right * LHorizontal * maxForce
			+ HRotBody.transform.forward * LVertical * maxForce 
			+ Vector3.up * RTrigger * maxForce
			- Vector3.up * LTrigger * maxForce
		);
	}

}

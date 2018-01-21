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

	float RHorizontal;
	float RVertical;

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

		eyes.transform.rotation = HRotBody.transform.rotation * VRotBody.transform.rotation;
	}

	void FixedUpdate() {

		HRotBody.AddRelativeTorque (Vector3.up * RHorizontal * maxHTorque);
		VRotBody.AddRelativeTorque (Vector3.right * RVertical * maxVTorque);
	}

}

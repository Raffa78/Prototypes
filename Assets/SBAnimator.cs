using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SBAnimator : MonoBehaviour {

	float runAnimSpeed = 5.0f;

	Animator anim;
	Rigidbody body;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator>();
		body = GetComponentInParent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
		anim.SetFloat("Forward", body.velocity.magnitude / runAnimSpeed, 0.0f, Time.deltaTime);

		Vector3 fw = body.velocity;
		fw.y = 0;
		transform.rotation = Quaternion.LookRotation(fw);
	}
}

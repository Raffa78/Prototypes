using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SBAnimator : MonoBehaviour {

	float runAnimSpeed = 5.0f;
	Quaternion lastRotation;

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

		if(fw != Vector3.zero)
		{
			transform.rotation = Quaternion.LookRotation(fw);
			lastRotation = transform.rotation;
		}
		else
		{
			transform.rotation = lastRotation;
		}
	}

	public void PlayPunch()
	{
		anim.SetTrigger("Punch");
	}

	public void StopPunch()
	{
		anim.SetTrigger("EndPunch");
	}
}

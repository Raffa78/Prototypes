using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SBAnimator : MonoBehaviour {

	float runAnimSpeed = 5.0f;
	Quaternion lastRotation;

	Animator anim;
	Rigidbody body;

	bool punched;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator>();
		body = GetComponentInParent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {

		if(punched)
		{
			return;
		}

		anim.SetFloat("Forward", body.velocity.magnitude / runAnimSpeed, 0.0f, Time.deltaTime);

		Vector3 fw = body.velocity;
		fw.y = 0;

		if (fw.magnitude > 0.1f)
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

	public void PlayPunched()
	{
		punched = true;
		anim.SetTrigger("Punched");
	}

	public void StopPunched()
	{
		punched = false;
		anim.SetTrigger("EndPunched");
	}
}

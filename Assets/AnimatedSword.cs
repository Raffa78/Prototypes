using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedSword : MonoBehaviour {
	Animator animator;
	public Rigidbody playerBody;

	public float swingCooldown = 0.0f;
	float swingTime;
	bool swingAlreadyHit = false;

	public float hitForce;
	public float hitRecoilForceRatio;

	void Awake() {

		swingTime = -Mathf.Infinity;
		animator = GetComponent<Animator> ();

		animator.GetBehaviour<SwordSMB> ().onIdle.AddListener (OnIdle);
		animator.GetBehaviour<SwordSMB> ().onSwingBack.AddListener (OnSwingBack);
	}

	void Update () {
		
		if(Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Fire1"))
		{
			if(!SwingInCooldown())
			{
				animator.SetTrigger ("Swing");		
				swingTime = Time.time;
				swingAlreadyHit = false;
			}

		}	

	}

	void OnSwingBack()
	{
		
	}

	void OnIdle()
	{
	}

	void OnTriggerEnter(Collider collider)
	{
		if (swingAlreadyHit) {
			return;
		}

		Vector3 impulse = collider.attachedRigidbody.position - playerBody.position;
		impulse.Normalize ();

		collider.attachedRigidbody.AddForce (hitForce * impulse, ForceMode.Impulse);
		playerBody.AddForce (-hitRecoilForceRatio * hitForce * impulse, ForceMode.Impulse);

		swingAlreadyHit = true;
	}

	bool SwingInCooldown()
	{
		return Time.time < (swingTime + swingCooldown);
	}
}

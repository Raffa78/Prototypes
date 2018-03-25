using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedSword : MonoBehaviour {
	Animator animator;
	public Rigidbody playerBody;

	bool swingAlreadyHit = false;

	public float hitForce;
	public float hitRecoilForceRatio;

	bool swingInCooldown = false;

	void Awake() {
		
		animator = GetComponent<Animator> ();

		animator.GetBehaviour<SwordSMB> ().onIdle.AddListener (OnIdle);
		animator.GetBehaviour<SwordSMB> ().onSwingBack.AddListener (OnSwingBack);

		GetComponentInChildren<Collider> ().enabled = false;
	}

	void Update () {
		
		if(Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Fire1"))
		{
			if(!swingInCooldown)
			{
				animator.SetTrigger ("Swing");	
				swingAlreadyHit = false;
				swingInCooldown = true;
				GetComponentInChildren<Collider> ().enabled = true;
			}
		}	
	}

	void OnSwingBack()
	{
		GetComponentInChildren<Collider> ().enabled = false;
	}

	void OnIdle()
	{
		swingInCooldown = false;
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

}

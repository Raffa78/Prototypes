using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedSword : MonoBehaviour {
	PhotonView photonView;
	Animator animator;
	public Rigidbody playerBody;

	bool swingAlreadyHit = false;

	public float hitForce;
	public float hitRecoilForceRatio;

	bool swingInCooldown = false;

	public AudioClip hitClip;
	public AudioClip beingHitClip;

	void Awake() {

		photonView = GetComponent<PhotonView> ();

		if (photonView == null)
			return;

		if (!photonView.isMine) {
			enabled = false;
			return;
		}
		
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
				PhotonNetwork.RPC (photonView, "SetSwingTriggerToProxy", PhotonTargets.Others, false, null);
			}
		}	
	}

	[PunRPC]
	void SetSwingTriggerToProxy()
	{
		transform.parent.parent.Find ("BodyProxy").GetComponentInChildren<Animator> ().SetTrigger ("Swing");
	}	

	void OnSwingBack()
	{
		GetComponentInChildren<Collider> ().enabled = false;
	}

	void OnIdle()
	{
		
		StartCoroutine (SwingCooldown());
	}

	IEnumerator SwingCooldown()
	{
		yield return new WaitForSeconds (1.8f);
		swingInCooldown = false;
	}

	void OnTriggerEnter(Collider collider)
	{
		if (swingAlreadyHit) {
			return;
		}

		if (collider.attachedRigidbody == null)
			return;

		if (collider.attachedRigidbody.name != "BodyProxy")
			return;
		
		AudioSource.PlayClipAtPoint (hitClip, transform.position);

		Vector3 impulse = collider.attachedRigidbody.position - playerBody.position;
		impulse.Normalize ();
				
		collider.attachedRigidbody.AddForce (hitForce * impulse, ForceMode.Impulse);

		int id = collider.attachedRigidbody.transform.parent.Find ("Body").GetComponent<PhotonView> ().viewID;
		photonView.RPC ("SwordHit", PhotonTargets.Others, id, impulse);
		//collider.attachedRigidbody.AddForce (hitForce * impulse, ForceMode.Impulse);
		//playerBody.AddForce (-hitRecoilForceRatio * hitForce * impulse, ForceMode.Impulse);

		swingAlreadyHit = true;
	}


	public int countToDeath = 3;

	[PunRPC]
	public void SwordHit(int ID, Vector3 impulse)
	{
		Rigidbody body = PhotonView.Find (ID).GetComponent<Rigidbody>();
		body.AddForce (hitForce * impulse, ForceMode.Impulse);

		AudioSource.PlayClipAtPoint (beingHitClip, body.position);

		body.GetComponentInChildren<AnimatedSword>().countToDeath--;

		if (body.GetComponentInChildren<AnimatedSword>().countToDeath == 0) 
		{
			PhotonNetwork.Destroy (body.GetComponentInParent<DualStickFly> ().gameObject);
			Vector3 position = GameObject.Find("Spawn").transform.position;
			GameObject newPlayerObject = PhotonNetwork.Instantiate( "TestPlayer", position, Quaternion.identity, 0 );
		}
	}

	void FixedUpdate()
	{
		
	}
}

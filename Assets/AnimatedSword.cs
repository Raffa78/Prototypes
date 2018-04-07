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

	bool canSwing = true;

	public AudioClip hitClip;
	public AudioClip beingHitClip;

	public AnimationCurve motorCurve;

	public TrailRenderer swordTrail;

	public GameObject hitPrefab;

	public GameObject collidersForHit;

	void Awake() {

		swordTrail.enabled = false;

		photonView = GetComponent<PhotonView> ();

		if (photonView == null)
			return;

		if (!photonView.isMine) {
			enabled = false;
			return;
		}
		
		animator = GetComponent<Animator> ();

		animator.GetBehaviour<SwordSMB> ().onSwing.AddListener(OnSwing);
		animator.GetBehaviour<SwordSMB> ().onIdle.AddListener (OnIdle);
		animator.GetBehaviour<SwordSMB> ().onSwingBack.AddListener (OnSwingBack);

		if (collidersForHit != null)
			collidersForHit.SetActive(false);
	}

	void Update () {

		if (transform.parent.parent.parent.name == "BodyProxy")
			return;

		if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Fire1"))
		{
			if(canSwing)
			{
				swingAlreadyHit = false;
				canSwing = false;

				if (collidersForHit != null)
					collidersForHit.SetActive(true);

				animator.SetTrigger ("Swing");	
				PhotonNetwork.RPC (photonView, "SetSwingTriggerToProxy", PhotonTargets.Others, false, null);
			}
		}	
	}

	[PunRPC]
	void SetSwingTriggerToProxy()
	{
		transform.parent.parent.parent.Find ("BodyProxy").GetComponentInChildren<Animator> ().SetTrigger ("Swing");
	}	

	void OnSwing()
	{
		swordTrail.enabled = true;
	}

	void OnSwingBack()
	{
		if (collidersForHit != null)
			collidersForHit.SetActive(false);
	}

	void OnIdle()
	{
		swordTrail.enabled = false;
		if (collidersForHit != null)
			collidersForHit.SetActive(false);
		StartCoroutine (SwingCooldown());
	}

	IEnumerator SwingCooldown()
	{
		yield return new WaitForSeconds (1.8f);
		canSwing = true;
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
		Instantiate(hitPrefab, collider.attachedRigidbody.position, Quaternion.identity);

		print("entered");
		Vector3 impulse = collider.attachedRigidbody.position - playerBody.position;
		impulse.Normalize ();
				
		//Disable motors of bodyProxy on big collisions so to have a sort of client prediction
		lastHitJoint = collider.attachedRigidbody.GetComponent<ConfigurableJoint>();
		JointDrive drive = new JointDrive();
		drive.positionSpring = 0f;
		drive.positionDamper = 0f;
		drive.maximumForce = lastHitJoint.xDrive.maximumForce;
		lastHitJoint.xDrive = drive;
		lastHitJoint.yDrive = drive;
		lastHitJoint.zDrive = drive;
		//lastHitJoint.angularXDrive = drive;
		//lastHitJoint.angularYZDrive = drive;

		StartCoroutine (EnableProxyMotors ());

		collider.attachedRigidbody.AddForce (hitForce * impulse, ForceMode.Impulse);

		int id = collider.attachedRigidbody.transform.parent.Find ("Body").GetComponent<PhotonView> ().viewID;
		photonView.RPC ("SwordHit", PhotonTargets.Others, id, impulse);
		//collider.attachedRigidbody.AddForce (hitForce * impulse, ForceMode.Impulse);
		//playerBody.AddForce (-hitRecoilForceRatio * hitForce * impulse, ForceMode.Impulse);

		if (collidersForHit != null)
			collidersForHit.SetActive(false);

		swingAlreadyHit = true;
	}

	ConfigurableJoint lastHitJoint;

	float reenableMotorsDuration = 4.0f;
	float spring = 1000f;
	float damper = 100f;

	IEnumerator EnableProxyMotors()
	{
		if (lastHitJoint == null)
			yield break ;

		float time = 0; ;
		
		while(time < reenableMotorsDuration)
		{
			JointDrive drive = new JointDrive();
			float lerpedSpring = Mathf.Lerp(0, spring, motorCurve.Evaluate(time / reenableMotorsDuration));
			float lerpedDamper = Mathf.Lerp(0, damper, motorCurve.Evaluate(time / reenableMotorsDuration));
			drive.positionSpring = lerpedSpring;
			drive.positionDamper = lerpedDamper;
			drive.maximumForce = lastHitJoint.xDrive.maximumForce;
			lastHitJoint.xDrive = drive;
			lastHitJoint.yDrive = drive;
			lastHitJoint.zDrive = drive;
			//lastHitJoint.angularXDrive = drive;
			//lastHitJoint.angularYZDrive = drive;

			time += Time.deltaTime;
			yield return null;
		}
	}


	public int countToDeath = 30;

	[PunRPC]
	public void SwordHit(int ID, Vector3 impulse)
	{
		Rigidbody body = PhotonView.Find (ID).GetComponent<Rigidbody>();
		body.AddForce (hitForce * impulse, ForceMode.Impulse);

		AudioSource.PlayClipAtPoint (beingHitClip, body.position);
		Instantiate(hitPrefab, body.position, Quaternion.identity);

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puncher : MonoBehaviour {

	public AudioClip hitClip;
	public AudioClip beingHitClip;
	public GameObject hitPrefab;
	public GameObject beingHitPrefab;

	float punchForce = 180.0f;

	Collider punchCollider;
	PhotonView pv;

	// Use this for initialization
	void Start () {
		punchCollider = GetComponent<Collider>();
		punchCollider.enabled = false;

		pv = GetComponent<PhotonView>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void OnTriggerEnter(Collider other)
	{
        if (other.transform.parent == null ) {
            return;
        }
		GameObject bodyProxy = other.transform.parent.gameObject;

		if(bodyProxy.name != "BodyProxy")
		{
			print(bodyProxy.name);
			return;
		}

		AudioSource.PlayClipAtPoint(hitClip, transform.position);
		Instantiate(hitPrefab, transform.position, Quaternion.identity);

		bodyProxy.transform.Find("KinematicBodyProxy").gameObject.SetActive(false);

		Vector3 hitForce = transform.parent.GetComponentInChildren<SBAnimator>().transform.forward;
		hitForce.y = 0.4f;
		hitForce.Normalize();
		hitForce *= punchForce;

		Rigidbody hitBody = bodyProxy.GetComponent<Rigidbody>();
		hitBody.AddForce(hitForce, ForceMode.Impulse);

		int id = bodyProxy.transform.parent.Find("Body").GetComponent<PhotonView>().viewID;
		pv.RPC("PunchOther", PhotonTargets.Others, id, hitForce, transform.position);

		bodyProxy.transform.parent.GetComponent<NinjasPlayer>().GetPunched();
	}

	[PunRPC]
	public void PunchOther(int ID, Vector3 hitForce, Vector3 hitPosition)
	{
		PhotonView pv = PhotonView.Find(ID);
		Rigidbody body = pv.GetComponent<Rigidbody>();
		body.AddForce(hitForce, ForceMode.Impulse);

		pv.GetComponentInParent<NinjasPlayer>().GetPunched();
		
		AudioSource.PlayClipAtPoint(beingHitClip, hitPosition);
		Instantiate(beingHitPrefab, hitPosition, Quaternion.identity);
		
		//PhotonNetwork.RPC(photonView, "HitRoundTrip", PhotonTargets.Others, false, null);
	}

	public void EnablePunch()
	{
		punchCollider.enabled = true;
	}

	public void DisablePunch()
	{
		punchCollider.enabled = false;
	}
}

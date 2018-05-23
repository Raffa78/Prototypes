using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puncher : MonoBehaviour {

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
		GameObject bodyProxy = other.transform.parent.gameObject;

		if(bodyProxy.name != "BodyProxy")
		{
			print(bodyProxy.name);
			return;
		}

		bodyProxy.transform.Find("KinematicBodyProxy").gameObject.SetActive(false);

		Vector3 hitForce = transform.parent.GetComponentInChildren<SBAnimator>().transform.forward;
		hitForce.y = 0.4f;
		hitForce.Normalize();
		hitForce *= punchForce;

		Rigidbody hitBody = bodyProxy.GetComponent<Rigidbody>();
		hitBody.AddForce(hitForce, ForceMode.Impulse);

		int id = bodyProxy.transform.parent.Find("Body").GetComponent<PhotonView>().viewID;
		pv.RPC("PunchOther", PhotonTargets.Others, id, hitForce);

		bodyProxy.transform.parent.GetComponent<SBPlayer>().GetPunched();
	}

	[PunRPC]
	public void PunchOther(int ID, Vector3 hitForce)
	{
		PhotonView pv = PhotonView.Find(ID);
		Rigidbody body = pv.GetComponent<Rigidbody>();
		body.AddForce(hitForce, ForceMode.Impulse);

		pv.GetComponentInParent<SBPlayer>().GetPunched();

		/*
		AudioSource.PlayClipAtPoint(beingHitClip, body.position);
		Instantiate(hitPrefab, body.position, Quaternion.identity);

		body.GetComponentInChildren<AnimatedSword>().countToDeath--;

		if (body.GetComponentInChildren<AnimatedSword>().countToDeath == 0)
		{
			PhotonNetwork.Destroy(body.GetComponentInParent<DualStickFly>().gameObject);
			Vector3 position = GameObject.Find("Spawn").transform.position;
			GameObject newPlayerObject = PhotonNetwork.Instantiate("TestPlayer", position, Quaternion.identity, 0);
		}
		*/
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

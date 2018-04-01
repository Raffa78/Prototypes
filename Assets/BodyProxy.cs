using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyProxy : MonoBehaviour {

	PhotonView photonView;
	// Use this for initialization
	void Awake() {
		photonView = GetComponent<PhotonView> ();

		if (photonView.isMine) {
			GetComponent<MeshRenderer> ().enabled = false;
			transform.GetChild (0).gameObject.SetActive (false);
		} else {
			GetComponent<FollowTransform> ().enabled = false;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

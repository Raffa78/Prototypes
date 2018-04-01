using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeactivateIfNotOwned : MonoBehaviour {

	PhotonView photonView;

	// Use this for initialization
	void Start () {
		
		photonView = GetComponent<PhotonView> ();
		if (!photonView.isMine)
			gameObject.SetActive (false);
	}
}

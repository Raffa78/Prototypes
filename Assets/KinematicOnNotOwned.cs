using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinematicOnNotOwned : MonoBehaviour {

	PhotonView photonView;

	// Use this for initialization
	void Start () {
		photonView = GetComponent<PhotonView>();

		if (!photonView.isMine)
			GetComponent<Rigidbody>().isKinematic = true;
	}
	
}

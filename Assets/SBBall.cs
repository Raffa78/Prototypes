
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SBBall : MonoBehaviour {

	PhotonView photonView;
	
	// Use this for initialization
	void Start () {
		photonView = GetComponent<PhotonView>();
		
	}
}

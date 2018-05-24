using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawn : MonoBehaviour {

	void Start()
	{
		

	}
	// Use this for initialization
	void OnJoinedRoom () {

		Vector3 position = GameObject.Find("Spawn").transform.position;
		PhotonNetwork.Instantiate( "TestPlayer", position, Quaternion.identity, 0 );

		//PhotonNetwork.sendRate = 30;
		//PhotonNetwork.sendRateOnSerialize = 30; 
	}

}

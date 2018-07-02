using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NinjasSpawn : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


	void OnJoinedRoom()
	{

		SpawnPlayer();
		
		//PhotonNetwork.sendRate = 30;
		//PhotonNetwork.sendRateOnSerialize = 30; 
	}

	void SpawnPlayer()
	{
		Vector3 position = GameObject.Find("Spawn").transform.position;
		PhotonNetwork.Instantiate("NinjasPlayer", position, Quaternion.identity, 0);
	}
}

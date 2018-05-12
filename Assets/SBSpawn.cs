using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SBSpawn : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


	void OnJoinedRoom()
	{

		Vector3 position = GameObject.Find("Spawn").transform.position;
		GameObject newPlayerObject = PhotonNetwork.Instantiate("SBPlayer", position, Quaternion.identity, 0);

		//PhotonNetwork.sendRate = 30;
		//PhotonNetwork.sendRateOnSerialize = 30; 
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawn : MonoBehaviour {

	static 
	// Use this for initialization
	void OnJoinedRoom () {
		
		Vector3 position = new Vector3( Random.value * 33.5f, 0.0f, 3.0f );
		GameObject newPlayerObject = PhotonNetwork.Instantiate( "TestPlayer", position, Quaternion.identity, 0 );
	}

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawn : MonoBehaviour {

	void Start()
	{
		

	}
	// Use this for initialization
	void OnJoinedRoom () {
		PhotonNetwork.SetSendingEnabled ((byte)1, true);
		PhotonNetwork.SetInterestGroups ((byte)1, true);

		Vector3 position = new Vector3( Random.value * 33.5f, 0.0f, 3.0f );
		GameObject newPlayerObject = PhotonNetwork.Instantiate( "TestPlayer", position, Quaternion.identity, (byte)1 );
	}

}

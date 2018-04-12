
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class NetBall : MonoBehaviour {

	PhotonView photonView;
	
	// Use this for initialization
	void Start () {
		photonView = GetComponent<PhotonView>();

		if(!photonView.isMine)
		{
			foreach(NetBall ball in GameObject.FindObjectsOfType<NetBall>())
			{
				ConfigurableJoint joint = ball.GetComponent<ConfigurableJoint>();
				if (joint != null)
				{
					joint.connectedBody = GetComponent<Rigidbody>();
				}
			}
		}
		else
		{
			Destroy(GetComponent<ConfigurableJoint>());
		}	
	}
}

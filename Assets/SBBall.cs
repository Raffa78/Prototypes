
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SBBall : MonoBehaviour {

	PhotonView photonView;
	
	// Use this for initialization
	void Start () {
		photonView = GetComponent<PhotonView>();
		transform.parent = null;

		if (!PhotonNetwork.player.IsMasterClient && photonView.isMine)
		{
			Destroy(gameObject);
		}	
	}

	public void TakeOver()
	{
		photonView.TransferOwnership(PhotonNetwork.player.ID);
	}

	public void RequestOwnership()
	{
		photonView.RequestOwnership();
	}

	public bool IsMine()
	{
		return photonView.isMine;
	}
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SBBall : MonoBehaviour, IPunObservable
{

	PhotonView photonView;
	Rigidbody myRigidbody;
	bool isCatched;
	bool takingOver;

	// Use this for initialization
	IEnumerator Start() {
		myRigidbody = GetComponent<Rigidbody>();
		photonView = GetComponent<PhotonView>();
		transform.parent = null;

		if ((!PhotonNetwork.player.IsMasterClient && photonView.isMine) || (PhotonNetwork.player.IsMasterClient && !photonView.isMine))
		{
			yield return new WaitForSeconds(0.5f);
			Destroy(gameObject);
			yield break;
		}

		if (!photonView.isMine)
		{
			myRigidbody.isKinematic = true;
		}

	}

	public void Update()
	{
		if (!photonView.isMine)
		{
			myRigidbody.isKinematic = true;
			return;
		}

		if (isCatched)
		{
			myRigidbody.isKinematic = true;
		}
		else
		{
			myRigidbody.isKinematic = false;
		}
	}

	public void TakeOver()
	{
		takingOver = true;
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

	public bool IsCatched()
	{
		return isCatched;
	}

	public void Catch()
	{
		isCatched = true;
	}

	public void Uncatch()
	{
		isCatched = false;
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if(stream.isWriting)
		{
			print(Time.frameCount + ": stream is writing");
			takingOver = false;
			stream.SendNext(isCatched);
		}
		else
		{
			if(!takingOver)
			{
				print(Time.frameCount + ": stream is reading and not taking over");
				isCatched = (bool)stream.ReceiveNext();
			}
			else
			{
				print(Time.frameCount + ": stream is reading and taking over");
			}
		}

	}
}

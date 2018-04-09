using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetInfos : MonoBehaviour {

	PhotonView photonView;
	Text pingText;
	// Use this for initialization
	void Start () {
		photonView = GetComponent<PhotonView>();

		if (!photonView.isMine)
		{
			gameObject.SetActive(false);
			return;
		}

		pingText = transform.Find("Ping").GetComponent<Text>();

	}
	
	// Update is called once per frame
	void Update () {
		if (!photonView.isMine)
			return;

		pingText.text = PhotonNetwork.GetPing().ToString();
	}
}

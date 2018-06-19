using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SBGoal : MonoBehaviour {

	Text text;
	float score;
	PhotonView pv;

	// Use this for initialization
	void Start () {
		pv = GetComponent<PhotonView>();
		text = GetComponentInChildren<Text>();
		text.text = "0";
		score = 0;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void OnTriggerEnter(Collider other)
	{
		SBBall ball = other.GetComponentInParent<SBBall>();

		if(ball == null)
		{
			return;
		}

		if(!ball.GetComponent<PhotonView>().isMine)
		{
			return;
		}

		pv.RPC("Score", PhotonTargets.All);

		DeployBall(ball);
	}

	[PunRPC]
	void Score()
	{
		score++;
		text.text = score.ToString();
	}

	void DeployBall(SBBall ball)
	{
		ball.transform.position = new Vector3(0.0f, 10.0f, 0.0f);
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchBall : MonoBehaviour {

	public Transform ballSocket;
	Rigidbody ballBody;
	Rigidbody body;

	public float throwSpeed;

	// Use this for initialization
	void Start () {
		body = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
		if(ballBody == null)
		{
			return;
		}

		ballBody.transform.position = ballSocket.position;
		ballBody.transform.rotation = ballSocket.rotation;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if(ballBody != null)
		{
			return;
		}

		if(collision.transform.name == "Ball")
		{
			SBBall ball = collision.transform.GetComponent<SBBall>();

			if(!ball.IsMine())
			{
				ball.TakeOver();
			}

			ballBody = collision.rigidbody;
			collision.rigidbody.isKinematic = true;
			collision.transform.parent = null;
			collision.transform.localPosition = ballSocket.position;
			collision.transform.localRotation = ballSocket.rotation;
		}
	}

	public void ThrowBall(Vector3 vel)
	{
		if(ballBody == null)
		{
			return;
		}

		ballBody.isKinematic = false;
		ballBody.velocity = vel;
		ballBody = null;
	}

	public bool HasBall()
	{
		return ballBody != null;
	}
}

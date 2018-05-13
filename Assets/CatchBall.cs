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
			collision.transform.parent = ballSocket;
			collision.transform.localPosition = Vector3.zero;
			collision.transform.localRotation = Quaternion.identity;
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

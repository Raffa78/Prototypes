using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchBall : MonoBehaviour
{

	public Transform ballSocket;
	Rigidbody ballBody;
	Rigidbody body;

	public float dropSpeed = 10.0f;

	// Use this for initialization
	void Start()
	{
		body = GetComponent<Rigidbody>();
	}

	// Update is called once per frame
	void Update()
	{
		print(ballBody);

		if (ballBody == null)
		{
			return;
		}

		ballBody.transform.position = ballSocket.position;
		ballBody.transform.rotation = ballSocket.rotation;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (ballBody != null)
		{
			return;
		}

		if (other.transform.parent == null)
		{
			return;
		}

		if (other.transform.parent.name == "Ball")
		{
			SBBall ball = other.transform.parent.GetComponent<SBBall>();

			if (!ball.IsMine())
			{
				ball.TakeOver();
			}

			ball.Catch();
			ballBody = other.transform.parent.GetComponent<Rigidbody>();
			ballBody.isKinematic = true;
			other.transform.parent.parent = null;
			other.transform.parent.localPosition = ballSocket.position;
			other.transform.parent.localRotation = ballSocket.rotation;
		}
	}

	public void ThrowBall(Vector3 vel)
	{
		if (ballBody == null)
		{
			return;
		}

		ballBody.GetComponent<SBBall>().Uncatch();

		ballBody.isKinematic = false;
		ballBody.velocity = vel;
		ballBody = null;
	}

	public bool HasBall()
	{
		return ballBody != null;
	}

	public void DropBall()
	{
		if(ballBody == null)
		{
			return;
		}

		ballBody.GetComponent<SBBall>().Uncatch();

		ballBody.isKinematic = false;
		ballBody.velocity = Vector3.up * dropSpeed;
		ballBody = null;

	}
}

﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DualStickFly : Photon.MonoBehaviour, IPunObservable {

	Transform bodyObject;
	Rigidbody body;
	Rigidbody bodyPuller;

	Transform bodyProxy;

	public Rigidbody HRotBody;
	public Rigidbody VRotBody;
	public Transform eyes;

	public float maxHTorque = 1.0f;
	public float maxVTorque = 1.0f;

	public float maxForce = 1.0f;

	[Header("Wing Settings")]
	public GameObject LeftWing; 
	public GameObject RightWing;
	public AnimationCurve forceProfile;
	public float blowWingMaxAngle = 45;
	bool canBlow = true;
	bool blowing = false;
	float lastBlowTime;
	float profileDuration;
	float[] integralCurve;

	float RHorizontal;
	float RVertical;
	float LHorizontal;
	float LVertical;
	float RTrigger;
	float LTrigger;

	PhotonView m_PhotonView;

	// Use this for initialization
	void Awake() {
		
		m_PhotonView = GetComponent<PhotonView>();
	}

	IEnumerator Start () {
		profileDuration = forceProfile.keys [forceProfile.length - 1].time;
		IntegrateCurve ();

		bodyObject = transform.Find ("Body");
		body = bodyObject.GetComponent<Rigidbody> ();

		bodyProxy = transform.Find ("BodyProxy");

		eyes = /*transform.Find ("BodyPuller").*/bodyObject.transform.Find ("Eyes");

		bodyPuller = transform.Find ("BodyPuller").GetComponent<Rigidbody> ();

		if (!m_PhotonView.isMine) {

			

			//our rigidbody replica is moved cinematically via Photon Transform View
			body.isKinematic = true;
			bodyPuller.gameObject.SetActive (false);

			//Disable all mesh renderer children. We don't want to see the other players exact replica, but bodyProxy instead
			Array.ForEach<MeshRenderer>(bodyObject.GetComponentsInChildren<MeshRenderer> (), x => x.enabled = false);
			//Disable colliders also. bodyProxy is jointed to replica and it is our physic representation of other players
			Array.ForEach<Collider>(bodyObject.GetComponentsInChildren<Collider> (), x => x.enabled = false);

			Array.ForEach<TrailRenderer>(bodyObject.GetComponentsInChildren<TrailRenderer> (), x => x.enabled = false);

			//Disable Camera of other players' replicas
			eyes.gameObject.SetActive (false);

			yield return new WaitForSeconds(0.5f);

			Rigidbody rb = transform.Find("BodyProxy").GetComponent<Rigidbody>();
			rb.isKinematic = true;
			rb.position = transform.Find("Body").position;
			rb.rotation = transform.Find("Body").rotation;
			rb.isKinematic = false;

			

		} else {
			print ("net: " + PhotonNetwork.sendRate + " " + PhotonNetwork.sendRateOnSerialize);

			Destroy (bodyProxy.gameObject);
		}

		yield break;
	}

	void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		
	}
	
	// Update is called once per frame
	void Update () {

		if (!m_PhotonView.isMine)
		{
			if(Input.GetKeyDown(KeyCode.Space))
			{
				Array.ForEach<MeshRenderer>(
					bodyObject.GetComponentsInChildren<MeshRenderer>(), x => x.enabled = !x.enabled);
			}
			return;
		}
		
		/*
		RHorizontal = Input.GetAxis ("RHorizontal");
		RVertical = Input.GetAxis ("RVertical");
		LHorizontal = Input.GetAxis ("LHorizontal");
		LVertical = Input.GetAxis ("LVertical");
		RTrigger = Input.GetAxis ("RTrigger");
		LTrigger = Input.GetAxis ("LTrigger");

		if (Input.GetKey (KeyCode.A))
			LHorizontal = -1;
		if (Input.GetKey (KeyCode.D))
			LHorizontal = 1;
		if (Input.GetKey (KeyCode.W))
			LVertical = 1;
		if (Input.GetKey (KeyCode.S))
			LVertical = -1;
			*/

		eyes.transform.rotation = HRotBody.transform.rotation * VRotBody.transform.rotation;

		bodyPuller.rotation = HRotBody.transform.rotation;




	}

	void FixedUpdate() {

		if (photonView.isMine)
		{

			bodyPuller.AddForce(
				bodyObject.right * LHorizontal * maxForce
				+ bodyObject.forward * LVertical * maxForce
				+ Vector3.up * RTrigger * maxForce
				- Vector3.up * LTrigger * maxForce
			);

			HRotBody.AddRelativeTorque(Vector3.up * RHorizontal * maxHTorque);
			VRotBody.AddRelativeTorque(Vector3.right * RVertical * maxVTorque);
		}
		else
		{
			bodyProxy.GetComponent<Rigidbody>().AddForce(
				bodyObject.right * LHorizontal * maxForce
				+ bodyObject.forward * LVertical * maxForce
				+ Vector3.up * RTrigger * maxForce
				- Vector3.up * LTrigger * maxForce
			);
		}	
		
		
		//IcarusFlight ();
	}
	

	void IcarusFlight() {

		
		Vector3 force = HRotBody.transform.right * LHorizontal
		                + HRotBody.transform.forward * LVertical
		                + Vector3.up * RTrigger
		                - Vector3.up * LTrigger;

		if (force.magnitude > 1.0f) {
			force.Normalize ();
		}

		if (canBlow && force.magnitude > 0.1) {
			
			canBlow = false;
			blowing = true;
			lastBlowTime = Time.time;
		}
			
		if (blowing) {
			
			float t = Time.time - lastBlowTime;

			if (t > profileDuration) {
				
				blowing = false;
				canBlow = true;

			} else {

				body.AddForce(force * maxForce * forceProfile.Evaluate(t));
				LeftWing.transform.localEulerAngles = new Vector3(0, 0, blowWingMaxAngle * Integral(t));
				RightWing.transform.localEulerAngles = new Vector3(0, 0, -blowWingMaxAngle * Integral(t));
			}
		}
	}

	public void IntegrateCurve()
	{
		float x_high = profileDuration;
		float x_low = 0.0f;
		float N_steps = 50.0f;

		float h = (x_high - x_low) / N_steps;
		float res = 0.0f;

		integralCurve = new float[(int)N_steps];
		float max = 0.0f;

		for (int i = 0; i < N_steps; i++)
		{
			res += forceProfile.Evaluate(x_low + i * h);

			if (max < res * h)
				max = res * h;
			
			integralCurve [i] = res * h;
		}

		//normalize to max
		for (int i = 0; i < N_steps; i++) {

			integralCurve [i] = integralCurve [i] / max;
		}

		return;
	}

	float Integral(float time){
		
		float N_steps = integralCurve.Length;
		float x = time * N_steps / profileDuration;

		int x_low = Mathf.FloorToInt (x);
		int x_high = Mathf.CeilToInt (x);

		if (x_low != x_high) {
			
			return Mathf.Lerp (
				integralCurve [x_low],
				integralCurve [x_high],
				x - x_low);
			
		}

		return integralCurve [x_high];
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{

		if (stream.isWriting) {

			RHorizontal = Input.GetAxis ("RHorizontal");
			RVertical = Input.GetAxis ("RVertical");
			LHorizontal = Input.GetAxis ("LHorizontal");
			LVertical = Input.GetAxis ("LVertical");
			RTrigger = Input.GetAxis ("RTrigger");
			LTrigger = Input.GetAxis ("LTrigger");

			stream.SendNext (RHorizontal);
			stream.SendNext (RVertical);
			stream.SendNext (LHorizontal);
			stream.SendNext (LVertical);
			stream.SendNext (RTrigger);
			stream.SendNext (LTrigger);

		} else {
			
			RHorizontal = (float)stream.ReceiveNext ();
			RVertical = (float)stream.ReceiveNext ();
			LHorizontal = (float)stream.ReceiveNext ();
			LVertical = (float)stream.ReceiveNext ();
			RTrigger = (float)stream.ReceiveNext ();
			LTrigger = (float)stream.ReceiveNext ();
		}
	}
}

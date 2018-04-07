using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordRotation : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.rotation = Quaternion.LookRotation(transform.parent.forward, Vector3.up);
	}
}

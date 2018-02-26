using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicSword : MonoBehaviour {

	public ConfigurableJoint swordJoint;
	bool swingRight = true;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetButtonDown("Fire1"))
		{
			if (swingRight) {
				swordJoint.targetRotation = Quaternion.Euler (0, -170, 0);
				swingRight = false;
			} else {
				swordJoint.targetRotation = Quaternion.Euler (0, -10, 0);
				swingRight = true;
			}
		}	
	}
}

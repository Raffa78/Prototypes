using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionEnterPrint : MonoBehaviour {

	void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.gameObject.name.Contains ("Ground"))
			return;
		
	}
}

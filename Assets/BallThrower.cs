using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallThrower : MonoBehaviour {

	public GameObject prefab;
	public float throwVelocity;
	public float throwCooldown;
	public float coneRadius;
	public float coneHeight;

	// Use this for initialization
	IEnumerator Start () {
		while (true) {

			Vector3 dir = (Vector3)(Random.insideUnitCircle) ;
			dir = (coneRadius * dir + coneHeight * Vector3.forward);

			GameObject instance = Instantiate (prefab, transform.position, transform.rotation);
			instance.GetComponent<Rigidbody> ().velocity = throwVelocity * transform.TransformVector (dir.normalized);


			yield return new WaitForSeconds (throwCooldown);
		}
	}
}

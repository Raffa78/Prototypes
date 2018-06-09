using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SprintSlider : MonoBehaviour {

	Slider slider;
	SBPlayer player;

	// Use this for initialization
	void Start () {
		slider = GetComponent<Slider>();
		player = GetComponentInParent<SBPlayer>();

		if(!player.GetComponent<PhotonView>().isMine)
		{
			Destroy(gameObject);
		}
	}
	
	// Update is called once per frame
	void Update () {
		slider.value = player.sprintPool;
	}
}

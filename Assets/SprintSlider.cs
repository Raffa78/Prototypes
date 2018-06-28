using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SprintSlider : MonoBehaviour {

	Slider slider;
	NinjasPlayer player;

	// Use this for initialization
	void Start () {
		slider = GetComponent<Slider>();
		player = GetComponentInParent<NinjasPlayer>();

		if (player == null)
		{
			return;
		}

		if (!player.GetComponent<PhotonView>().isMine)
		{
			Destroy(gameObject);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(player == null)
		{
			return;
		}

		slider.value = player.sprintPool;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StartButton : Button {
	
	SBPlayer player;

	// Use this for initialization
	protected override void Start()
	{
		base.Start();

		player = GetComponentInParent<SBPlayer>();

		if (!player.GetComponent<PhotonView>().isMine)
		{
			Destroy(gameObject);
		}
	}


	public override void OnPointerDown(PointerEventData eventData)
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		Destroy(gameObject);
	}
}

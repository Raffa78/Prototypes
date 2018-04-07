using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SwordSMB : StateMachineBehaviour {

	public UnityEvent onSwing;
	public UnityEvent onSwingBack;
	public UnityEvent onIdle;

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (stateInfo.IsName("SwordSwing"))
			onSwing.Invoke();

		if (stateInfo.IsName("SwordSwingBack"))
			onSwingBack.Invoke();

		if (stateInfo.IsName ("SwordIdle"))
			onIdle.Invoke ();
	}
}

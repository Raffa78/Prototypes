using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.IO;
using UnityEditor;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class ParrotPlayer : MonoBehaviour
{
	Queue<KeyLog> recording;
	HashSet<KeyCode> keysDown = new HashSet<KeyCode> ();
	int gameViewLeft;
	int gameViewBottom;

	//[HideInInspector]
	public string path;
	float timeStartedPlaying;
	InputWrapper inputSim = new InputWrapper ();

	// Use this for initialization
	void Start ()
	{
		DontDestroyOnLoad (this);
		if (path != null && File.Exists (path)) {
			Debug.Log ("Starting macro at path " + path);
			IFormatter formatter = new BinaryFormatter ();
			FileStream fileStream = new FileStream (path, FileMode.Open);
			recording = (Queue<KeyLog>)formatter.Deserialize (fileStream);
			fileStream.Close ();

			int[] mousePosInWholeScreen = inputSim.GetMousePos ();
			gameViewLeft = mousePosInWholeScreen [0] - (int)Mathf.Round (Input.mousePosition.x);
			gameViewBottom = mousePosInWholeScreen [1] + (int)Mathf.Round (Input.mousePosition.y);

			timeStartedPlaying = Time.time;
		}
	}

	// Update is called once per frame
	void Update ()
	{
		if (recording != null) {
			while (recording.Count > 0 && Time.time + Time.deltaTime / 2 - timeStartedPlaying >= recording.Peek().time) {
				KeyLog toSend = recording.Dequeue ();

				if (toSend.type == LogType.Down) {
					//mouse simulated with a different function, so can't just use keydown for everything and pass in the code
					if (toSend.key == KeyCode.Mouse0) {
						inputSim.MouseLeftDown ();

					} else if (toSend.key == KeyCode.Mouse1) {
						inputSim.MouseRightDown ();
					} else {
						inputSim.KeyDown (InputToVirtual.map [toSend.key]);
					}
					keysDown.Add (toSend.key);
					// Debug.Log (toSend.key + " down");

				} else if (toSend.type == LogType.Up) {
					if (toSend.key == KeyCode.Mouse0) {
						inputSim.MouseLeftUp ();
					} else if (toSend.key == KeyCode.Mouse1) {
						inputSim.MouseRightUp ();

					} else {
						inputSim.KeyUp (InputToVirtual.map [toSend.key]);

					}
					if (keysDown.Contains (toSend.key))
						keysDown.Remove (toSend.key);
					// Debug.Log (toSend.key + " up");


				} else if (toSend.type == LogType.MouseTracking) {
					inputSim.MoveMouseTo (gameViewLeft + (int)Mathf.Round (toSend.mouseX * Screen.width), 
					                   gameViewBottom - (int)Mathf.Round (toSend.mouseY * Screen.height));
				} 
			}
		}
	}

	//note: on destroy, any keys that are still simulated down should be simulated up or may 
	//cause problems with OS
	void OnDestroy ()
	{
		foreach (KeyCode keyCode in keysDown) {
			if (keyCode == KeyCode.Mouse0) {
				inputSim.MouseLeftUp ();
			} else if (keyCode == KeyCode.Mouse1) {
				inputSim.MouseRightUp ();
			} else {
				inputSim.KeyUp (InputToVirtual.map [keyCode]);
			}
		}
	}
}

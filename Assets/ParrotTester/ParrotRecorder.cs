using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public enum LogType
{
    Down,
    Up,
    MouseTracking
}

[Serializable]
public class KeyLog : ISerializable
{
    public float time;
    public KeyCode key;
    public LogType type;
    public float mouseX;
    public float mouseY;

    //needed for serialization
    public KeyLog()
    {

    }

    public KeyLog(float time, KeyCode key, LogType type, 
	              float mouseX = 0, float mouseY = 0)
    {
        this.time = time;
        this.key = key;
        this.type = type;
        this.mouseX = mouseX;
        this.mouseY = mouseY;
    }

    public void Print()
    {
        Debug.Log(time + " " + key + " " + type + " " + mouseX + " " + mouseY);
    }


	//todo: maybe use inheritance so we don't have to serialize the values which are not needed
    public KeyLog(SerializationInfo info, StreamingContext context)
    {
        time = (float)info.GetValue("time", typeof(float));
        key = (KeyCode)info.GetValue("key", typeof(KeyCode));
        type = (LogType)info.GetValue("type", typeof(LogType));
        mouseX = (float)info.GetValue("mouseX", typeof(float));
        mouseY = (float)info.GetValue("mouseY", typeof(float));
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("time", time, typeof(float));
        info.AddValue("key", key, typeof(KeyCode));
        info.AddValue("type", type, typeof(LogType));
        info.AddValue("mouseX", mouseX, typeof(float));
        info.AddValue("mouseY", mouseY, typeof(float));
    }
}

public class ParrotRecorder : MonoBehaviour
{
    public Queue<KeyLog> recording = new Queue<KeyLog>();

    public string path;

    float timeStartedRecording;

    void Start()
    {
        DontDestroyOnLoad(this);
        timeStartedRecording = Time.time;
    }

    void Update()
    {
		float recordingTime = Time.time - timeStartedRecording;

		recording.Enqueue(new KeyLog(recordingTime, KeyCode.None, LogType.MouseTracking, Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height));

        foreach (KeyCode keyCode in InputToVirtual.map.Keys)
        //mouse can be recorded using the same method as the keyboard, but simulated differently
        {
            if (Input.GetKeyDown(keyCode))
            {
				recording.Enqueue(new KeyLog(recordingTime, keyCode, LogType.Down));

            }
            if (Input.GetKeyUp(keyCode))
            {
				recording.Enqueue(new KeyLog(recordingTime, keyCode, LogType.Up));
            }
        }
    }

    void OnDestroy()
    {
#if UNITY_EDITOR && !UNITY_WEBPLAYER
        Debug.Log("Saved macro to " + path);
        IFormatter formatter = new BinaryFormatter();
        FileStream fileStream = new FileStream(path, FileMode.Create);
        formatter.Serialize(fileStream, recording);
        fileStream.Close();
        AssetDatabase.Refresh();
#endif
    }
}

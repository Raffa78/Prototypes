using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;

public struct Macro
{
    public string path;
    public string name;

    public Macro(string path)
    {
        this.path = path;
        name = path.Split(Path.DirectorySeparatorChar).Last();
    }
}

[ExecuteInEditMode]
public class ParrotTester : EditorWindow
{

    string folderPath = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "ParrotTesterData";
    string saveName = "";
    List<Macro> macros = new List<Macro>();

    bool wasPlaying = false;
    bool isRecording = false;

	Vector2 scrollPos;

    [MenuItem("Window/ParrotTester")]
    public static void Show()
    {
        EditorWindow.GetWindow(typeof(ParrotTester));
    }

    void OnEnable()
    {
        LoadMacros();
    }

    void LoadMacros()
    {
        if (Directory.Exists(folderPath))
        {
            macros.Clear();
            string[] files = Directory.GetFiles(folderPath);
            foreach (string file in files)
            {
                if (file.EndsWith(".parrot"))
                {
                    Macro macro = new Macro(file);
                    macros.Add(macro);
                }
            }
        }
    }

    void DestroyRecorders()
    {
        isRecording = false;

        //there should only be one recorder
        ParrotRecorder[] parrotRecorders = FindObjectsOfType<ParrotRecorder>();
        foreach (ParrotRecorder parrotRecorder in parrotRecorders)
        {
            DestroyImmediate(parrotRecorder.gameObject);
        }

        if (parrotRecorders.Length > 0)
        {
            LoadMacros();
            Repaint();
        }
    }

    void Update()
    {
        //todo: players won't get cleaned up if window is closed during playback
        if (!EditorApplication.isPlaying && wasPlaying)
        {
            ParrotPlayer[] parrotPlayers = FindObjectsOfType<ParrotPlayer>();
            foreach (ParrotPlayer parrotPlayer in parrotPlayers)
            {
                DestroyImmediate(parrotPlayer.gameObject);
            }

            DestroyRecorders();
        }

        wasPlaying = EditorApplication.isPlaying;
    }

    void OnGUI()
	{
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Working directory:");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(folderPath);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Change", GUILayout.Width(80)))
        {
            string newSaveFolderPath = EditorUtility.OpenFolderPanel("Load Macros in Directory", "", "");
            if (Directory.Exists(newSaveFolderPath))
            {
                folderPath = newSaveFolderPath;
                LoadMacros();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();
	
        EditorGUILayout.LabelField("Recorder", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Macro name: ", GUILayout.Width(80));
        saveName = EditorGUILayout.TextField(saveName);
        if (!isRecording && GUILayout.Button("Start Recording", GUILayout.Width(110)))
        {
            string savePath = folderPath + Path.DirectorySeparatorChar + saveName;
            if (saveName == "")
            {
                savePath += "Unnamed";
            }
            savePath += ".parrot";

            Directory.CreateDirectory(folderPath);

            ParrotRecorder parrotRecorder = Instantiate(Resources.Load<ParrotRecorder>("ParrotRecorder"));
            if (parrotRecorder != null)
            {
                isRecording = true;
                parrotRecorder.path = savePath;
            }

            EditorApplication.isPlaying = true;
        }
        else if (isRecording && (GUILayout.Button("Stop Recording", GUILayout.Width(130)) || !EditorApplication.isPlayingOrWillChangePlaymode))
        {
            DestroyRecorders();
        }
       

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Player", EditorStyles.boldLabel);


		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        foreach (Macro macro in macros)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(macro.name.Substring(0, macro.name.Length - 7), GUILayout.MinWidth(80));

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Play", GUILayout.Width(75)))
            {
                ParrotPlayer parrotPlayer = Instantiate(Resources.Load<ParrotPlayer>("ParrotPlayer"));
                if (parrotPlayer != null)
                {
                    parrotPlayer.path = macro.path;
                }

                EditorApplication.isPlaying = true;
            }
            EditorGUILayout.EndHorizontal();
        }

		EditorGUILayout.EndScrollView();
    }
}

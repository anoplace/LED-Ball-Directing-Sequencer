using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(Sequencer))]
public class SequencerEditor : Editor
{
    public static void ShowEditorGUI(Sequencer sq)
    {

    }
	
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Show Sequencer Editor"))
            SequencerEditorWindow.ShowWindow();
    }
}

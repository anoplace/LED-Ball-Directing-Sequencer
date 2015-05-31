using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(Pattern))]
public class PatternEditor : Editor
{

    public static void ShowEditorGUI()
    {

    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Show Pattern Editor"))
            PatternEditorWindow.ShowWindow();
    }
}

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
        var ptn = (Pattern)target;
        DrawDefaultInspector();
        EditorGUILayout.LabelField(new GUIContent(ptn.patternTex), GUILayout.Height(ptn.numBalls * ptn.numLeds));
        if (GUILayout.Button("Show Editor & Refresh"))
        {
            PatternEditorWindow.ShowWindow();
            SequencerEditorWindow.ShowWindow();
            ptn.CreatePatternTex();
        }
    }
}

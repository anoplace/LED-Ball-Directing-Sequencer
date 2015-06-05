using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(Note))]
public class NoteEditor : Editor
{
    public static void ShowEditorGUI(Note note)
    {

    }

    public override void OnInspectorGUI()
    {
        var not = (Note)target;
        DrawDefaultInspector();
        EditorGUILayout.LabelField(new GUIContent(not.noteTex), GUILayout.Height(not.numLeds));
        if (GUILayout.Button("Show Editor & Refresh"))
        {
            PatternEditorWindow.ShowWindow();
            SequencerEditorWindow.ShowWindow();
            not.CreateNoteTex();
        }
        if (GUI.changed)
            not.CreateNoteTex();
    }
    void DrawNoteInspector()
    {

    }
}

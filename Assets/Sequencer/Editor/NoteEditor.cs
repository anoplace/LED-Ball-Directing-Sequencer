using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(Note))]
public class NoteEditor : Editor
{
    public static Note Duplicate(Note n)
    {
        var newNot = Instantiate(n) as Note;
        var path = AssetDatabase.GetAssetPath(n);

        AssetDatabase.CreateAsset(newNot, AssetDatabase.GenerateUniqueAssetPath(path));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return newNot;
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
        {
            Undo.RecordObject(not,"note property changed");
            not.CreateNoteTex();
            EditorUtility.SetDirty(not);
        }
    }
    void DrawNoteInspector()
    {

    }
}

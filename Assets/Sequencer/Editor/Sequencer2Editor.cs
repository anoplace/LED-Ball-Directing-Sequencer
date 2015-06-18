using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Sequencer2))]
public class Sequencer2Editor : Editor {
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Show Sequencer2 Editor"))
            Sequencer2EditorWindow.ShowWindow();
    }

}

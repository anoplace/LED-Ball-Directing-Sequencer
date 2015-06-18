using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Pattern2))]
public class Pattern2Editor :  Editor{
	public static Pattern2 Duplicate(Pattern2 p)
    {
        var newPtn = Instantiate(p) as Pattern2;
        var path = AssetDatabase.GetAssetPath(p);

        AssetDatabase.CreateAsset(newPtn, AssetDatabase.GenerateUniqueAssetPath(path));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return newPtn;
    }

    public override void OnInspectorGUI()
    {
        var ptn = (Pattern2)target;
        DrawDefaultInspector();
        EditorGUILayout.LabelField(new GUIContent(ptn.patternTex), GUILayout.Height(ptn.numBalls * ptn.numLeds));
        if (GUILayout.Button("Show Editor & Refresh"))
        {
            Pattern2EditorWindow.ShowWindow();
           // Sequencer2EditorWindow.ShowWindow();
            ptn.CreatePatternTex();
        }
	}
}

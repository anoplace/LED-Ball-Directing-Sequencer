using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Note2))]
public class Note2Editor : Editor {
    public static Note2 Duplicate(Note2 n)
    {
        var newNot = Instantiate(n) as Note2;
        var path = AssetDatabase.GetAssetPath(n);

        AssetDatabase.CreateAsset(newNot, AssetDatabase.GenerateUniqueAssetPath(path));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return newNot;
    }

    public override void OnInspectorGUI()
    {
        var not = (Note2)target;
        DrawDefaultInspector();
        EditorGUILayout.LabelField(new GUIContent(not.noteTex), GUILayout.Height(not.numLeds));
        if (GUILayout.Button("Refresh"))
            not.CreateNoteTex();
        if (GUILayout.Button("Create Pallet")){
            var pallet = ColorPallet.CreateInstance(typeof(ColorPallet)) as ColorPallet;
            var path = AssetDatabase.GenerateUniqueAssetPath("/Assets/Sequencer/Pallets/" + "new pallet.asset");
            AssetDatabase.CreateAsset(pallet, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

    }
}

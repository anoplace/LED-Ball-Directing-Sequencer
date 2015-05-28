using UnityEditor;
using UnityEngine;

public class Example : EditorWindow
{ 

    [MenuItem("Window/Example")]
    static void Do ()
    {
        GetWindow<Example> ();
    }

    Object currentObject = null;
    Object selectedObject = null;

    void OnGUI ()
    {
        //ObjectPickerを開く
        if (GUILayout.Button ("ShowObjectPicker")) {
            int controlID = EditorGUIUtility.GetControlID (FocusType.Passive);
            //CameraのコンポーネントをタッチしているGameObjectを選択する
            EditorGUIUtility.ShowObjectPicker<Camera> (null, true, "", controlID);
        }

        string commandName = Event.current.commandName;
        if (commandName == "ObjectSelectorUpdated") {
            currentObject = EditorGUIUtility.GetObjectPickerObject ();
            //ObjectPickerを開いている間はEditorWindowの再描画が行われないのでRepaintを呼びだす
            Repaint ();
        } else if (commandName == "ObjectSelectorClosed") {
            selectedObject = EditorGUIUtility.GetObjectPickerObject ();
        }

        EditorGUILayout.ObjectField (currentObject, typeof(Object), true);
        EditorGUILayout.ObjectField (selectedObject, typeof(Object), true);
    }
}
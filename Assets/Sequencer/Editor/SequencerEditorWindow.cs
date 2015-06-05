using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class SequencerEditorWindow : EditorWindow
{

    [MenuItem("Window/Sequence Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<SequencerEditorWindow>();
        window.Show();
        var sq = window.editingSq;
        if (sq == null)
            return;
        foreach (var pp in sq.patternList)
            pp.pattern.CreatePatternTex();
    }

    Sequencer editingSq;

    object activeNode;
    object willDelete;

    int mode;
    bool showPicker = false;
    Object pickedObj;

    Vector2 scrollPos;
    Vector2 tmpMousePosition;
    float zoom = 1f;
    int noteWidth
    {
        get
        {
            return (int)(zoom * (float)SequencerEditorUtility.noteWidth);
        }
    }
    DirectionController controller
    {
        get
        {
            if (_dcon == null)
                _dcon = FindObjectOfType(typeof(DirectionController)) as DirectionController;
            return _dcon;
        }
    }
    DirectionController _dcon;

    // Update is called once per frame
    void OnInspectorUpdate()
    {
        if (EditorApplication.isPlaying && !EditorApplication.isPaused)
        {
            Selection.activeObject = controller.playingSequencer;
            Repaint();
        }
        //update is for add and remove nodes
        CheckPicker();
        if (willDelete == null)
            return;
        var deletePtn = willDelete as PatternPosition;

        if (deletePtn != null && editingSq.patternList.Contains(deletePtn))
            editingSq.patternList.Remove(deletePtn);
        willDelete = null;
        Repaint();
    }
    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        foreach (var o in FindObjectsOfType(typeof(Sequencer)))
            if (editingSq != o)
                if (GUILayout.Button(o.name))
                    Selection.activeObject = o;
        GUILayout.EndHorizontal();
        if (Selection.activeGameObject == null)
        {
            if (editingSq != null)
                DrawSequencerGUI();
            else
                GUILayout.Label("Select GameObject!!");

            return;
        }
        var sq = Selection.activeGameObject.GetComponent<Sequencer>();
        if (sq == null)
        {
            if (GUILayout.Button("Add Sequencer!"))
                Selection.activeGameObject.AddComponent<Sequencer>();
            return;
        }
        editingSq = sq;

        DrawSequencerGUI();
    }
    void DrawSequencerGUI()
    {
        Event e = Event.current;

        EditorGUILayout.LabelField(editingSq.name);

        var scrollHeight = (editingSq.numBalls + 1) * SequencerEditorUtility.noteHeight;
        scrollPos = GUILayout.BeginScrollView(scrollPos, false, false, GUILayout.Height(scrollHeight));
        if (EditorApplication.isPlaying && !EditorApplication.isPaused)
            scrollPos.x = (editingSq.playTime - 0.5f) * SequencerEditorUtility.noteWidth;
        var scrollWidth = editingSq.length * noteWidth;
        GUILayout.BeginHorizontal(GUILayout.Width(scrollWidth));
        GUILayout.FlexibleSpace();

        if (e.type == EventType.ContextClick && e.button == 1)
        {
            tmpMousePosition = e.mousePosition;
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("New Pattern"), false, CreatePattern);
            menu.AddItem(new GUIContent("Add Pattern"), false, SelectPattern);
            menu.ShowAsContext();
            e.Use();
        }

        Handles.color = Color.white;
        for (var x = 0; x < scrollWidth; x = x + (int)noteWidth)
            Handles.DrawLine(new Vector3(x, 0, 0), new Vector3(x, SequencerEditorUtility.noteHeight * editingSq.numBalls, 0));
        Handles.color = Color.gray;
        for (var y = 0; y <= editingSq.numBalls * SequencerEditorUtility.noteHeight; y += (int)SequencerEditorUtility.noteHeight)
            Handles.DrawLine(new Vector3(0, y, 0), new Vector3(scrollWidth, y, 0));

        // 		Patterns and Notes are here!!
        BeginWindows();
        for (var i = 0; i < editingSq.patternList.Count; i++)
        {
            var pp = editingSq.patternList[i];
            if (pp.pattern == null)
            {
                editingSq.patternList.Remove(pp);
            }
            var rct = new Rect(pp.time * noteWidth, pp.ballIndex * SequencerEditorUtility.noteHeight, pp.pattern.duration * noteWidth, pp.pattern.numBalls * SequencerEditorUtility.noteHeight);
            var style = new GUIStyle();
            style.normal.background = pp.pattern.patternTex;
            style.normal.textColor = Color.white;
            style.active.textColor = Color.red;

            rct = GUILayout.Window(i, rct, PatternWindow, pp.pattern.name, style);
            if (!e.isMouse)
                SetPatternValWithPos(pp, new Vector2(rct.xMin, rct.yMin));
        }
        EndWindows();
        Handles.color = Color.red;
        Handles.DrawLine(
            new Vector2(editingSq.playTime * SequencerEditorUtility.noteWidth, 0),
            new Vector2(editingSq.playTime * SequencerEditorUtility.noteWidth, editingSq.numBalls * SequencerEditorUtility.noteHeight)
            );

        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();

        var z = EditorGUILayout.FloatField("Zoom", zoom);
        if (z != zoom)
        {
            activeNode = null;
            z = Mathf.Max(0.1f, Mathf.Min(2f, z));
            zoom = z;
        }

        if (e.button == 0 && e.type == EventType.mouseDown)
        {
            Selection.activeObject = editingSq;
            EditorGUI.FocusTextInControl("");
            activeNode = null;
        }
        if (e.isKey && (e.keyCode == KeyCode.Backspace || e.keyCode == KeyCode.Delete))
            willDelete = activeNode;
    }

    void PatternWindow(int id)
    {
        var e = Event.current;
        var currentPp = editingSq.patternList[id];

        GUILayout.BeginHorizontal(GUILayout.Width(noteWidth * 0.25f));
        GUILayout.FlexibleSpace();

        if (e.button == 0 && (e.type == EventType.mouseDown))
        {
            Selection.activeObject = currentPp.pattern;
            activeNode = currentPp;
            if (e.clickCount == 2)
            {
                PatternEditorWindow.ShowWindow();
                SequencerEditorWindow.ShowWindow();
            }
        }

        GUILayout.EndHorizontal();
        GUI.DragWindow();
    }


    void CheckPicker()
    {
        if (!showPicker)
            return;
        var pickerID = EditorGUIUtility.GetObjectPickerControlID();
        if (pickerID != 0)
            pickedObj = EditorGUIUtility.GetObjectPickerObject();

        if (mode == 1)
        {
            if (pickerID == 0)
            {
                var newPtn = pickedObj as Pattern;
                if (newPtn != null)
                {
                    var newPp = new PatternPosition();
                    newPp.pattern = newPtn;
                    SetPatternValWithPos(newPp, tmpMousePosition);
                    editingSq.patternList.Add(newPp);
                }
                showPicker = false;
                mode = 0;
                Repaint();
            }
        }
    }

    #region GenericMenu Functions
    void CreatePattern()
    {
        Undo.RecordObject(editingSq, "add new pattern");
        var newPp = new PatternPosition();
        var newPattern = Pattern.CreateInstance<Pattern>();
        newPattern.numBalls = editingSq.numBalls;
        newPattern.numLeds = editingSq.numLeds;
        newPattern.Init();

        AssetDatabase.CreateAsset(newPattern, AssetDatabase.GenerateUniqueAssetPath("Assets/Sequencer/Datas/Patterns/new Pattern.asset"));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        newPp.pattern = newPattern;
        SetPatternValWithPos(newPp, tmpMousePosition);
        editingSq.patternList.Add(newPp);

        Selection.activeObject = newPattern;
        EditorUtility.SetDirty(editingSq);
        Repaint();
    }

    void SelectPattern()
    {
        mode = 1;
        EditorGUIUtility.ShowObjectPicker<Pattern>(null, false, "", mode);
        showPicker = true;
    }
    #endregion

    #region Snap node position
    void SetPatternValWithPos(PatternPosition pp, Vector2 pos)
    {
        Undo.RecordObject(editingSq, "edit pattern pos");
        pp.time = Mathf.Floor(pos.x / noteWidth * 4f) / 4f;
        pp.time = Mathf.Max(0, Mathf.Min((float)editingSq.length - 0.25f, pp.time));
        pp.ballIndex = Mathf.FloorToInt(pos.y / SequencerEditorUtility.noteHeight);
        pp.ballIndex = Mathf.Max(0, Mathf.Min(editingSq.numBalls - 1, pp.ballIndex));
        EditorUtility.SetDirty(editingSq);
    }
    #endregion
}
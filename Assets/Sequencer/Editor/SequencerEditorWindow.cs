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
    object willDuplicate;
    object willCopy;

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

        if (willDuplicate != null)
        {
            var duplicatePp = willDuplicate as PatternPosition;
            willDuplicate = null;

            if (duplicatePp != null)
            {
                var newPtn = PatternEditor.Duplicate(duplicatePp.pattern);
                Selection.activeObject = newPtn;

                var newPp = new PatternPosition();
                newPp.time = duplicatePp.time + 1f;
                newPp.ballIndex = duplicatePp.ballIndex;
                newPp.pattern = newPtn;

                editingSq.patternList.Add(newPp);
                activeNode = newPp;
                Repaint();
            }
        }

        if (willCopy != null)
        {
            var copyPp = willCopy as PatternPosition;
            willCopy = null;

            if (copyPp != null)
            {
                var newPp = new PatternPosition();
                newPp.time = copyPp.time + 1f;
                newPp.ballIndex = copyPp.ballIndex;
                newPp.pattern = copyPp.pattern;

                editingSq.patternList.Add(newPp);
                activeNode = newPp;
                Repaint();
            }
        }

        if (willDelete != null)
        {
            var deletePtn = willDelete as PatternPosition;

            if (deletePtn != null && editingSq.patternList.Contains(deletePtn))
                editingSq.patternList.Remove(deletePtn);
            willDelete = null;
            Repaint();
        }
    }
    void OnGUI()
    {
        var sq = Selection.activeObject as Sequencer;
        if (sq != null)
            editingSq = sq;

        if (editingSq != null)
            DrawSequencerGUI();

        GUILayout.BeginHorizontal();
        foreach (var o in FindObjectsOfType(typeof(Sequencer)))
            if (editingSq != o)
                if (GUILayout.Button(o.name))
                    Selection.activeObject = o;
        GUILayout.EndHorizontal();
    }
    void DrawSequencerGUI()
    {
        Event e = Event.current;

        EditorGUILayout.LabelField(editingSq.name);

        var scrollHeight = (editingSq.numBalls + 1) * SequencerEditorUtility.noteHeight;
        scrollPos = GUILayout.BeginScrollView(scrollPos, false, false, GUILayout.Height(scrollHeight));
        if (EditorApplication.isPlaying && !EditorApplication.isPaused)
            scrollPos.x = (editingSq.playTime - 5f) * SequencerEditorUtility.noteWidth;
        var scrollWidth = editingSq.duration * noteWidth;
        GUILayout.BeginHorizontal(GUILayout.Width(scrollWidth));
        GUILayout.FlexibleSpace();

        if (e.type == EventType.ContextClick && e.button == 1)
        {
            tmpMousePosition = e.mousePosition;
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("New Pattern"), false, CreatePattern);
            menu.ShowAsContext();
            e.Use();
        }

        Handles.color = Color.white;
        for (var x = 0; x < scrollWidth; x = x + (int)noteWidth)
            Handles.DrawLine(new Vector3(x, 0, 0), new Vector3(x, SequencerEditorUtility.noteHeight * editingSq.numBalls, 0));
        Handles.color = Color.gray;
        for (var y = 0; y <= editingSq.numBalls * SequencerEditorUtility.noteHeight; y += (int)SequencerEditorUtility.noteHeight)
            Handles.DrawLine(new Vector3(0, y, 0), new Vector3(scrollWidth, y, 0));

        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        if (e.type == EventType.DragPerform)
        {
            var newPtn = DragAndDrop.objectReferences[0] as Pattern;
            if (newPtn != null)
                AddPattern(newPtn, e.mousePosition);
        }

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
            style.wordWrap = true;
            style.normal.textColor = Color.white;
            if (activeNode == pp)
                style.normal.textColor = Color.red;

            rct = GUILayout.Window(i, rct, PatternWindow, pp.time + " (" + pp.time * (60f / editingSq.bpm) + "): " + pp.pattern.name, style);
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
        if (e.isKey && ((e.control || e.command) && e.keyCode == KeyCode.D))
            willDuplicate = activeNode;
        if (e.isKey && ((e.control || e.command) && e.keyCode == KeyCode.C))
            willCopy = activeNode;
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
                    AddPattern(newPtn, tmpMousePosition);
                showPicker = false;
                mode = 0;
                Repaint();
            }
        }
    }

    void AddPattern(Pattern p, Vector2 pos)
    {
        var newPp = new PatternPosition();
        newPp.pattern = p;
        SetPatternValWithPos(newPp, pos);
        editingSq.patternList.Add(newPp);
    }

    #region GenericMenu Functions
    void CreatePattern()
    {
        Undo.RecordObject(editingSq, "add new pattern");
        var newPtn = Pattern.CreateInstance<Pattern>();
        newPtn.numBalls = editingSq.numBalls;
        newPtn.numLeds = editingSq.numLeds;
        newPtn.Init();

        AssetDatabase.CreateAsset(newPtn, AssetDatabase.GenerateUniqueAssetPath("Assets/Sequencer/Datas/Patterns/new Pattern.asset"));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        AddPattern(newPtn, tmpMousePosition);

        Selection.activeObject = newPtn;
        EditorUtility.SetDirty(editingSq);
        Repaint();
    }

    #endregion

    #region Snap node position
    void SetPatternValWithPos(PatternPosition pp, Vector2 pos)
    {
        Undo.RecordObject(editingSq, "edit pattern pos");
        pp.time = Mathf.Floor(pos.x / noteWidth * 4f) / 4f;
        pp.time = Mathf.Max(0.25f - pp.pattern.duration, Mathf.Min((float)editingSq.duration - 0.25f, pp.time));
        pp.ballIndex = Mathf.FloorToInt(pos.y / SequencerEditorUtility.noteHeight);
        pp.ballIndex = Mathf.Max(1 - pp.pattern.numBalls, Mathf.Min(editingSq.numBalls - 1, pp.ballIndex));
        EditorUtility.SetDirty(editingSq);
    }
    #endregion
}
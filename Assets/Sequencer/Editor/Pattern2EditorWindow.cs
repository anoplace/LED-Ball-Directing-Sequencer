using UnityEngine;
using UnityEditor;
using System.Collections;

public class Pattern2EditorWindow : EditorWindow {

    [MenuItem("Window/Pattern2 Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<Pattern2EditorWindow>();
        window.Show();
        var ptn = window.editingPtn;
        if (ptn == null)
            return;
        foreach (var pp in ptn.patternList)
            pp.pattern.CreatePatternTex();
        foreach (var np in ptn.noteList)
            np.note.CreateNoteTex();
    }


    Pattern2 editingPtn;

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

    // Update is called once per frame
    void OnInspectorUpdate()
    {
        //update is for add and remove nodes
        CheckPicker();
        if (willDuplicate != null)
        {
            var duplicatePp = willDuplicate as PatternPosition2;
            var duplicateNp = willDuplicate as NotePosition2;
            willDuplicate = null;

            if (duplicatePp != null)
            {
                var newPtn = Pattern2Editor.Duplicate(duplicatePp.pattern);

                var newPp = new PatternPosition2();
                newPp.time = duplicatePp.time + 1f;
                newPp.ballIndex = duplicatePp.ballIndex;
                newPp.pattern = newPtn;

                editingPtn.patternList.Add(newPp);
                activeNode = newPp;
                Repaint();
            }

            if (duplicateNp != null)
            {
                var newNot = Note2Editor.Duplicate(duplicateNp.note);
                Selection.activeObject = newNot;

                var newNp = new NotePosition2();
                newNp.time = duplicateNp.time + 1f;
                newNp.ballIndex = duplicateNp.ballIndex;
                newNp.note = newNot;

                editingPtn.noteList.Add(newNp);
                activeNode = newNp;
                Repaint();
            }
        }

        if (willCopy != null)
        {
            var CopyPp = willCopy as PatternPosition2;
            var CopyNp = willCopy as NotePosition2;
            willCopy = null;

            if (CopyPp != null)
            {
                var newPp = new PatternPosition2();
                newPp.time = CopyPp.time + 1f;
                newPp.ballIndex = CopyPp.ballIndex;
                newPp.pattern = CopyPp.pattern;

                editingPtn.patternList.Add(newPp);
                activeNode = newPp;
                Repaint();
            }

            if (CopyNp != null)
            {
                var newNp = new NotePosition2();
                newNp.time = CopyNp.time + 1f;
                newNp.ballIndex = CopyNp.ballIndex;
                newNp.note = CopyNp.note;

                editingPtn.noteList.Add(newNp);
                activeNode = newNp;
                Repaint();
            }
        }

        if (willDelete != null)
        {
            var deletePtn = willDelete as PatternPosition2;
            var deleteNot = willDelete as NotePosition2;

            if (deletePtn != null && editingPtn.patternList.Contains(deletePtn))
                editingPtn.patternList.Remove(deletePtn);
            if (deleteNot != null && editingPtn.noteList.Contains(deleteNot))
                editingPtn.noteList.Remove(deleteNot);
            willDelete = null;
            Repaint();
        }
    }
    void OnGUI()
    {
        var selecting = Selection.activeObject as Pattern2;
        if (selecting == null)
        {
            if (editingPtn != null)
                DrawPatternGUI();
            else
                GUILayout.Label("Select Pattern!!");

            return;
        }

        editingPtn = selecting;

        DrawPatternGUI();
    }
    void DrawPatternGUI()
    {
        Event e = Event.current;

        EditorGUILayout.LabelField(editingPtn.name);

        var scrollHeight = (editingPtn.numBalls + 1) * SequencerEditorUtility.noteHeight;
        scrollPos = GUILayout.BeginScrollView(scrollPos, false, false, GUILayout.Height(scrollHeight));
        var scrollWidth = editingPtn.duration * noteWidth;
        GUILayout.BeginHorizontal(GUILayout.Width(scrollWidth));
        GUILayout.FlexibleSpace();

        if (e.type == EventType.ContextClick && e.button == 1)
        {
            tmpMousePosition = e.mousePosition;
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("New Pattern"), false, CreatePattern);
            menu.AddItem(new GUIContent("New Note"), false, CreateNote);
            menu.ShowAsContext();
            e.Use();
        }

        Handles.color = Color.white;
        for (var x = 0; x < scrollWidth; x = x + (int)noteWidth)
            Handles.DrawLine(new Vector3(x, 0, 0), new Vector3(x, SequencerEditorUtility.noteHeight * editingPtn.numBalls, 0));
        Handles.color = Color.gray;
        for (var y = 0; y <= editingPtn.numBalls * SequencerEditorUtility.noteHeight; y += (int)SequencerEditorUtility.noteHeight)
            Handles.DrawLine(new Vector3(0, y, 0), new Vector3(scrollWidth, y, 0));

        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        if (e.type == EventType.DragPerform)
        {
            var newPtn = DragAndDrop.objectReferences[0] as Pattern2;
            var newNot = DragAndDrop.objectReferences[0] as Note2;
            if (newPtn != null)
                AddPattern(newPtn, e.mousePosition);
            if (newNot != null)
                AddNote(newNot, e.mousePosition);
        }

        // 		Patterns and Notes are here!!
        BeginWindows();
        for (var i = 0; i < editingPtn.patternList.Count; i++)
        {
            var pp = editingPtn.patternList[i];
            if (pp.pattern == null)
            {
                editingPtn.patternList.Remove(pp);
                continue;
            }
            var rct = new Rect(pp.time * noteWidth, pp.ballIndex * SequencerEditorUtility.noteHeight, pp.pattern.duration * noteWidth, pp.pattern.numBalls * SequencerEditorUtility.noteHeight);
            var style = new GUIStyle();
            style.normal.background = pp.pattern.patternTex;
            style.normal.textColor = Color.white;
            if (activeNode == pp)
                style.normal.textColor = Color.red;

            rct = GUILayout.Window(i, rct, PatternWindow, pp.pattern.name, style);
            if (!e.isMouse)
                SetPatternValWithPos(pp, new Vector2(rct.xMin, rct.yMin));
        }
        for (var i = 0; i < editingPtn.noteList.Count; i++)
        {
            var np = editingPtn.noteList[i];
            if (np.note == null)
            {
                editingPtn.noteList.Remove(np);
                continue;
            }
            var rct = new Rect(np.time * noteWidth, np.ballIndex * SequencerEditorUtility.noteHeight, np.duration * noteWidth, SequencerEditorUtility.noteHeight);
            var style = new GUIStyle();
            style.normal.background = np.note.noteTex;
            style.normal.textColor = Color.white;
            if (activeNode == np)
                style.normal.textColor = Color.red;
            rct = GUILayout.Window(i + editingPtn.patternList.Count, rct, NoteWindow, np.note.name, style);
            if (!e.isMouse)
                SetNoteValWithPos(np, new Vector2(rct.xMin, rct.yMin));
        }
        EndWindows();

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
            Selection.activeObject = editingPtn;
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
        var currentPp = editingPtn.patternList[id];

        GUILayout.BeginHorizontal(GUILayout.Width(noteWidth * 0.25f));
        GUILayout.FlexibleSpace();

        if (e.button == 0 && (e.type == EventType.mouseDown))
        {
            activeNode = currentPp;
            if (e.clickCount == 2)
                Selection.activeObject = currentPp.pattern;
        }

        GUILayout.EndHorizontal();
        GUI.DragWindow();
    }

    void NoteWindow(int id)
    {
        var e = Event.current;
        var currentNp = editingPtn.noteList[id - editingPtn.patternList.Count];

        GUILayout.BeginHorizontal(GUILayout.Width(noteWidth * zoom * 0.25f));
        GUILayout.FlexibleSpace();

        if (e.button == 0 && (e.type == EventType.mouseDown))
        {
            Selection.activeObject = currentNp.note;
            activeNode = currentNp;
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
                var newPtn = pickedObj as Pattern2;
                if (newPtn != null)
                {
                    AddPattern(newPtn, tmpMousePosition);
                }
                showPicker = false;
                mode = 0;
                Repaint();
            }
        }
        else if (mode == 2)
        {
            if (pickerID == 0)
            {
                var newNot = pickedObj as Note2;
                if (newNot != null)
                    AddNote(newNot, tmpMousePosition);
                showPicker = false;
                mode = 0;
                Repaint();
            }
        }
    }

    void AddPattern(Pattern2 p, Vector2 pos)
    {
        if (p.CheckLoop(editingPtn))
        {
            Debug.LogWarning("new pattern try to add is contains loop!");
            showPicker = false;
            mode = 0;
            Repaint();
            return;
        }
        var newPp = new PatternPosition2();
        newPp.pattern = p;
        SetPatternValWithPos(newPp, pos);
        editingPtn.patternList.Add(newPp);
    }
    void AddNote(Note2 n, Vector2 pos)
    {
        var newNp = new NotePosition2();
        newNp.note = n;
        newNp.duration = 1f;
        SetNoteValWithPos(newNp, pos);
        editingPtn.noteList.Add(newNp);
    }

    #region GenericMenu Functions
    void CreatePattern()
    {
        Undo.RecordObject(editingPtn, "add new pattern");
        var newPtn = Pattern2.CreateInstance<Pattern2>();
        newPtn.numBalls = editingPtn.numBalls;
        newPtn.numLeds = editingPtn.numLeds;
        newPtn.pallet = editingPtn.pallet;

        AssetDatabase.CreateAsset(newPtn, AssetDatabase.GenerateUniqueAssetPath("Assets/Sequencer/Datas/PatternNeo/new Pattern.asset"));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        AddPattern(newPtn, tmpMousePosition);

        EditorUtility.SetDirty(editingPtn);
        Repaint();
    }
    void CreateNote()
    {
        Undo.RecordObject(editingPtn, "add new note");
        var newNot = Note2.CreateInstance<Note2>();
        newNot.numLeds = editingPtn.numLeds;

        AssetDatabase.CreateAsset(newNot, AssetDatabase.GenerateUniqueAssetPath("Assets/Sequencer/Datas/NoteNeo/new Note.asset"));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        AddNote(newNot, tmpMousePosition);

        Selection.activeObject = newNot;
        EditorUtility.SetDirty(editingPtn);
        Repaint();
    }

    #endregion

    #region Snap node position
    void SetPatternValWithPos(PatternPosition2 pp, Vector2 pos)
    {
        Undo.RecordObject(editingPtn, "edit pattern pos");
        pp.time = Mathf.Floor(pos.x / noteWidth * 4f) / 4f;
        pp.time = Mathf.Max(0.25f - pp.pattern.duration, Mathf.Min((float)editingPtn.duration - 0.25f, pp.time));
        pp.ballIndex = Mathf.FloorToInt(pos.y / SequencerEditorUtility.noteHeight);
        pp.ballIndex = Mathf.Max(1 - pp.pattern.numBalls, Mathf.Min(editingPtn.numBalls - 1, pp.ballIndex));
        EditorUtility.SetDirty(editingPtn);
    }
    void SetNoteValWithPos(NotePosition2 np, Vector2 pos)
    {
        Undo.RecordObject(editingPtn, "edit note pos");
        np.time = Mathf.Floor(pos.x / noteWidth * 4f) / 4f;
        np.time = Mathf.Max(0.25f - np.duration, Mathf.Min((float)editingPtn.duration - 0.25f, np.time));
        np.ballIndex = Mathf.FloorToInt(pos.y / SequencerEditorUtility.noteHeight);
        np.ballIndex = Mathf.Max(0, Mathf.Min(editingPtn.numBalls - 1, np.ballIndex));
        EditorUtility.SetDirty(editingPtn);
    }
    #endregion
}
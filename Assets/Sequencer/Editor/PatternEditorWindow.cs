using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class PatternEditorWindow : EditorWindow
{

    public static void ShowWindow()
    {
        var window = GetWindow<PatternEditorWindow>();
        window.Show();
    }


    Pattern editingPtn;

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

    // Update is called once per frame
    void Update()
    {
        //update is for add and remove nodes
        CheckPicker();
        if (willDelete == null)
            return;
        var deletePtn = willDelete as PatternPosition;
        var deleteNot = willDelete as NotePosition;

        if (deletePtn != null && editingPtn.patternList.Contains(deletePtn))
            editingPtn.patternList.Remove(deletePtn);
        if (deleteNot != null && editingPtn.noteList.Contains(deleteNot))
            editingPtn.noteList.Remove(deleteNot);
        willDelete = null;
        Repaint();
    }
    void OnGUI()
    {
        var selecting = Selection.activeObject as Pattern;
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
            menu.AddItem(new GUIContent("Add Pattern"), false, SelectPattern);
            menu.AddItem(new GUIContent("Add Note"), false, SelectNote);
            menu.ShowAsContext();
            e.Use();
        }

        Handles.color = Color.white;
        for (var x = 0; x < scrollWidth; x = x + (int)noteWidth)
            Handles.DrawLine(new Vector3(x, 0, 0), new Vector3(x, 1000, 0));
        Handles.color = Color.gray;
        for (var y = 0; y <= editingPtn.numBalls * SequencerEditorUtility.noteHeight; y += (int)SequencerEditorUtility.noteHeight)
            Handles.DrawLine(new Vector3(0, y, 0), new Vector3(scrollWidth, y, 0));

        // 		Patterns and Notes are here!!
        BeginWindows();
        for (var i = 0; i < editingPtn.patternList.Count; i++)
        {
            var pp = editingPtn.patternList[i];
            var rct = new Rect(pp.time * noteWidth, pp.ballIndex * SequencerEditorUtility.noteHeight, pp.pattern.duration * noteWidth, pp.pattern.numBalls * SequencerEditorUtility.noteHeight);
            rct = GUILayout.Window(i, rct, PatternWindow, pp.pattern.name, "flow node 1");
            if (!e.isMouse)
                SetPatternValWithPos(pp, new Vector2(rct.xMin, rct.yMin));
        }
        for (var i = 0; i < editingPtn.noteList.Count; i++)
        {
            var np = editingPtn.noteList[i];
            var rct = new Rect(np.time * noteWidth, np.ballIndex * SequencerEditorUtility.noteHeight, np.note.duration * noteWidth, SequencerEditorUtility.noteHeight);
            rct = GUILayout.Window(i + editingPtn.patternList.Count, rct, NoteWindow, np.note.name, "flow node 1");
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
                var newPtn = pickedObj as Pattern;
                if (newPtn != null)
                {
                    if (editingPtn.CheckLoop(newPtn))
                    {
                        Debug.Log("new pattern try to add is contains loop!");
                        Repaint();
                        return;
                    }
                    var newPp = new PatternPosition();
                    newPp.pattern = newPtn;
                    SetPatternValWithPos(newPp, tmpMousePosition);
                    editingPtn.patternList.Add(newPp);
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
                var newNot = pickedObj as Note;
                if (newNot != null)
                {
                    var newNp = new NotePosition();
                    newNp.note = newNot;
                    SetNoteValWithPos(newNp, tmpMousePosition);
                    editingPtn.noteList.Add(newNp);
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
        Undo.RecordObject(editingPtn, "add new pattern");
        var newPp = new PatternPosition();
        var newPattern = Pattern.CreateInstance<Pattern>();
        newPattern.numBalls = editingPtn.numBalls;
        newPattern.Init();

        AssetDatabase.CreateAsset(newPattern, AssetDatabase.GenerateUniqueAssetPath("Assets/Sequencer/Datas/Patterns/new Pattern.asset"));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        newPp.pattern = newPattern;
        SetPatternValWithPos(newPp, tmpMousePosition);
        editingPtn.patternList.Add(newPp);

        Selection.activeObject = newPattern;
        EditorUtility.SetDirty(editingPtn);
        Repaint();
    }
    void CreateNote()
    {
        Undo.RecordObject(editingPtn, "add new note");
        var newNp = new NotePosition();
        var newNote = Note.CreateInstance<Note>();
        newNote.Init();

        AssetDatabase.CreateAsset(newNote, AssetDatabase.GenerateUniqueAssetPath("Assets/Sequencer/Datas/Notes/new Note.asset"));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        newNp.note = newNote;
        SetNoteValWithPos(newNp, tmpMousePosition);
        editingPtn.noteList.Add(newNp);

        Selection.activeObject = newNote;
        EditorUtility.SetDirty(editingPtn);
        Repaint();
    }

    void SelectPattern()
    {
        mode = 1;
        EditorGUIUtility.ShowObjectPicker<Pattern>(null, false, "", mode);
        showPicker = true;
    }
    void SelectNote()
    {
        mode = 2;
        EditorGUIUtility.ShowObjectPicker<Note>(null, false, "", mode);
        showPicker = true;
    }
    #endregion

    #region Snap node position
    void SetPatternValWithPos(PatternPosition pp, Vector2 pos)
    {
        Undo.RecordObject(editingPtn, "edit pattern pos");
        pp.time = Mathf.Floor(pos.x / noteWidth * 4f) / 4f;
        pp.time = Mathf.Max(0, Mathf.Min((float)editingPtn.duration - 0.25f, pp.time));
        pp.ballIndex = Mathf.FloorToInt(pos.y / SequencerEditorUtility.noteHeight);
        pp.ballIndex = Mathf.Max(0, Mathf.Min(editingPtn.numBalls - 1, pp.ballIndex));
        EditorUtility.SetDirty(editingPtn);
    }
    void SetNoteValWithPos(NotePosition np, Vector2 pos)
    {
        Undo.RecordObject(editingPtn, "edit note pos");
        np.time = Mathf.Floor(pos.x / noteWidth * 4f) / 4f;
        np.time = Mathf.Max(0, Mathf.Min((float)editingPtn.duration - 0.25f, np.time));
        np.ballIndex = Mathf.FloorToInt(pos.y / SequencerEditorUtility.noteHeight);
        np.ballIndex = Mathf.Max(0, Mathf.Min(editingPtn.numBalls - 1, np.ballIndex));
        EditorUtility.SetDirty(editingPtn);
    }
    #endregion
}
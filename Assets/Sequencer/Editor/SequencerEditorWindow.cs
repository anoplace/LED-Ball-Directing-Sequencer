using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class SequencerEditorWindow : EditorWindow
{

    // Use this for initialization
    public static void ShowWindow()
    {
        var window = GetWindow<SequencerEditorWindow>();
        window.Show();
    }
    const float
        noteWidth = 32f,
        noteHeight = 32f;

    Sequencer editingSq;
    Pattern editingPtn;
    Note editingNot;
    object activeNode;
    object willDelete;

    float newTime;
    int mode;
    bool showPicker = false;
    Object pickObj;
    Vector2 scrollPos;
    Vector2 tmpMousePosition;

    // Update is called once per frame
    void Update()
    {
        CheckPicker();
        if (willDelete == null)
            return;
        var deletePtn = willDelete as PatternPosition;
        var deleteNot = willDelete as NotePosition;

        if (deletePtn != null && editingSq.patternList.Contains(deletePtn))
            editingSq.patternList.Remove(deletePtn);
        if (deleteNot != null && editingSq.noteList.Contains(deleteNot))
            editingSq.noteList.Remove(deleteNot);
        willDelete = null;
        Repaint();
    }
    void OnGUI()
    {
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

        scrollPos = GUILayout.BeginScrollView(scrollPos, false, false);
        float scrollWidth = editingSq.length * 10f;
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
        for (var y = 0; y <= editingSq.numBalls * noteHeight; y += (int)noteHeight)
            Handles.DrawLine(new Vector3(0, y, 0), new Vector3(scrollWidth, y, 0));

        // 		Patterns and Notes are here!!
        BeginWindows();
        for (var i = 0; i < editingSq.patternList.Count; i++)
        {
            var pp = editingSq.patternList[i];
            var rct = new Rect(pp.time * noteWidth, pp.ballIndex * noteHeight, pp.pattern.duration * noteWidth, pp.pattern.numBalls * noteHeight);
            rct = GUILayout.Window(i, rct, PatternWindow, pp.pattern.name, "flow node 1");
            if (!e.isMouse)
                SetPatternValWithPos(pp, new Vector2(rct.xMin, rct.yMin));
        }
        for (var i = 0; i < editingSq.noteList.Count; i++)
        {
            var np = editingSq.noteList[i];
            var rct = new Rect(np.time * noteWidth, np.ballIndex * noteHeight, np.note.duration * noteWidth, noteHeight);
            rct = GUILayout.Window(i + editingSq.patternList.Count, rct, NoteWindow, np.note.name, "flow node 1");
            if (!e.isMouse)
                SetNoteValWithPos(np, new Vector2(rct.xMin, rct.yMin));
        }
        EndWindows();

        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();

        if (e.button == 0 && e.type == EventType.mouseDown)
        {
            Selection.activeObject = editingSq;
            activeNode = null;
        }
        if (e.isKey && (e.keyCode == KeyCode.Backspace || e.keyCode == KeyCode.Delete))
            willDelete = activeNode;

        //         CheckPicker();
    }

    void PatternWindow(int id)
    {
        var e = Event.current;
        var currentPp = editingSq.patternList[id];
        editingPtn = currentPp.pattern;

        GUILayout.BeginHorizontal(GUILayout.Width(noteWidth * 0.25f));
        GUILayout.FlexibleSpace();

        if (e.button == 0 && (e.type == EventType.mouseDown))
        {
            Selection.activeObject = editingPtn;
            activeNode = currentPp;
        }

        GUILayout.EndHorizontal();
        GUI.DragWindow();
    }

    void NoteWindow(int id)
    {
        var e = Event.current;
        var currentNp = editingSq.noteList[id - editingSq.patternList.Count];
        editingNot = currentNp.note;

        GUILayout.BeginHorizontal(GUILayout.Width(noteWidth * 0.25f));
        GUILayout.FlexibleSpace();

        if (e.button == 0 && (e.type == EventType.mouseDown))
        {
            Selection.activeObject = editingNot;
            activeNode = currentNp;
        }

        GUILayout.EndHorizontal();
        GUI.DragWindow();
    }


    void CheckPicker()
    {
        Event e = Event.current;

        if (showPicker)
        {
            var pickerID = EditorGUIUtility.GetObjectPickerControlID();
            if (pickerID != 0)
                pickObj = EditorGUIUtility.GetObjectPickerObject();

            if (mode == 1)
            {
                if (pickerID == 0)
                {
                    var newPtn = pickObj as Pattern;
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
            else if (mode == 2)
            {
                if (pickerID == 0)
                {
                    var newNot = pickObj as Note;
                    if (newNot != null)
                    {
                        var newNp = new NotePosition();
                        newNp.note = newNot;
                        SetNoteValWithPos(newNp, tmpMousePosition);
                        editingSq.noteList.Add(newNp);
                    }
                    showPicker = false;
                    mode = 0;
                    Repaint();
                }
            }
        }
    }

    void CreatePattern()
    {
        Undo.RecordObject(editingSq, "add new pattern");
        var newPp = new PatternPosition();
        var newPattern = Pattern.CreateInstance<Pattern>();
        newPattern.numBalls = editingSq.numBalls;
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
    void CreateNote()
    {
        Undo.RecordObject(editingSq, "add new note");
        var newNp = new NotePosition();
        var newNote = Note.CreateInstance<Note>();
        newNote.Init();

        AssetDatabase.CreateAsset(newNote, AssetDatabase.GenerateUniqueAssetPath("Assets/Sequencer/Datas/Notes/new Note.asset"));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        newNp.note = newNote;
        SetNoteValWithPos(newNp, tmpMousePosition);
        editingSq.noteList.Add(newNp);

        Selection.activeObject = newNote;
        EditorUtility.SetDirty(editingSq);
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

    void SetPatternValWithPos(PatternPosition pp, Vector2 pos)
    {
        Undo.RecordObject(editingSq, "edit pattern pos");
        pp.time = Mathf.Floor(pos.x / noteWidth * 4f) / 4f;
        pp.ballIndex = Mathf.FloorToInt(pos.y / noteHeight);
        EditorUtility.SetDirty(editingSq);
    }
    void SetNoteValWithPos(NotePosition np, Vector2 pos)
    {
        Undo.RecordObject(editingSq, "edit note pos");
        np.time = Mathf.Floor(pos.x / noteWidth * 4f) / 4f;
        np.ballIndex = Mathf.FloorToInt(pos.y / noteHeight);
        EditorUtility.SetDirty(editingSq);
    }
}
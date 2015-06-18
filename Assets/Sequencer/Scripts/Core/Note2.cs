using UnityEngine;
using System.Collections;

[System.SerializableAttribute]
public class Note2 : ScriptableObject
{
    public int noteId;
	
    public int fromColor0;
    public int fromColor1;
    public int toColor0;
    public int toColor1;

    [HideInInspector]
    public int numLeds = 12;
	[HideInInspector]
    public ColorPallet pallet;

    Color color;

    public Texture2D noteTex
    {
        get
        {
            if (_tex == null)
                CreateNoteTex();
            return _tex;
        }
    }
    Texture2D _tex;

    public void CreateNoteTex()
    {
        if (_tex != null)
            DestroyImmediate(_tex);
        var width = 16;
        var height = numLeds;
        _tex = new Texture2D(width, height);
        _tex.filterMode = FilterMode.Point;

        for (var x = 0; x < width; x++)
        {
            var t = (float)x / (float)width;
            var colors = GetColors(t);
            for (var y = 0; y < height; y++)
                _tex.SetPixel(x, height - y - 1, colors[y]);
        }
        _tex.Apply();
        _tex.hideFlags = HideFlags.HideAndDontSave;
    }
    public Color[] GetColors(float t)
    {
        Color
            c00 = pallet.colors[fromColor0],
            c01 = pallet.colors[fromColor1],
            c10 = pallet.colors[toColor0],
            c11 = pallet.colors[toColor1];
        var colors = new Color[numLeds];

        var c0 = Color.Lerp(c00, c10, t);
        var c1 = Color.Lerp(c01, c11, t);
        for (var i = 0; i < colors.Length; i++)
            colors[i] = Color.Lerp(c0, c1, (float)i / (float)colors.Length);
        return colors;
    }
}

[System.SerializableAttribute]
public class NotePosition2
{
    public float time;
    public float duration;
    public int ballIndex;
    public Note2 note;
}
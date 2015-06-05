using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.SerializableAttribute]
public class Note : ScriptableObject
{
    public float duration = 1f;
    public Interpolation interpolationType = Interpolation.flat;
    public bool useGradient = true;
    public bool useSingleColor = false;
    public Gradient gradient;
    public Color[] colors;
    public float shift = 0;
    public int numLeds = 12;
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
        var width = (int)(duration * 8);
        var height = numLeds;
        _tex = new Texture2D(width, height);
        _tex.filterMode = FilterMode.Point;
        SetColors();
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                _tex.SetPixel(x, height - y - 1, GetCurrentColors()[y] * ((x % 2 == 0 && interpolationType == Interpolation.lerp) ? 0.75f : 1f));
				//EditorWindow上では、下が０、上が１になるので、y=1-yで上下反転
    }
    _tex.Apply();
            _tex.hideFlags = HideFlags.HideAndDontSave;
        }
    }
    public void Init()
{
    colors = new Color[numLeds];
}


void SetColors()
{
    if (useGradient)
    {
        for (var i = 0; i < colors.Length; i++)
        {
                float t = ((float)i / (float)colors.Length + shift) % 1f;
                colors[i] = gradient.Evaluate(t);
        }
    }
    else if (useSingleColor)
    {
        for (var i = 0; i < colors.Length; i++)
        {
            colors[i] = colors[0];
        }
    }
}
public Color[] GetCurrentColors()
{
    for (var i = 0; i < colors.Length; i++)
        colors[i].a = 0.75f;
    return colors;
}
public enum Interpolation
{
    lerp = 0,
    flat = 1,
}
}
[System.SerializableAttribute]
public class NotePosition
{
    public float time;
    public int ballIndex;
    public Note note;
}
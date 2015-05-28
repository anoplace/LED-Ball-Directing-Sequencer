using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.SerializableAttribute]
public class Note : ScriptableObject
{
    public float duration = 1f;
    public Interpolation interpolationType;
    public bool
        useGradient = true,
        useSingleColor = false;
    public Gradient gradient;
    public Color[] colors;
    public float shift = 0;
    public int numLeds = 12;
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
    Color[] GetCurrentColors()
    {
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
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.SerializableAttribute]
public class Pattern : ScriptableObject
{
    public float duration = 5f;
    public int numBalls = 10;
    public int numLeds = 12;
    public List<PatternPosition> patternList = new List<PatternPosition>();
    public List<NotePosition> noteList = new List<NotePosition>();

    public Texture2D patternTex
    {
        get
        {
            if (_tex == null)
                CreatePatternTex();
            return _tex;
        }
    }
    Texture2D _tex;
    public void CreatePatternTex()
    {
        if (_tex != null)
            DestroyImmediate(_tex);

        var width = (int)(duration * 8);
        var height = numBalls * numLeds;
        _tex = new Texture2D(width, height);
        _tex.filterMode = FilterMode.Point;

        for (var x = 0; x < width; x++)
        {
            for (var i = 0; i < numBalls; i++)//ball index
            {
                var ballColors = GetColors((float)x / 8f, i);
                for (var n = 0; n < numLeds; n++)//num leds
                {
                    var y = i * numLeds + n;
                    _tex.SetPixel(x, numLeds * numBalls - y - 1, ballColors[n]);
                    //EditorWindow上では、下が０、上が１になるので、y=1-yで上下反転
                }
            }
        }
        _tex.Apply();
        _tex.hideFlags = HideFlags.HideAndDontSave;
    }
    public void Init()
    {

    }
    //colors for LEDs of ball
    public Color[] GetColors(float time, int ballIndex)
    {
        var pp = patternList
            .Where(
                b => b.ballIndex <= ballIndex &&
                ballIndex < b.ballIndex + b.pattern.numBalls &&
                b.time <= time && time < b.time + b.pattern.duration
			)
			.FirstOrDefault();
        if (pp != null)
            return pp.pattern.GetColors(time - pp.time, ballIndex - pp.ballIndex);
        var nextNote = GetNextNote(time, ballIndex);
        var currentNote = GetCurrentNote(time, ballIndex);
        var prevNote = GetPrevNote(time, ballIndex);

        if (nextNote != null)
            if (nextNote.note.interpolationType == Note.Interpolation.lerp)
                if (prevNote != null)
                    return GetLerpColors(time, ballIndex, prevNote, nextNote);

        if (currentNote != null)
            return currentNote.note.GetCurrentColors();

        return Enumerable.Repeat<Color>(new Color(0, 0, 0, 0.75f), numLeds).ToArray();
    }

    public bool CheckLoop(Pattern p)
    {
        if (this == p)
            return true;
        foreach (var pp in patternList)
            if (pp.pattern.CheckLoop(p))
                return true;
        return false;
    }
    Color[] GetLerpColors(
        float time, int ballIndex,
        NotePosition prevNote, NotePosition nextNote
    )
    {
        var lerpVal = GetLerpVal(time, prevNote, nextNote);
        var pNote = prevNote.note;
        var nNote = nextNote.note;

        var colors = new Color[prevNote.note.numLeds];
        for (var i = 0; i < colors.Length; i++)
            colors[i] = Color.Lerp(pNote.colors[i], nNote.colors[i], lerpVal);

        return colors;
    }
    float GetLerpVal(float time, NotePosition prevNote, NotePosition nextNote)
    {
        float
            prevTime = prevNote.time,
            nextTime = nextNote.time;

        return (time - prevTime) / (nextTime - prevTime);
    }
    NotePosition GetCurrentNote(float time, int ballIndex)
    {
        return noteList
            .Where(b => b.ballIndex == ballIndex && b.time <= time && time < b.time + b.note.duration)
            .OrderBy(b => b.time).FirstOrDefault();
    }
    NotePosition GetPrevNote(float time, int ballIndex)
    {
        return noteList
            .Where(b => b.ballIndex == ballIndex && b.time < time)
            .OrderBy(b => b.time).LastOrDefault();
    }
    NotePosition GetNextNote(float time, int ballIndex)
    {
        return noteList
            .Where(b => b.ballIndex == ballIndex && time <= b.time)
            .OrderBy(b => b.time).FirstOrDefault();
    }
}
[System.SerializableAttribute]
public class PatternPosition
{
    public float time;
    public int ballIndex;
    public Pattern pattern;

}

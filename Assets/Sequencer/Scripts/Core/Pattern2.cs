using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.SerializableAttribute]
public class Pattern2 : ScriptableObject
{
    public float duration = 5f;
    public int numBalls = 10;
    [HideInInspector]
    public int numLeds = 12;
	[HideInInspector]
    public ColorPallet pallet;

    public List<PatternPosition2> patternList = new List<PatternPosition2>();
    public List<NotePosition2> noteList = new List<NotePosition2>();
    float prevTime = 0;

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
            for (var i = 0; i < numBalls; i++)
            {
                var colors = GetColors((float)x / 8f, i);
                for (var n = 0; n < numLeds; n++)
                {
                    var y = i * numBalls + n;
                    var edge = (x == 0) || (x == width - 1) || (y == 0) || (y == height - 1);
                    _tex.SetPixel(x, numLeds * numBalls - y - 1, Color.Lerp(colors[n], Color.white, edge ? 0.5f : 0));
                }
            }
        }
        _tex.Apply();
        _tex.hideFlags = HideFlags.HideAndDontSave;
    }
    public Color[] GetColors(float time, int ballIndex)
    {
        var np = GetCurrentNote(time, ballIndex);
        if (np != null)
            return np.note.GetColors((time - np.time) / np.duration);

        var pp = patternList
            .Where(
                b => b.ballIndex <= ballIndex
                && ballIndex < b.ballIndex + b.pattern.numBalls
                && b.time <= time
                && time < b.time + b.pattern.duration
            )
            .OrderBy(b => b.time)
            .LastOrDefault();

        if (pp != null)
            return pp.pattern.GetColors(time - pp.time, ballIndex - pp.ballIndex);

        return Enumerable.Repeat<Color>(new Color(0, 0, 0, 0.75f), numLeds).ToArray();
    }

    public bool CheckLoop(Pattern2 p)
    {
        if (this == p)
            return true;
        foreach (var pp in patternList)
            if (pp.pattern.CheckLoop(p))
                return true;
        return false;
    }
    public NotePosition2 GetNewNote(float time,int ballIndex)
    {
        var currentNp = GetCurrentNote(time, ballIndex);
        var prevNp = GetCurrentNote(prevTime, ballIndex);
        prevTime = time;
        if (currentNp != prevNp)
            return currentNp;
        return null;
    }

    NotePosition2 GetCurrentNote(float time, int ballIndex)
    {
		return noteList
            .Where(
                    b => b.ballIndex == ballIndex
                    && b.time <= time
                    && time < b.time + b.duration
            )
            .OrderBy(b => b.time)
            .LastOrDefault();
	}
}

[System.SerializableAttribute]
public class PatternPosition2
{
    public float time;
    public int ballIndex;
    public Pattern2 pattern;
}
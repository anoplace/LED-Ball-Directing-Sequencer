using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.SerializableAttribute]
public class Pattern : ScriptableObject
{
    public float duration = 5f;
    public int numBalls = 10;
    Sequencer.Ball[] balls;
    public List<PatternPosition> patternList = new List<PatternPosition>();
    public List<NotePosition> noteList = new List<NotePosition>();

    public void Init()
    {

    }
    //colors for LEDs of ball
    public Color[] GetColors(float time, int ballIndex)
    {
        var pp = patternList.Where(b => b.time < time && time < b.time + b.pattern.duration).FirstOrDefault();
        if (pp != null)
            return pp.pattern.GetColors(time - pp.time, ballIndex + pp.ballIndex);
        var nextNote = GetNextNote(time, ballIndex);
        var currentNote = GetCurrentNote(time, ballIndex);
        var prevNote = GetPrevNote(time, ballIndex);
		
        if (nextNote == null)
        {
            if (currentNote != null)
                return currentNote.note.colors;
        }
        else
        {
            if (nextNote.note.interpolationType == Note.Interpolation.lerp)
            {
                if (prevNote != null)
                    return GetLerpColors(time, ballIndex, prevNote, nextNote);
            }
        }
        return Enumerable.Repeat<Color>(Color.black, balls[ballIndex].numLeds).ToArray();
    }

    public bool CheckLoop(Pattern p)
    {
        foreach (var pp in patternList)
        {
            if (pp.pattern == p)
                return true;
            if (CheckLoop(p))
                return true;
        }
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

        return (nextTime - time) / (nextTime - prevTime);
    }
    NotePosition GetCurrentNote(float time, int ballIndex)
    {
        return noteList
            .Where(b => b.ballIndex == ballIndex && b.time < time && time < b.time + b.note.duration)
            .OrderBy(b => b.time).FirstOrDefault();
    }
    NotePosition GetPrevNote(float time, int ballIndex)
    {
        return noteList
            .Where(b => b.ballIndex == ballIndex && b.time <= time)
            .OrderBy(b => b.time).LastOrDefault();
    }
    NotePosition GetNextNote(float time, int ballIndex)
    {
        return noteList
            .Where(b => b.ballIndex == ballIndex && time < b.time)
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

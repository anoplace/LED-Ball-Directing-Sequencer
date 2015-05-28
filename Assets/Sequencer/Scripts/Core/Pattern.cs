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
    public Color[] GetColors(float time, int ballIndex)
    {
        var pp = patternList.Where(b => b.time < time && time < b.time + b.pattern.duration).FirstOrDefault();
        if (pp != null)
            return pp.pattern.GetColors(time - pp.time, ballIndex);
        var nextNote = GetNextNote(time, ballIndex);
        var currentNote = GetCurrentNote(time, ballIndex);
        var prevNote = GetPrevNote(time, ballIndex);
        if (nextNote == null)
        {
            if (currentNote == null)
                return Enumerable.Repeat<Color>(Color.black, balls[ballIndex].numLeds).ToArray();
        }
        return new Color[] { Color.black };
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
    float GetLerpVal(float time, int ballIndex)
    {
        NotePosition
            prevNote = GetPrevNote(time, ballIndex),
            nextNote = GetNextNote(time, ballIndex);
        if (prevNote == null || nextNote == null)
            return 0;
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

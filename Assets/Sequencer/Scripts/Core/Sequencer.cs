using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sequencer : MonoBehaviour
{
    public int
        duration = 32,
		numBalls = 10;
    public float
        bpm = 80f,
        time,
        seekPos = 0;
    public List<PatternPosition> patternList = new List<PatternPosition>();
    public List<NotePosition> noteList = new List<NotePosition>();

    public float SetTime()
    {
        time = (float)duration * (60f / bpm);
        return time;
    }
    // Use this for initialization
    void Start()
    {
        SetTime();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Play()
    {

    }


    [System.SerializableAttribute]
    public class Ball : ScriptableObject
    {
        public int numLeds = 12;
        Color[] ledColors;

        void Init()
        {
            ledColors = new Color[numLeds];
        }
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Sequencer : MonoBehaviour
{
    public int
        length = 32,
        numBalls = 10;
    public float bpm = 80f;
    public float playTime = 0;
    public float duration;
    public List<PatternPosition> patternList = new List<PatternPosition>();

    public Color[] GetColors(float time, int ballIndex)
    {
        var pp = patternList.Where(b => b.time < time && time < b.time + b.pattern.duration).FirstOrDefault();
        if (pp != null)
            return pp.pattern.GetColors(time - pp.time, ballIndex + pp.ballIndex);

        return new Color[1] { Color.black };
    }
    // Use this for initialization
    void Start()
    {
        duration = (float)length * (60f / bpm);
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
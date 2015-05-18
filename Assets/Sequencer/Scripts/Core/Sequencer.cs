using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sequencer : MonoBehaviour
{
    public float
        bpm,
        time;
    public List<PatternPosition> patternList = new List<PatternPosition>();

    // Use this for initialization
    void Start()
    {

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
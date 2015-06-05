using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Osc;

public class Sequencer : MonoBehaviour
{
    public int
        length = 32,
        numBalls = 10,
        numLeds = 12;
    public AudioClip sound;
    public float bpm = 80f;
    public float playTime = 0;
    public float duration;
    public List<PatternPosition> patternList = new List<PatternPosition>();
    public Ball[] balls;

    OscSender oscSender;
    Anoball[] anoballs;

    public Color[] GetColors(float time, int ballIndex)
    {
        var pp = patternList
            .Where(b => b.time < time && time < b.time + b.pattern.duration)
            .OrderBy(b => b.time)
            .LastOrDefault();
        if (pp != null)
            return pp.pattern.GetColors(time - pp.time, ballIndex + pp.ballIndex);

        return Enumerable.Repeat<Color>(new Color(0, 0, 0, 0.75f), numLeds).ToArray();
    }
    // Use this for initialization
    void Start()
    {
        duration = (float)length * (60f / bpm);
        balls = new Ball[numBalls];
        for (var i = 0; i < numBalls; i++)
        {
            balls[i] = new Ball();
            balls[i].id = i;
        }
        oscSender = FindObjectOfType(typeof(OscSender)) as OscSender;
        anoballs = FindObjectsOfType(typeof(Anoball)) as Anoball[];
        anoballs = anoballs.OrderBy(b => b.name).ToArray();
    }


    public IEnumerator PlayStart()
    {
        playTime = 0;
        while (playTime < length)
        {
            playTime += Time.deltaTime * (bpm / 60f);
            for (var i = 0; i < numBalls; i++)
            {
                var b = balls[i];
                b.ledColors = GetColors(playTime, b.id);
                SendOSC(b);
                if (anoballs[i] != null)
                    anoballs[i].SetLightColors(b.ledColors);
            }
            yield return 0;
        }
    }

    void SendOSC(Ball b)
    {
        var ballAddress = "/ball" + b.id.ToString();
        for (var i = 0; i < b.ledColors.Length; i++)
        {
            var ledAddress = ballAddress + "/" + i.ToString();
            int colorCode = ColorToCode(b.ledColors[i]);
            MessageEncoder me = new MessageEncoder(ledAddress);
            me.Add(colorCode);
            //             Debug.Log(ledAddress + ", " + colorCode);
            oscSender.Send(me);
        }
    }
    int ColorToCode(Color c)
    {
        Color32 c32 = c;
        return ((c32.r << 8) + c32.g << 8) + c32.b;
    }

    [System.SerializableAttribute]
    public class Ball
    {
        public int id;
        public Color[] ledColors;

    }
}
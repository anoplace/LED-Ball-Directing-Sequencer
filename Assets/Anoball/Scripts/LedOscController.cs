using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Osc;

public class LedOscController : MonoBehaviour
{
    OscSender oSender;
    Anoball[] anoballs;
    Ball[] balls;
    // Use this for initialization
    void Start()
    {
        oSender = FindObjectOfType(typeof(OscSender)) as OscSender;
        anoballs = FindObjectsOfType(typeof(Anoball)) as Anoball[];
        anoballs = anoballs.OrderBy(b => b.name).ToArray();
    }

    public void SendToBall(Color[] cs, float duration, int ballId = 0)
    {
        var address = "/ball/" + ballId;
        var mEnc = new MessageEncoder(address);

        var miliSec = (int)(duration * 1000);
        Debug.Log(duration + ", " + miliSec);
        mEnc.Add(0);
        mEnc.Add(miliSec);
		foreach(var c in cs)
            mEnc.Add(ColorToCode(c));

        anoballs[ballId].SetGradientColors(cs, duration);
        oSender.Send(mEnc);
    }
	
    int ColorToCode(Color c)
    {
        Color32 c32 = c;
        return ((c32.r << 8) + c32.g << 8) + c32.b;
    }

	[System.SerializableAttribute]
    public class Pallet
    {
        public Color[] colors = new Color[12];
    }
    [System.SerializableAttribute]
    public class Ball
    {
        public int id;
        public Color[] ledColors;

    }
}

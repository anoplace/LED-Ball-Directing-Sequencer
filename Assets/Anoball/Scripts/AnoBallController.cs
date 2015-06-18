using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AnoBallController : MonoBehaviour
{
    public Color[] colors = new Color[10];
    public int[] bpms = new int[] { 121, 129, 163 };
    public float ctrlDelta = 0.05f;
    public float ctrlDuration = 1f;
    public int currentBpm = 121;
    public int gradientDelta = 3;
    public int forward = 3;
    int currentColorIndex = 0;

    LedOscController oscController;
    int numBalls = 10;
    int numLeds = 12;
    float oneShot
    {
        get
        {
            return 60f / (float)currentBpm;
        }
    }
    float bpmDelta
    {
        get
        {
            return Mathf.Max(0.05f, oneShot * ctrlDelta);
        }
    }
    float bpmDuration
    {
        get
        {
            return oneShot * ctrlDuration;
        }
    }

    // Use this for initialization
    void Start()
    {
        oscController = GetComponent<LedOscController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StopAllCoroutines();
                StartCoroutine(OrderGradientToNext(bpmDelta, bpmDuration));
            }
            if (Input.GetKeyDown(KeyCode.Return))
            {
                StopAllCoroutines();
                StartCoroutine(SyncGradientToNext(bpmDuration, Input.GetKey(KeyCode.RightShift)));
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {

            }
            if (Input.GetKeyDown(KeyCode.G))
                gradientDelta = 3;
            if (Input.GetKeyDown(KeyCode.C))
                gradientDelta = 0;
            if (Input.GetKeyDown(KeyCode.F))
            {
                StopAllCoroutines();
                StartCoroutine(FadeOut(bpmDuration));
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
                ctrlDuration = 1;
            if (Input.GetKeyDown(KeyCode.Alpha2))
                ctrlDuration = 2;
            if (Input.GetKeyDown(KeyCode.Alpha3))
                ctrlDuration = 4;

            if (Input.GetKeyDown(KeyCode.Alpha4))
                ctrlDelta = 0.1f;
            if (Input.GetKeyDown(KeyCode.Alpha5))
                ctrlDelta = 0.5f;
            if (Input.GetKeyDown(KeyCode.Alpha6))
                ctrlDelta = 1f;

            if (Input.GetKeyDown(KeyCode.Alpha7))
                currentBpm = bpms[0];
            if (Input.GetKeyDown(KeyCode.Alpha8))
                currentBpm = bpms[1];
            if (Input.GetKeyDown(KeyCode.Alpha9))
                currentBpm = bpms[2];

        }
    }

    Color[] GetGradientColors(Color c1, Color c2)
    {
        var cs = new Color[numLeds];
        for (var i = 0; i < numLeds; i++)
            cs[i] = Color.Lerp(c1, c2, (float)i / (float)numLeds);
        return cs;
    }

    IEnumerator SyncGradientToNext(float duration, bool changeHalf = false)
    {
        var delta = 0.05f;
        ColorForward();
        var nextColors = GetGradientColors(GetColor(currentColorIndex), GetColor(currentColorIndex + gradientDelta));
        for (var i = 0; i < numBalls; i++)
        {
            if (changeHalf && i % 2 == currentColorIndex % 2)
                oscController.SendToBall(nextColors, Mathf.Max(0.1f, duration - delta * (float)i), i);
            yield return new WaitForSeconds(delta);
        }
    }
    IEnumerator OrderGradientToNext(float delta, float duration)
    {
        ColorForward();
        for (var i = 0; i < numBalls; i++)
        {
            var nextColors = GetGradientColors(GetColor(currentColorIndex + i), GetColor(currentColorIndex + gradientDelta + i));
            oscController.SendToBall(nextColors, duration, i);
            yield return new WaitForSeconds(delta);
        }
    }

    IEnumerator FadeOut(float duration)
    {
        var delta = 0.05f;
        for (var i = 0; i < numBalls; i++)
        {
            var nextColors = Enumerable.Repeat<Color>(Color.black, numLeds).ToArray();
            oscController.SendToBall(nextColors, Mathf.Max(0.1f, duration - delta * (float)i), i);
            yield return new WaitForSeconds(delta);
        }
    }

    void ColorForward()
    {
        currentColorIndex = (currentColorIndex + forward) % colors.Length;
    }

    Color GetColor(int colorIndex)
    {
        colorIndex = (colorIndex + 1) % colors.Length;
        return colors[colorIndex];
    }

}

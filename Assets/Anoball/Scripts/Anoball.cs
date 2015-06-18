using UnityEngine;
using System.Collections;
using System.Linq;

public class Anoball : MonoBehaviour
{
    Light[] lits;
    //     IEnumerator coroutine;

    public void SetLightColors(Color[] cs)
    {
        for (var i = 0; i < lits.Length && i < cs.Length; i++)
            lits[i].color = cs[i];
    }
    public void SetGradientColors(Color[] cs, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(GradientColorTo(cs, duration));
    }

    IEnumerator GradientColorTo(Color[] toColors, float duration)
    {
        var fromColors = lits.Select(b => b.color).ToArray();
        float t = 0;
        while (t < 1f)
        {
            for (var i = 0; i < lits.Length && i < toColors.Length; i++)
                lits[i].color = Color.Lerp(fromColors[i], toColors[i], t);
            yield return t += Time.deltaTime / duration;
        }
        for (var i = 0; i < lits.Length && i < toColors.Length; i++)
            lits[i].color = toColors[i];
    }

    // Use this for initialization
    void Start()
    {
        lits = GetComponentsInChildren<Light>().OrderBy(b => b.name).ToArray();
    }

}

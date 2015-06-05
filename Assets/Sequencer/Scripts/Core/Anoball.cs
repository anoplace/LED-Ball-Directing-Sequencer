using UnityEngine;
using System.Collections;
using System.Linq;

public class Anoball : MonoBehaviour {
    Light[] lits;

    public void SetLightColors(Color[] cs)
    {
        for (var i = 0; i < lits.Length && i < cs.Length; i++)
            lits[i].color = cs[i];
    }
    // Use this for initialization
    void Start () {
        lits = GetComponentsInChildren<Light>().OrderBy(b => b.name).ToArray();
    }
}

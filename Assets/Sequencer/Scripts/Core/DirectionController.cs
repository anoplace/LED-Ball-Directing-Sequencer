using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DirectionController : MonoBehaviour
{
    public Direction[] directions;
    public Sequencer playingSequencer;


    // Use this for initialization
    IEnumerator Start()
    {
        var audio = GetComponent<AudioSource>();
        for (var i = 0; i < directions.Length; i++)
        {
            Debug.Log(i);
            var d = directions[i];
            switch (d.type)
            {
                case DirectionType.playSequencer:
                    playingSequencer = d.sequencer;
                    if (d.aClip != null){
                        audio.clip = d.aClip;
                        audio.Play();
                    }
                    yield return StartCoroutine(playingSequencer.PlayStart());
                    break;
                case DirectionType.waitForSeconds:
                    yield return new WaitForSeconds(d.waitTime);
                    break;
                case DirectionType.waitForKey:
                    var b = false;
                    while (!b)
                    {
                        b = Input.GetKeyDown(d.key);
                        yield return 0;
                    }
                    break;
            }
        }
        yield return 0;
    }

    [System.SerializableAttribute]
    public class Direction
    {
        public DirectionType type;
        public Sequencer sequencer;
        public AudioClip aClip;
        public float waitTime = 5f;
        public KeyCode key;
    }
    public enum DirectionType
    {
        playSequencer = 0,
        waitForSeconds = 1,
        waitForKey = 2,
    }
}

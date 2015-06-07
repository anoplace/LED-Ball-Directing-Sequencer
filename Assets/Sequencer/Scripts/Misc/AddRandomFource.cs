using UnityEngine;
using System.Collections;

public class AddRandomFource : MonoBehaviour
{

    Rigidbody r;

    void Start()
    {
        r = GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (Random.value < Time.fixedTime)
            r.AddForce(Random.insideUnitSphere, ForceMode.Impulse);
    }
}

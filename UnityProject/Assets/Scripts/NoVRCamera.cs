using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoVRCamera : MonoBehaviour
{
    [SerializeField]
    Transform[] head;
    [SerializeField]
    float headHeight = 1.79f;
    // Start is called before the first frame update
    void Start()
    {
        if (!ExampleUtil.isPresent())
            foreach (var t in head)
                t.localPosition = headHeight * Vector3.up;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafePointManager : MonoBehaviour
{
    [SerializeField]
    SafePoint _lastPoint;

    internal void TriggerEnter(SafePoint point, Collider other)
    {
        if (other.tag == "Player")
            _lastPoint = point;
    }

    public Transform GetLastPoint()
    {
        return _lastPoint.transform;
    }
}

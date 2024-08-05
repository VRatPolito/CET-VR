using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SafePoint : MonoBehaviour
{
    SafePointManager _manager;

    private void Awake()
    {
        _manager = FindObjectOfType<SafePointManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        _manager.TriggerEnter(this, other);
    }
}

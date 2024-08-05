using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class TriggerEvent : UnityEvent<GameObject, Collider> { }

public class TriggerEventRouter : MonoBehaviour
{
    public TriggerEvent OnTriggerEnterEvent;
    public TriggerEvent OnTriggerExitEvent;

    private void OnTriggerEnter(Collider other)
    {
        OnTriggerEnterEvent?.Invoke(gameObject, other);
    }
    private void OnTriggerExit(Collider other)
    {
        OnTriggerExitEvent?.Invoke(gameObject, other);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class CollisionEvent : UnityEvent<GameObject, GameObject> { };


public class CollisionManager : MonoBehaviour
{
    public CollisionEvent collisionDetected;

    internal void SignalCollision(GameObject first, GameObject other)
    {
        collisionDetected?.Invoke(first, other);
    }
}

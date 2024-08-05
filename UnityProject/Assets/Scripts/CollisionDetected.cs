using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetected : MonoBehaviour
{
    [SerializeField]
    CollisionManager _manager;
    // Start is called before the first frame update

    private void OnCollisionEnter(Collision collision)
    {
        if(_manager != null)
            _manager.SignalCollision(gameObject, collision.collider.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_manager != null)
            _manager.SignalCollision(gameObject, other.gameObject);
    }
}

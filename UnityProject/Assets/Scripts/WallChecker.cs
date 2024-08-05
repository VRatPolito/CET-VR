using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WallChecker : MonoBehaviour
{
    public UnityEvent<PlayerBreak> OnWallSurpassed = new UnityEvent<PlayerBreak>();

    [SerializeField]
    PlayerBreak _wall;
    [SerializeField]
    WallChecker _nextWall;


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            OnWallSurpassed?.Invoke(_wall);
            _nextWall.GetComponent<BoxCollider>().enabled = true;
            GetComponent<BoxCollider>().enabled = false;
        }
    }
}

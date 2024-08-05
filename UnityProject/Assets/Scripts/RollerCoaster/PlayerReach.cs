using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerReach : MonoBehaviour
{

    RollerCoasterLevelManager levManager;
    [SerializeField]
    bool isLastSign = false; //to identify the last sign
    private bool stop = false;

    private void Awake()
    {
        if(GameManager.Instance != null)
            levManager = (RollerCoasterLevelManager) GameManager.Instance.LevelManager;
        else
            levManager = FindObjectOfType<RollerCoasterLevelManager>(); 
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == levManager.cartBase)
        {
            if (levManager.isLastLap && this.isLastSign)
                stop = true;

            levManager.signs.activateNextSign(stop);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideIfMoving : MonoBehaviour
{
    [SerializeField]
    MSVehicleControllerFree _car;
    [SerializeField]
    float _speedThreshold = 0;
    SpriteRenderer _spriteRenderer;
    // Start is called before the first frame update
    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if ((_speedThreshold == 0 && _car.KMh == 0) ||  (_speedThreshold > 0 && _car.KMh < _speedThreshold))
        {
            _spriteRenderer.enabled = true;
        }
        else
            _spriteRenderer.enabled =false;
    }
}

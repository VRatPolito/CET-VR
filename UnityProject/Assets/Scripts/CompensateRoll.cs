using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CompensateRoll : MonoBehaviour
{
    [SerializeField]
    Transform _car;
  
    // Update is called once per frame
    void LateUpdate()
    {
        if(_car != null && transform.childCount > 0)
        {
            var a = _car.eulerAngles;
            transform.GetChild(0).eulerAngles = new Vector3(transform.GetChild(0).eulerAngles.x, transform.GetChild(0).eulerAngles.y, -a.z);
        }
    }
}

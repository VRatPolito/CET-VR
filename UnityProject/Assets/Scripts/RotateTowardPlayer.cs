using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTowardPlayer : MonoBehaviour
{
    [SerializeField]
    Transform _playerCamera;

    void Update()
    {
        this.gameObject.transform.LookAt(_playerCamera.transform, Vector3.up);
        Quaternion rot = this.transform.rotation;

        rot = Quaternion.Euler(rot.eulerAngles.x, rot.eulerAngles.y + 180, rot.eulerAngles.z);

        this.transform.rotation = rot;
    }
}

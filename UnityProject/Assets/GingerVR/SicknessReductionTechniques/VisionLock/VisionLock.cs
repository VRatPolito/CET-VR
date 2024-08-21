using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[ExecuteInEditMode]
public class VisionLock : MonoBehaviour
{
    [SerializeField]
    Transform _cameraOffset;
    [SerializeField]
    Transform _playerHead;
    Vector3 offset;
    public GameObject megaParent;
    [SerializeField]
    InputActionProperty _inputAction;
    bool _locking = false;
  
    Quaternion _initialRot = Quaternion.identity;
    Vector3 _initialPos = Vector3.zero;
    Quaternion _initialCamRot = Quaternion.identity;
    Vector3 _initialCamPos = Vector3.zero;
    bool lockNow = false;
    // Start is called before the first frame update
    void Start()
    {
        offset = new Vector3(0,0,0);
    }
    private void OnEnable()
    {
        _inputAction.action.Enable();
    }
    private void OnDisable()
    {
        _inputAction.action.Disable();
    }
    // Update is called once per frame
    void LateUpdate()
    {
        if (megaParent != null)
        {
            transform.position = Camera.main.gameObject.transform.position;
            transform.eulerAngles = (Camera.main.gameObject.transform.rotation).eulerAngles;
             if (Convert.ToBoolean(_inputAction.action.ReadValue<float>()))
            {
                megaParent.transform.parent = Camera.main.transform;
            }
            else
            {
                megaParent.transform.parent = null;
            }
        }
        else
        {
            lockNow = Convert.ToBoolean(_inputAction.action.ReadValue<float>());

            if(lockNow)
            {
                if (!_locking)
                {
                    _initialPos = _cameraOffset.localPosition;
                    _initialRot = _cameraOffset.localRotation;
                    _initialCamPos = _playerHead.localPosition;
                    _initialCamRot = _playerHead.localRotation;
                    _locking = true;
                    }
            }
            else
            {
                _locking = false;
                _initialPos = Vector3.zero;
                _initialRot = Quaternion.identity;
                _initialCamRot = Quaternion.identity;
                _initialCamPos = Vector3.zero;
            }

            if (_locking)
            {
                _cameraOffset.localPosition = _initialCamPos + _initialPos - _playerHead.localPosition;
                _cameraOffset.localRotation = _initialCamRot * _initialRot * Quaternion.Inverse(_playerHead.localRotation);
            }
        }
    }
}

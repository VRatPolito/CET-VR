using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GetInAndOut : MonoBehaviour
{
    [SerializeField]
    Transform _vehicleSeat;
    InputManagement _input;
    [SerializeField]
    bool _initiallyIn = false;
    [SerializeField]
    MSSceneControllerFree _controller;
    bool _iN = false;
    [SerializeField]
    bool _canGetOut = false;
    [SerializeField]
    float _verticalOffset = 2;
    FakeParenting _parenting;
    private void Awake()
    {
        _input = GetComponent<InputManagement>();
        _parenting = GetComponent<FakeParenting>();
    }
    // Start is called before the first frame update
    void Start()
    {
        if (_initiallyIn)
            Invoke(nameof(GetIn), 0.1f);
    }

    private void GetIn()
    {
        if(_vehicleSeat != null)
        {
            var c = GetComponent<CharacterController>();
            c.enabled = false;
            var cv = GetComponent<CharacterControllerVR>();
            cv.enabled = false;
            var j = GetComponent<JoystickMovement>();
            j.enabled = false;
            var l = GetComponent<SquaredLimitTracking>();
            l.enabled = true;
            l.Reset();
            _parenting.SetFakeParent(_vehicleSeat, _verticalOffset * Vector3.up, Quaternion.identity, true);
            
            _controller.enabled = true;
            if(_vehicleSeat.GetComponentInParent<HideMeshes>()._hideMeshes)
                _vehicleSeat.GetComponentInParent<HideMeshes>().Hide();
            _vehicleSeat.GetComponentInParent<MSVehicleControllerFree>().EnterInVehicle();
            _iN = true;
        }
    }
    private void GetOut()
    {
        if (_vehicleSeat != null)
        {
            _parenting.SetFakeParent(null);
            if (_vehicleSeat != null)
                transform.Translate(1.5f*Vector3.left, Space.Self);
            transform.Find("XR Origin").localPosition = Vector3.zero;
            var c = GetComponent<CharacterController>();
            c.enabled = true;
            var cv = GetComponent<CharacterControllerVR>();
            cv.enabled = true;
            var j = GetComponent<JoystickMovement>();
            j.enabled = true;
            var l = GetComponent<SquaredLimitTracking>();
            l.enabled = false;
            _controller.enabled = false;
        _vehicleSeat.GetComponentInParent<HideMeshes>().Show();
         _vehicleSeat.GetComponentInParent<MSVehicleControllerFree>().ExitTheVehicle();
        _iN = false;
        }
    }
    public void ToggleInOut()
    {
        if(_iN)
            GetOut();
        else
            GetIn();
    }
    // Update is called once per frame
    void Update()
    {
        if(_canGetOut && (Keyboard.current[Key.LeftCtrl].isPressed && Keyboard.current[Key.V].wasPressedThisFrame || (_input.IsLeftGripped && _input.IsRightGrippedDown) || (_input.IsLeftGrippedDown && _input.IsRightGripped)))
        {
            ToggleInOut();
        }
    }
}

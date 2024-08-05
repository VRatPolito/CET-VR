using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Spectate : MonoBehaviour
{
    [SerializeField]
    Camera _spectatorCamera;
    [SerializeField]
    Transform _spectatorRig;
    bool _isFoggy = false;
    bool _spectating = false;
    private void Awake()
    {
        _isFoggy = RenderSettings.fog;
    }
    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current[Key.LeftCtrl].isPressed && Keyboard.current[Key.S].wasPressedThisFrame)
            {
            if(_spectating)
            {
                _spectatorRig.gameObject.SetActive(false);
                _spectatorCamera.enabled = false;
                _spectating= false;
                Cursor.visible = true; 
                RenderSettings.fog = _isFoggy;
            }
            else
            {
                _spectatorRig.gameObject.SetActive(true);
                _spectatorCamera.enabled = true;
                _spectating = true;
                Cursor.visible = false; 
                RenderSettings.fog = false;
            }
            }
    }
}

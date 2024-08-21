using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HeadLockVR : MonoBehaviour
{
    bool cameraLocked = false;
    Quaternion lockedRotation;
    public GameObject parent; 
    PlayerInputActions inputActions;
    InputAction jump;
    // Start is called before the first frame update
    void Awake()
    {
        if (GameManager.Instance != null)
            inputActions = GameManager.Instance._playerInputActions;
        else
            inputActions = new PlayerInputActions();

        jump = inputActions.Player.Jump;
    }

    private void OnEnable()
    {
        if (GameManager.Instance == null)
            jump.Enable();
    }

    private void OnDisable()
    {
        if (GameManager.Instance == null)
            jump.Disable();
    }

    void Update(){
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 cameraAngles = Camera.main.transform.localEulerAngles;
        Vector3 parentAngles = new Vector3(cameraAngles.x-90, cameraAngles.y-90, cameraAngles.z-90);

        parent.transform.eulerAngles = parentAngles;
        //target.transform.rotation = Camera.main.transform.rotation * -1;
        //target.transform.Rotate(-cameraRotation.x, -cameraRotation.y, -cameraRotation.z);
        //Debug.Log(Camera.main.transform.localEulerAngles);
        //Camera.main.transform.LookAt(target.transform);
        //OVRManager.display.RecenterPose(); 
        //Camera.main.transform.rotation = lockedRotation;
        if(jump.ReadValue<bool>()){
            Debug.Log("Locked Camera");
            if(!cameraLocked){
                cameraLocked = true;
                lockedRotation = Quaternion.identity;
            }
            Camera.main.transform.rotation = lockedRotation;
        }else if(!jump.ReadValue<bool>())
        {
            cameraLocked = false;
        }
    }
}

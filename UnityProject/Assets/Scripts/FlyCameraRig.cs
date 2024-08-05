using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FlyCameraRig : MonoBehaviour
{
    /*
   Writen by Windexglow 11-13-10.  Use it, edit it, steal it I don't care.  
   Simple flycam I made, since I couldn't find any others made public.  
   Made simple to use (drag and drop, done) for regular keyboard layout  
   wasd : basic movement
   shift : Makes camera accelerate
   space : Moves camera on X and Z axis only.  So camera doesn't gain any height*/

    [SerializeField]
    float mainSpeed = 100.0f; //regular speed
    [SerializeField]
    float shiftAdd = 250.0f; //multiplied by how long shift is held.  Basically running
    [SerializeField]
    float maxShift  = 1000.0f; //Maximum speed when holdin gshift
    [SerializeField]
    float camSens = 0.25f; //How sensitive it with mouse
    [SerializeField]
    bool _useArrows = true;
    [SerializeField]
    bool _disableInVR = false;
    private Vector2 lastMouse = new Vector3(255, 255); //kind of in the middle of the screen, rather than at the top (play)
    private float totalRun = 1.0f;

    private void Start()
    {
        if (!ExampleUtil.isPresent())
            Cursor.visible = false;
    }

    void Update()
    {
        if (_disableInVR && ExampleUtil.isPresent())
            enabled = false;
        else
        {
            lastMouse = Mouse.current.position.ReadValue() - lastMouse;
            lastMouse = new Vector2(-lastMouse.y * camSens, lastMouse.x * camSens);
            lastMouse = new Vector2(transform.eulerAngles.x + lastMouse.x, transform.eulerAngles.y + lastMouse.y);
            transform.eulerAngles = lastMouse;
            lastMouse = Mouse.current.position.ReadValue();
            //Mouse  camera angle done.  

            //Keyboard commands
            var f = 0.0f;
            var p = GetBaseInput();
            if ((_useArrows && Keyboard.current[Key.RightShift].isPressed) || (!_useArrows && Keyboard.current[Key.LeftShift].isPressed))
            {
                totalRun += Time.deltaTime;
                p = p * totalRun * shiftAdd;
                p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
                p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
                p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
            }
            else
            {
                totalRun = Mathf.Clamp(totalRun * 0.5f, 1, 1000);
                p = p * mainSpeed;
            }

            p = p * Time.deltaTime;
            if (Keyboard.current[Key.Space].isPressed)
            { //If player wants to move on X and Z axis only
                f = transform.position.y;
                transform.Translate(p);
                transform.position = new Vector3(transform.position.x, f, transform.position.z);
            }
            else
            {
                transform.Translate(p);
            }
        }
    }

    private Vector3 GetBaseInput() { //returns the basic values, if it's 0 than it's not active.
    Vector3 p_Velocity = Vector3.zero;
        if (_useArrows)
        {
            if (Keyboard.current[Key.UpArrow].isPressed)
                p_Velocity += new Vector3(0, 0, 1);
            if (Keyboard.current[Key.DownArrow].isPressed)
                p_Velocity += new Vector3(0, 0, -1);
            if (Keyboard.current[Key.LeftArrow].isPressed)
                p_Velocity += new Vector3(-1, 0, 0);
            if (Keyboard.current[Key.RightArrow].isPressed)
                p_Velocity += new Vector3(1, 0, 0);
        }
    else
        {
            if (Keyboard.current[Key.W].isPressed)
                p_Velocity += new Vector3(0, 0, 1);
            if (Keyboard.current[Key.S].isPressed)
                p_Velocity += new Vector3(0, 0, -1);
            if (Keyboard.current[Key.A].isPressed)
                p_Velocity += new Vector3(-1, 0, 0);
            if (Keyboard.current[Key.D].isPressed)
                p_Velocity += new Vector3(1, 0, 0);
        }
    return p_Velocity;
    }
}

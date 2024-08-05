using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SimplePlayerController : MonoBehaviour
{
    bool canUp, canDown, canLeft, canRight = false;

    PlayerInputActions actions;
    InputAction move;
    Rigidbody rb;
    float speed = 10;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        actions = new PlayerInputActions();
        move = actions.Player.Move;
    }

    private void OnEnable()
    {
        move.Enable();
    }

    private void OnDisable()
    {
        move.Disable();
    }

    void Move(Vector2 v){
        rb.velocity = new Vector3(v.x*speed, v.x*speed/2, v.y*speed);
    }

    // Update is called once per frame
    void Update()
    {
       Move(move.ReadValue<Vector2>());
    }
}


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class JoystickMovement : MonoBehaviour
{
    [SerializeField]
    private float _walkSpeed = 5;
    [SerializeField]
    private float _runSpeed = 8;
    [SerializeField]
    private float _jumpHeight = 1.0f;
    [SerializeField]
    private AnimationCurve _jumpCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    [SerializeField]
    private float _jumpTime = 0.5f;
    private float _jumpStart;
    public bool Blocked
    {
        get { return _blocked; }
        set
        {
            if (value && !_blocked)
            {
                if (_bigArrow != null)
                    _bigArrow.gameObject.SetActive(false);
                _lastSpeed = 0;
                _lastDir = Quaternion.identity;
                _factor = 0;
                _rightPadDown = false;
                _leftPadDown = false;
            }

            _blocked = value;
        }
    }
    bool _blocked = false;
    Transform _leftController, _rightController;
    [SerializeField]
    Transform _bigArrow;
    CharacterControllerVR _target;
    InputManagement _input;
    bool _leftPadDown = false;
    bool _leftMotion = false;
    bool _rightMotion = false;
    bool _jumping = false;
    bool _rightPadDown = false;
    float _factor = 0;
    float _smoothStep = 0.05f;
    float _lastSpeed = 0;
    Quaternion _lastDir = Quaternion.identity;
    float _prevEvalutation;
    Vector2 _leftPadAxis = Vector2.zero;
    Vector2 _rightPadAxis = Vector2.zero;
    Vector3 moveDirection = Vector3.zero;

    // Start is called before the first frame update
    void Awake()
    {
        _input = GetComponent<InputManagement>();
        _target = GetComponent<CharacterControllerVR>();
        _leftController = _input.LeftController.transform;
        _rightController = _input.RightController.transform;
        if (_bigArrow != null)
            _bigArrow.gameObject.SetActive(false);
    }
    private void Start()
    {
        switch (_input._axisMode)
        {
            case InputMode.Click:
                _input.OnLeftPadPressed += LeftPadDown;
                _input.OnLeftPadUnpressed += LeftPadUp;
                _input.OnRightPadPressed += RightPadDown;
                _input.OnRightPadUnpressed += RightPadUp;
                break;
            case InputMode.Touch:
                _input.OnLeftPadTouched += LeftPadDown;
                _input.OnLeftPadUntouched += LeftPadUp;
                _input.OnRightPadTouched += RightPadDown;
                _input.OnRightPadUntouched += RightPadUp;
                break;
        }
    }
    private void RightPadUp(object sender)
    {
            _rightPadAxis = Vector2.zero;
            switch (_input._axisMode)
            {
                case InputMode.Click:
                    if (_input.IsLeftPadPressed && !Blocked)
                        _leftPadDown = true;
                    else if (_bigArrow != null)
                        _bigArrow.gameObject.SetActive(false);
                    break;
                case InputMode.Touch:
                    _rightPadDown = false;
                    _rightMotion = false;
                    if (_input.IsLeftPadTouched && !Blocked)
                        {
                        _leftPadDown = true;
                        _leftPadAxis = new Vector2(_input.LeftPadAxis.padX, _input.LeftPadAxis.padY);
                        }
                    else if (_bigArrow != null)
                        _bigArrow.gameObject.SetActive(false);
                    break;
            }
    }

    private void RightPadDown(object sender, PadEventArgs e)
    {
        if (!Blocked && !_leftPadDown)
        {
            _rightPadDown = true;
            if (_bigArrow != null)
                _bigArrow.gameObject.SetActive(true);         
        }
    }

    private void LeftPadUp(object sender)
    {
        _leftPadDown = false;
        switch (_input._axisMode)
        {
            case InputMode.Click:
                if (_input.IsRightPadPressed && !Blocked)
                    _rightPadDown = true;
                else if (_bigArrow != null)
                    _bigArrow.gameObject.SetActive(false);
                break;
            case InputMode.Touch:
                _leftMotion = false;
                _leftPadAxis = Vector2.zero;
                if (_input.IsRightPadTouched && !Blocked)
                {
                    _rightPadDown = true;
                    _rightPadAxis = new Vector2(_input.RightPadAxis.padX, _input.RightPadAxis.padY);
                }
                else if (_bigArrow != null)
                    _bigArrow.gameObject.SetActive(false);
                break;
        }
    }

    private void LeftPadDown(object sender, PadEventArgs e)
    {
        if (!Blocked && !_rightPadDown)
        {           
            _leftPadDown = true;
            if (_bigArrow != null)
                _bigArrow.gameObject.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!Blocked)
        {
            _leftPadAxis = InputManagement.PadEventArgs(_input.LeftPadAxis);
            _rightPadAxis = InputManagement.PadEventArgs(_input.RightPadAxis);

            if (_input._axisMode == InputMode.Touch)
            {
                if (_leftPadAxis.y != 0)
                    _leftMotion = true; 
                if (_rightPadAxis.y != 0)
                    _rightMotion = true;
            }
            else
            {
                _leftMotion = _leftPadDown;
                _rightMotion = _rightPadDown;
            }

            if (_bigArrow != null)
            {
                if (_leftMotion)
                    _bigArrow.localEulerAngles = new Vector3(0, _leftController.localEulerAngles.y, 0);
                else if (_rightMotion)
                    _bigArrow.localEulerAngles = new Vector3(0, _rightController.localEulerAngles.y, 0);
            }

            moveDirection = CalculateMotion();
            moveDirection = ApplyGravityAndJump(moveDirection);
            _target.Move(moveDirection);

        }


    }

    private Vector3 ApplyGravityAndJump(Vector3 moveDirection)
    {
        if (!_jumping)
        {
            bool inputJump =  _target.isGrounded && (_input.IsLeftTriggerClickedDown || _input.IsRightTriggerClickedDown || Keyboard.current[Key.Space].wasPressedThisFrame);
            if (inputJump)
            {
                _jumpStart = Time.time;
                _prevEvalutation = _jumpCurve.Evaluate(0) * _jumpHeight;
                moveDirection -= Physics.gravity * Time.deltaTime;
                _jumping = true;
            }
        }
        else
        {
            var now = Time.time;
            if(Time.time > _jumpStart + _jumpTime)
            {
                now = _jumpStart + _jumpTime;
                _jumping = false;
            }

            var cursor = Mathf.InverseLerp(_jumpStart, _jumpStart + _jumpTime, now);
            var newEval = _jumpCurve.Evaluate(cursor) * _jumpHeight;
            var diff = newEval - _prevEvalutation;
            moveDirection.y = diff;
            moveDirection -= Physics.gravity * Time.deltaTime;
            _prevEvalutation = newEval;
        }
        return moveDirection;
    }

    private Vector3 CalculateMotion()
    {
        Vector3 movement = Vector3.zero;

        if (_leftMotion || _rightMotion)
        {
            if (GameManager.Instance != null && GameManager.Instance._isLeftHanded)
            {
                if (_rightMotion)
                    CalculateRightMotion(out movement);
                else if (_leftMotion)
                    CalculateLeftMotion(out movement);
            }
            else
            {
                if (_leftMotion)
                    CalculateLeftMotion(out movement);
                else if (_rightMotion)
                    CalculateRightMotion(out movement);
            }
        }
        else if (_factor > 0)
        {
            movement = getForwardXZ(Time.deltaTime * _factor * _lastSpeed, _lastDir);
            _factor -= _smoothStep;
            if (_factor < 0)
                _factor = 0;
        }
        else if (_lastSpeed > 0)
        {
            _lastSpeed = 0;
            _lastDir = Quaternion.identity;
        }

        return movement;
    }

    private void CalculateRightMotion(out Vector3 movement)
    {
        _lastDir = _rightController.transform.rotation;
        if (_target.isGrounded)
        {
            if (_input.IsRightGripped)
                _lastSpeed = _runSpeed;
            else
            {
                float t = (_rightPadAxis.y + 1) / 2;
                _lastSpeed = Mathf.Lerp(0, _walkSpeed, t);
            }

            movement = getForwardXZ(Time.deltaTime * _factor * _lastSpeed, _lastDir);
            _factor += _smoothStep;
            if (_factor > 1)
                _factor = 1;
        }
        else
            movement = getForwardXZ(Time.deltaTime * _factor * _lastSpeed, _lastDir);
    }
    private void CalculateLeftMotion(out Vector3 movement)
    {
        _lastDir = _leftController.transform.rotation;
        if (_target.isGrounded)
        {
            if (_input.IsLeftGripped)
                _lastSpeed = _runSpeed;
            else
            {
                float t = (_leftPadAxis.y + 1) / 2;
                _lastSpeed = Mathf.Lerp(0, _walkSpeed, t);
            }

            movement = getForwardXZ(Time.deltaTime * _factor * _lastSpeed, _lastDir);
            _factor += _smoothStep;
            if (_factor > 1)
                _factor = 1;
        }
        else
            movement = getForwardXZ(Time.deltaTime * _factor * _lastSpeed, _lastDir);
    }

    // Returns a forward vector given the distance and direction
    public static Vector3 getForwardXZ(float forwardDistance, Quaternion direction)
    {
        return Vector3.Normalize(vector3XZOnly(direction * Vector3.forward)) * forwardDistance;
    }

    // Returns a Vector3 with only the X and Z components (Y is 0'd)
    public static Vector3 vector3XZOnly(Vector3 vec)
    {
        return new Vector3(vec.x, 0f, vec.z);
    }

    public float GetRunSpeed()
    {
        return _runSpeed;
    }
    public float GetWalkSpeed()
    {
        return _walkSpeed;
    }
}

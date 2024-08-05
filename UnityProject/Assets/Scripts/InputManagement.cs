using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using UnityEngine.XR.Interaction.Toolkit;

public struct PadEventArgs
{
    public float padX, padY;
}

public enum InputMode { Touch, Click };

public class InputManagement : MonoBehaviour
{
    [SerializeField]
    internal InputMode _axisMode = InputMode.Click;
    [SerializeField]
    internal GameObject LeftController;
    [SerializeField]
    internal GameObject RightController;

    [SerializeField]
    private InputActionProperty _leftGrabPinchAction;
    [SerializeField]
    private InputActionProperty _rightGrabPinchAction;
    [SerializeField]
    private InputActionProperty _leftGrabGripAction;
    [SerializeField]
    private InputActionProperty _rightGrabGripAction;
    [SerializeField]
    private InputActionProperty _leftTeleportAction;
    [SerializeField]
    private InputActionProperty _rightTeleportAction;
    [SerializeField]
    private InputActionProperty _leftTouchPadAction;
    [SerializeField]
    private InputActionProperty _rightTouchPadAction;
    [SerializeField]
    private InputActionProperty _leftTouchPosAction;
    [SerializeField]
    private InputActionProperty _rightTouchPosAction;
    private Vector2 _rightAxisTarget;
    private Vector2 _leftAxisTarget;
    private Vector2 _rightAxis;
    private Vector2 _leftAxis;

    public event Action<object> OnLeftTriggerClicked, OnLeftTriggerUnclicked, OnLeftPadUnpressed, OnRightPadUnpressed, OnLeftPadUntouched, OnRightPadUntouched, OnLeftGripped, OnLeftUngripped,
                               OnRightTriggerClicked, OnRightTriggerUnclicked, OnRightGripped, OnRightUngripped;
    public event Action<object, PadEventArgs> OnLeftPadPressed, OnRightPadPressed, OnLeftPadTouched, OnRightPadTouched;

    private bool _leftTriggerClicked, _rightTriggerClicked, _leftPadPressed, _rightPadPressed, _leftGripped, _rightGripped, _leftPadTouched, _rightPadTouched;
    private bool _leftTriggerClickedFrame, _rightTriggerClickedFrame, _leftPadPressedFrame, _rightPadPressedFrame, _leftGrippedFrame, _rightGrippedFrame, _leftPadTouchedFrame, _rightPadTouchedFrame;
    private bool _leftTriggerUnclickedFrame, _rightTriggerUnclickedFrame, _leftPadUnpressedFrame, _rightPadUnpressedFrame, _leftUngrippedFrame, _rightUngrippedFrame, _leftPadUntouchedFrame, _rightPadUntouchedFrame;
    private PadEventArgs _leftPadEventArgs, _rightPadEventArgs;

 
    public bool IsLeftTriggerClickedDown
    {
        get { return _leftTriggerClickedFrame; }
    }
    public bool IsRightTriggerClickedDown
    {
        get { return _rightTriggerClickedFrame; }
    }
    public bool IsLeftPadPressedDown
    {
        get { return _leftPadPressedFrame; }
    }
    public bool IsRightPadPressedDown
    {
        get { return _rightPadPressedFrame; }
    }
    public bool IsLeftPadTouchedDown
    {
        get { return _leftPadTouchedFrame; }
    }
    public bool IsRightPadTouchedDown
    {
        get { return _rightPadTouchedFrame; }
    }
    public bool IsLeftGrippedDown
    {
        get { return _leftGrippedFrame; }
    }
    public bool IsRightGrippedDown
    {
        get { return _rightGrippedFrame; }
    }

    public bool IsLeftTriggerClickedUp
    {
        get { return _leftTriggerUnclickedFrame; }
    }
    public bool IsRightTriggerClickedUp
    {
        get { return _rightTriggerUnclickedFrame; }
    }
    public bool IsLeftPadPressedUp
    {
        get { return _leftPadUnpressedFrame; }
    }
    public bool IsRightPadPressedUp
    {
        get { return _rightPadUnpressedFrame; }
    }
    public bool IsLeftPadTouchedUp
    {
        get { return _leftPadUntouchedFrame; }
    }
    public bool IsRightPadTouchedUp
    {
        get { return _rightPadUntouchedFrame; }
    }
    public bool IsLeftGrippedUp
    {
        get { return _leftUngrippedFrame; }
    }
    public bool IsRightGrippedUp
    {
        get { return _rightUngrippedFrame; }
    }
    public Vector2 RightAxis
    {
        get { return _rightAxis; }
    }
    public Vector2 LeftAxis
    {
        get { return _leftAxis; }
    }

    public bool IsLeftTriggerClicked
    {
        private set
        {
            if (value != _leftTriggerClicked)
            {
                if (value && OnLeftTriggerClicked != null)
                    OnLeftTriggerClicked.Invoke(this);
                else if (!value && OnLeftTriggerUnclicked != null)
                    OnLeftTriggerUnclicked.Invoke(this);
            }
            _leftTriggerClicked = value;
        }
        get { return _leftTriggerClicked; }
    }

    public bool IsRightTriggerClicked
    {
        private set
        {
            if (value != _rightTriggerClicked)
            {
                if (value && OnRightTriggerClicked != null)
                    OnRightTriggerClicked.Invoke(this);
                else if (!value && OnRightTriggerUnclicked != null)
                    OnRightTriggerUnclicked.Invoke(this);
            }
            _rightTriggerClicked = value;
        }
        get { return _rightTriggerClicked; }
    }

    public bool IsLeftPadPressed
    {
        private set
        {
            if (value != _leftPadPressed)
            {
                if (value && OnLeftPadPressed != null)
                    OnLeftPadPressed.Invoke(this, LeftPadAxis);
                else if (!value && OnLeftPadUnpressed != null)
                    OnLeftPadUnpressed.Invoke(this);
            }
            _leftPadPressed = value;
        }
        get { return _leftPadPressed; }
    }

    public bool IsRightPadPressed
    {
        private set
        {
            if (value != _rightPadPressed)
            {
                if (value && OnRightPadPressed != null)
                    OnRightPadPressed.Invoke(this, RightPadAxis);
                else if (!value && OnRightPadUnpressed != null)
                    OnRightPadUnpressed.Invoke(this);
            }
            _rightPadPressed = value;
        }
        get { return _rightPadPressed; }
    }

    public bool IsLeftPadTouched
    {
        private set
        {
            if (value != _leftPadTouched)
            {
                if (value && OnLeftPadTouched != null)
                    OnLeftPadTouched.Invoke(this, LeftPadAxis);
                else if (!value && OnLeftPadUntouched != null)
                    OnLeftPadUntouched.Invoke(this);
            }
            _leftPadTouched = value;
        }
        get { return _leftPadTouched; }
    }

    public bool IsRightPadTouched
    {
        private set
        {
            if (value != _rightPadTouched)
            {
                if (value && OnRightPadTouched != null)
                    OnRightPadTouched.Invoke(this, RightPadAxis);
                else if (!value && OnRightPadUntouched != null)
                    OnRightPadUntouched.Invoke(this);
            }
            _rightPadTouched = value;
        }
        get { return _rightPadTouched; }
    }

    public bool IsLeftGripped
    {
        private set
        {
            if (value != _leftGripped)
            {
                if (value && OnLeftGripped != null)
                    OnLeftGripped.Invoke(this);
                else if (!value && OnLeftUngripped != null)
                    OnLeftUngripped.Invoke(this);
            }
            _leftGripped = value;
        }
        get { return _leftGripped; }
    }

    public bool IsRightGripped
    {
        private set
        {
            if (value != _rightGripped)
            {
                if (value && OnRightGripped != null)
                    OnRightGripped.Invoke(this);
                else if (!value && OnRightUngripped != null)
                    OnRightUngripped.Invoke(this);
            }
            _rightGripped = value;
        }
        get { return _rightGripped; }
    }
	
		public PadEventArgs LeftPadAxis
    {
        private set {
            _leftPadEventArgs = value;
        }
        get
        {
            return _leftPadEventArgs;
        }
    }

    public PadEventArgs RightPadAxis
    {
        private set
        {
            _rightPadEventArgs = value;
        }
        get
        {
            return _rightPadEventArgs;
        }
    }
    private void Awake()
    {
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance._trackpadOrThumbstick == TrackpadOrThumbstick.TrackPad)
               _axisMode = InputMode.Click;
            else
               _axisMode = InputMode.Touch;
        }
    }

    // Update is called once per frame
    void Update()
    {
        ResetFrameBooleans();  

        if (ExampleUtil.isPresent())
        {
            bool axisUpdated = false; 
            if (ReadValueAsButton(_leftGrabPinchAction))
            {
                if (!IsLeftTriggerClicked)
                    _leftTriggerClickedFrame = true;
                else
                    _leftTriggerClickedFrame = false;

                IsLeftTriggerClicked = true;
            }
            else
            {
                if (IsLeftTriggerClicked)
                    _leftTriggerUnclickedFrame = true;
                else
                    _leftTriggerUnclickedFrame = false;

                IsLeftTriggerClicked = false;
            }

            var axis = PadEventArgs(_leftTouchPosAction.action.ReadValue<Vector2>());

            if (ReadValueAsButton(_leftTeleportAction))
            {
                LeftPadAxis = axis;

                if (!IsLeftPadPressed)
                    _leftPadPressedFrame = true;
                else
                    _leftPadPressedFrame = false;

                IsLeftPadPressed = true;
                axisUpdated = true;
            }
            else
            {
                LeftPadAxis = default;

                if (IsLeftPadPressed)
                    _leftPadUnpressedFrame = true;
                else
                    _leftPadUnpressedFrame = false;

                IsLeftPadPressed = false;
                axisUpdated = true;
            }

            if (ReadValueAsButton(_leftTouchPadAction) || (axis.padX>0 || axis.padY>0))
            {
                LeftPadAxis = axis;

                if (!IsLeftPadTouched)
                    _leftPadTouchedFrame = true;
                else
                    _leftPadTouchedFrame = false;

                IsLeftPadTouched = true;
                axisUpdated = true;
            }
            else
            {
                LeftPadAxis = default;

                if (IsLeftPadTouched)
                    _leftPadUntouchedFrame = true;
                else
                    _leftPadUntouchedFrame = false;

                IsLeftPadTouched = false;
                axisUpdated = true;
            }

            if (!axisUpdated)
            {
                if (IsLeftPadTouched || IsLeftPadPressed)
                    LeftPadAxis = axis;
                else
                    LeftPadAxis = PadEventArgs(0, 0);
            }
            else
                axisUpdated = false;

            if (ReadValueAsButton(_leftGrabGripAction))
            {
                if (!IsLeftGripped)
                    _leftGrippedFrame = true;
                else
                    _leftGrippedFrame = false;

                IsLeftGripped = true;
            }
            else
            {
                if (IsLeftGripped)
                    _leftUngrippedFrame = true;
                else
                    _leftUngrippedFrame = false;

                IsLeftGripped = false;
            }

            if (ReadValueAsButton(_rightGrabPinchAction))
            {
                if (!IsRightTriggerClicked)
                    _rightTriggerClickedFrame = true;
                else
                    _rightTriggerClickedFrame = false;

                IsRightTriggerClicked = true;
            }
            else
            {
                if (IsRightTriggerClicked)
                    _rightTriggerUnclickedFrame = true;
                else
                    _rightTriggerUnclickedFrame = false;

                IsRightTriggerClicked = false;
            }

            axis = PadEventArgs(_rightTouchPosAction.action.ReadValue<Vector2>());

            if (ReadValueAsButton(_rightTeleportAction))
            {
                RightPadAxis = axis;

                if (!IsRightPadPressed)
                    _rightPadPressedFrame = true;
                else
                    _rightPadPressedFrame = false;

                IsRightPadPressed = true;
                axisUpdated = true;
            }
            else
            {
                RightPadAxis = default;

                if (IsRightPadPressed)
                    _rightPadUnpressedFrame = true;
                else
                    _rightPadUnpressedFrame = false;

                IsRightPadPressed = false;
                axisUpdated = true;
            }


            if (ReadValueAsButton(_rightTouchPadAction) || (axis.padX>0 || axis.padY>0))
            {
                RightPadAxis = axis;

                if (!IsRightPadTouched)
                    _rightPadTouchedFrame = true;
                else
                    _rightPadTouchedFrame = false;

                IsRightPadTouched = true;
                axisUpdated = true;
            }
            else
            {
                RightPadAxis = default;

                if (IsRightPadTouched)
                    _rightPadUntouchedFrame = true;
                else
                    _rightPadUntouchedFrame = false;

                IsRightPadTouched = false;
                axisUpdated = true;
            }

            if (!axisUpdated)
            {
                if (IsRightPadTouched || IsRightPadPressed)
                    RightPadAxis = axis;
                else
                    RightPadAxis = PadEventArgs(0, 0);
            }

            if (ReadValueAsButton(_rightGrabGripAction))
            {
                if (!IsRightGripped)
                    _rightGrippedFrame = true;
                else
                    _rightGrippedFrame = false;

                IsRightGripped = true;
            }
            else
            {
                if (IsRightGripped)
                    _rightUngrippedFrame = true;
                else
                    _rightUngrippedFrame = false;

                IsRightGripped = false;
            }

            float x = 0, y = 0;
            if ((_axisMode == InputMode.Touch && IsRightPadTouched) || (_axisMode == InputMode.Click && IsRightPadPressed))
            {
                var a = RightPadAxis;
                if (a.padX > 0)
                    x = 1;
                else if (a.padX < 0)
                    x = -1;
                if (a.padY > 0)
                    y = 1;
                else if (a.padY < 0)
                    y = -1;
            }
            _rightAxisTarget = new Vector2(x, y);

            x = 0;
            y = 0;
            if ((_axisMode == InputMode.Touch && IsLeftPadTouched) || (_axisMode == InputMode.Click && IsLeftPadPressed))
            {
                var a = LeftPadAxis;
                if (a.padX > 0)
                    x = 1;
                else if (a.padX < 0)
                    x = -1;
                if (a.padY > 0)
                    y = 1;
                else if (a.padY < 0)
                    y = -1;
            }
            _leftAxisTarget = new Vector2(x, y);
        }
    }

    public bool ReadValueAsButton(InputActionProperty a)
    {
        bool value = false;

        try
        {
            value = a.action.ReadValue<bool>();
        }
        catch (InvalidOperationException)
        {
            try
            {
                value = Convert.ToBoolean(a.action.ReadValue<float>());
            }
            catch (InvalidOperationException)
            {
                try
                {
                    var v = a.action.ReadValue<Vector2>();
                    if (v.x > 0 && v.y > 0)
                        value = true;
                    else
                        value = false;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
        
        return value;
    }


    private void ResetFrameBooleans()
    {
        _leftTriggerClickedFrame = false;
        _rightTriggerClickedFrame = false;
        _leftPadPressedFrame = false;
        _rightPadPressedFrame = false;
        _leftGrippedFrame = false;
        _rightGrippedFrame = false;
        _leftPadTouchedFrame = false;
        _rightPadTouchedFrame = false;
        _leftTriggerUnclickedFrame = false;
        _rightTriggerUnclickedFrame = false;
        _leftPadUnpressedFrame = false;
        _rightPadUnpressedFrame = false;
        _leftUngrippedFrame = false;
        _rightUngrippedFrame = false;
        _leftPadUntouchedFrame = false;
        _rightPadUntouchedFrame = false;
    }

    void FixedUpdate()
    {
        float x = _rightAxis.x;
        float y = _rightAxis.y;
        if (_rightAxis.x > _rightAxisTarget.x)
        {
            x = _rightAxis.x - 0.05f;
            if (x < _rightAxisTarget.x)
                x = _rightAxisTarget.x;
        }
        else if (_rightAxis.x < _rightAxisTarget.x)
        {
            x = _rightAxis.x + 0.05f;
            if (x > _rightAxisTarget.x)
                x = _rightAxisTarget.x;
        }
        if (_rightAxis.y > _rightAxisTarget.y)
        {
            y = _rightAxis.y - 0.05f;
            if (y < _rightAxisTarget.y)
                y = _rightAxisTarget.y;
        }
        else if (_rightAxis.y < _rightAxisTarget.y)
        {
            y = _rightAxis.y + 0.05f;
            if (y > _rightAxisTarget.y)
                y = _rightAxisTarget.y;
        }
        _rightAxis = new Vector2(x, y);

        x = _leftAxis.x;
        y = _leftAxis.y;
        if (_leftAxis.x > _leftAxisTarget.x)
        {
            x = _leftAxis.x - 0.05f;
            if (x < _leftAxisTarget.x)
                x = _leftAxisTarget.x;
        }
        else if (_leftAxis.x < _leftAxisTarget.x)
        {
            x = _leftAxis.x + 0.05f;
            if (x > _leftAxisTarget.x)
                x = _leftAxisTarget.x;
        }
        if (_leftAxis.y > _leftAxisTarget.y)
        {
            y = _leftAxis.y - 0.05f;
            if (y < _leftAxisTarget.y)
                y = _leftAxisTarget.y;
        }
        else if (_leftAxis.y < _leftAxisTarget.y)
        {
            y = _leftAxis.y + 0.05f;
            if (y > _leftAxisTarget.y)
                y = _leftAxisTarget.y;
        }
        _leftAxis = new Vector2(x, y);
    }

    public static PadEventArgs PadEventArgs()
    {
        PadEventArgs p = new PadEventArgs();
        p.padX = 0;
        p.padY = 0;
        return p;
    }
    public static PadEventArgs PadEventArgs(float X, float Y)
    {
        PadEventArgs p = new PadEventArgs();
        p.padX = X;
        p.padY = Y;
        return p;
    }
    public static PadEventArgs PadEventArgs(Vector2 A)
    {
        PadEventArgs p = new PadEventArgs();
        p.padX = A.x;
        p.padY = A.y;
        return p;
    }

    public static Vector2 PadEventArgs(PadEventArgs p)
    {
        return new Vector2(p.padX, p.padY);
    }
}


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollerCoasterControls : MonoBehaviour
{
    [SerializeField]
    PathCreation.Examples.PathFollower pathFollower;

    [SerializeField]
    RollerCoasterLevelManager RC_Man;
    InputManagement _input;
    bool speedChangedLastFrame = false;
    float smoothStartRaise;
    [SerializeField]
    float speedChangeSmoothTime = 0.5f; 
    float previewAxisValueRaise = 0; //value at the previous frame
    [SerializeField]
    ButtonSequence sequenceSigns; //g.o. of the button sequence

    private void Awake()
    {
        GameObject _player = null;
        if (GameManager.Instance == null)
            _player = GameObject.FindGameObjectWithTag("Player");
        else
            _player = GameManager.Instance.LevelManager.Player.gameObject;

        _input = _player.GetComponent<InputManagement>();
        _input.OnLeftTriggerClicked += LeftTriggerClicked;
        _input.OnRightTriggerClicked += RightTriggerClicked;
    }

    private void Update()
    {
        if (RC_Man._started && !RC_Man._ended)
            manageSpeed();
    }
    private void manageSpeed()
    {

        if (GameManager.Instance != null && GameManager.Instance._isLeftHanded)
        {
            //if left-handed, use left touchpad to manage the speed

             
            bool changedThisFrame = false;
            if ((_input._axisMode == InputMode.Touch && _input.IsLeftPadTouched) || (_input._axisMode == InputMode.Click && _input.IsLeftPadPressed))
            {
                #region raise/lower w/ rightpad
                if (_input.LeftPadAxis.padY > 0.2f) //raise
                {
                    if (pathFollower.Speed < RC_Man.maxSpeed)
                    {
                        if (previewAxisValueRaise < 0.2f || !speedChangedLastFrame) //first pressure of raise
                        {
                            smoothStartRaise = Time.time;

                            previewAxisValueRaise = _input.LeftPadAxis.padY;
                        }

                        float smoothMultiplier = Mathf.InverseLerp(smoothStartRaise, smoothStartRaise + speedChangeSmoothTime, Time.time);
                        smoothMultiplier = Mathf.Clamp(smoothMultiplier, 0f, 1f);

                        pathFollower.Speed = pathFollower.Speed + getRaiseSpeedLerp(_input.LeftPadAxis.padY) * smoothMultiplier * Time.deltaTime;

                        changedThisFrame = true;
                    }
                }
                else if (_input.LeftPadAxis.padY < -0.2f)//lower
                {
                    if (pathFollower.Speed > RC_Man.minSpeed)
                    {
                        if (previewAxisValueRaise > 0.2f || !speedChangedLastFrame) //first pressure of lower
                        {
                            smoothStartRaise = Time.time;

                            previewAxisValueRaise = _input.LeftPadAxis.padY;
                        }

                        float smoothMultiplier = Mathf.InverseLerp(smoothStartRaise, smoothStartRaise + speedChangeSmoothTime, Time.time);
                        smoothMultiplier = Mathf.Clamp(smoothMultiplier, 0f, 1f);

                        pathFollower.Speed = pathFollower.Speed - getRaiseSpeedLerp(_input.LeftPadAxis.padY) * smoothMultiplier * Time.deltaTime;

                        changedThisFrame = true;
                    }
                }
                #endregion
            }
            speedChangedLastFrame = changedThisFrame;
        }
        else
        {
            bool changedThisFrame = false;
            if ((_input._axisMode == InputMode.Touch && _input.IsRightPadTouched) || (_input._axisMode == InputMode.Click && _input.IsRightPadPressed))
            {
                #region raise/lower w/ Rightpad
                if (_input.RightPadAxis.padY > 0.2f) //raise
                {
                    if(pathFollower.Speed < RC_Man.maxSpeed)
                    {
                        if (previewAxisValueRaise < 0.2f || !speedChangedLastFrame) //first pressure of raise
                        {
                            smoothStartRaise = Time.time;

                            previewAxisValueRaise = _input.RightPadAxis.padY;
                        }

                        float smoothMultiplier = Mathf.InverseLerp(smoothStartRaise, smoothStartRaise + speedChangeSmoothTime, Time.time);
                        smoothMultiplier = Mathf.Clamp(smoothMultiplier, 0f, 1f);

                        pathFollower.Speed = pathFollower.Speed + getRaiseSpeedLerp(_input.RightPadAxis.padY) * smoothMultiplier * Time.deltaTime;

                        changedThisFrame = true;
                    }

                }
                else if (_input.RightPadAxis.padY < -0.2f)//lower
                {
                    if (pathFollower.Speed > RC_Man.minSpeed)
                    {
                        if (previewAxisValueRaise > 0.2f || !speedChangedLastFrame) //first pressure of lower
                        {
                            smoothStartRaise = Time.time;

                            previewAxisValueRaise = _input.RightPadAxis.padY;
                        }

                        float smoothMultiplier = Mathf.InverseLerp(smoothStartRaise, smoothStartRaise + speedChangeSmoothTime, Time.time);
                        smoothMultiplier = Mathf.Clamp(smoothMultiplier, 0f, 1f);


                        pathFollower.Speed = pathFollower.Speed - getRaiseSpeedLerp(_input.RightPadAxis.padY) * smoothMultiplier * Time.deltaTime;

                        changedThisFrame = true;
                    }
                }
            }
            speedChangedLastFrame = changedThisFrame;
            #endregion
        }
    }


float getRaiseSpeedLerp(float axisValue)
{
    float val = Mathf.Abs(axisValue);
    float speed;

    float perc = Mathf.InverseLerp(0.2f, 1f, val);
    speed = Mathf.Lerp(RC_Man.minSpeed, RC_Man.maxSpeed, perc);


    return speed;

}

private void LeftTriggerClicked(object obj)
    {
        //checks correct pressure in sequence
        if (RC_Man._started && !RC_Man._ended)
            sequenceSigns.sequenceButtonPressed("L"); //left trigger presssed
    }
 
    private void RightTriggerClicked(object obj)
    {
        //checks correct pressure in sequence
        if (RC_Man._started && !RC_Man._ended)
            sequenceSigns.sequenceButtonPressed("R"); //right trigger pressed
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    InputField _inputField;
    [SerializeField]
    Dropdown _techniqueDropdown;
    [SerializeField]
    Dropdown _trackpadThumbstickDropdown;
    [SerializeField]
    Toggle _handCheckBox;

    bool _loading = false;

    // Start is called before the first frame update
    void Start()
    {
        _inputField.text = GameManager.Instance._userID.ToString();
        _techniqueDropdown.value = (int) GameManager.Instance._technique;
        _trackpadThumbstickDropdown.value = (int) GameManager.Instance._trackpadOrThumbstick;
        _handCheckBox.isOn = GameManager.Instance._isLeftHanded;
    }

    public void LoadScene(string name)
    {
        GameManager.Instance.LoadScene(name);
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current[Key.S].wasPressedThisFrame)
            GameManager.Instance.LoadScene("NavigationalSearch");
        else if (Keyboard.current[Key.R].wasPressedThisFrame)
            GameManager.Instance.LoadScene("TrackRace");
        else if (Keyboard.current[Key.D].wasPressedThisFrame)
            GameManager.Instance.LoadScene("TowerDefense");
        else if (Keyboard.current[Key.C].wasPressedThisFrame)
            GameManager.Instance.LoadScene("RollerCoaster");
        else if (Keyboard.current[Key.Q].wasPressedThisFrame)
            Application.Quit();
        else if (Keyboard.current[Key.H].wasPressedThisFrame)
            _handCheckBox.isOn = !GameManager.Instance._isLeftHanded;
        else if (Keyboard.current[Key.DownArrow].wasPressedThisFrame)
        {
            var i = (int)GameManager.Instance._technique + 1;
            if (i >= Enum.GetNames(typeof(MitigationTechnique)).Length)
                i = 0;
            _techniqueDropdown.value = i;
        }
        else if (Keyboard.current[Key.UpArrow].wasPressedThisFrame)
        {
            var i = (int)GameManager.Instance._technique - 1;
            if (i < 0)
                i = Enum.GetNames(typeof(MitigationTechnique)).Length-1;
            _techniqueDropdown.value = i;
        }
        else if (Keyboard.current[Key.LeftArrow].wasPressedThisFrame)
        {
            var i = (int)GameManager.Instance._trackpadOrThumbstick + 1;
            if (i >= Enum.GetNames(typeof(TrackpadOrThumbstick)).Length)
                i = 0;
            _trackpadThumbstickDropdown.value = i;
        }
        else if (Keyboard.current[Key.RightArrow].wasPressedThisFrame)
        {
            var i = (int)GameManager.Instance._trackpadOrThumbstick - 1;
            if (i < 0)
                i = Enum.GetNames(typeof(TrackpadOrThumbstick)).Length - 1;
            _trackpadThumbstickDropdown.value = i;
        }
        else if (Keyboard.current[Key.NumpadPlus].wasPressedThisFrame)
        {
            _inputField.text = (GameManager.Instance._userID + 1).ToString();
        }
        else if (Keyboard.current[Key.NumpadMinus].wasPressedThisFrame)
        {
            if (GameManager.Instance._userID > -1)
                _inputField.text = (GameManager.Instance._userID - 1).ToString();
        }
    }


    public void OnUserIDChanged(string value)
    {
        if (Int32.TryParse(value, out GameManager.Instance._userID))
            PlayerPrefs.SetInt("UserID", GameManager.Instance._userID);
    }

    public void OnTechniqueChanged(int value)
    {
        GameManager.Instance._technique = (MitigationTechnique)value;
        PlayerPrefs.SetInt("MitigationTechnique", value);
    }

    public void OnTrackpadOrThumbstickChanged(int value)
    {
        GameManager.Instance._trackpadOrThumbstick = (TrackpadOrThumbstick)value;
        PlayerPrefs.SetInt("TrackpadOrThumbstick", value);
    }

    public void OnHandChanged(bool value)
    {
        GameManager.Instance._isLeftHanded = value;
        PlayerPrefs.SetInt("IsLeftHanded", Convert.ToInt32(value));
    }

    public void Quit()
    {
        Application.Quit();
    }
}

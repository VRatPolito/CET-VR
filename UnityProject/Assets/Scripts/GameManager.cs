using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum MitigationTechnique { None, AuthenticNose, CircleEffet, DynamicColorBlur, GazeContingentDOF, DotEffect, DynamicFOV, HeadSnapper, VirtualNose, VirtualCave, VisionLock, VRTunnellingPro };

public enum TrackpadOrThumbstick { TrackPad, ThumbStick };

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField]
    string _logFileName = "Log";
    [SerializeField]
    internal char _csvSeparator = ';';

    [SerializeField]
    internal PlayerInputActions _playerInputActions;
    internal int _userID = -1;
    LevelManager _levelManager;
    [SerializeField]
    internal string _mainMenu = "StartScene";
    internal bool _isLeftHanded = false;
    internal MitigationTechnique _technique;
    internal TrackpadOrThumbstick _trackpadOrThumbstick;
    internal DateTime _startTime;
    bool _loading = false;

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
            _playerInputActions = new PlayerInputActions();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        if(PlayerPrefs.HasKey("UserID"))
        {
            _userID = PlayerPrefs.GetInt("UserID");
        }    
        if(PlayerPrefs.HasKey("MitigationTechnique"))
        {
            _technique = (MitigationTechnique)PlayerPrefs.GetInt("MitigationTechnique");
        }
        if (PlayerPrefs.HasKey("TrackpadOrThumbstick"))
        {
            _trackpadOrThumbstick = (TrackpadOrThumbstick)PlayerPrefs.GetInt("TrackpadOrThumbstick");
        }
        if (PlayerPrefs.HasKey("IsLeftHanded"))
        {
            _isLeftHanded = Convert.ToBoolean(PlayerPrefs.GetInt("IsLeftHanded"));
        }
        LoadScene(_mainMenu);
    }

    public LevelManager LevelManager {
        get {
            if (_levelManager == null)
                _levelManager = FindObjectOfType<LevelManager>();
            return _levelManager;
            }
        }

    public void LoadScene(string name)
    {
        if (!_loading)
            SceneManager.LoadScene(name);
    }

    public void LoadSceneAsync(string name)
    {
        if (!_loading)
            StartCoroutine(LoadAsyncScene(name));
    }

    IEnumerator LoadAsyncScene(string name)
    {
        _loading = true;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(name);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        _loading = false;
    }

    internal string GetTime()
    {
        if(LevelManager == null)
            return "00:00";
        var t = LevelManager.GetTime();
        var ts = TimeSpan.FromSeconds(t);
        return ts.Minutes.ToString("D2")+":"+ ts.Seconds.ToString("D2");
    }

    internal void ResultToFile(string result)
    {
        string timeStamp = (_startTime.ToShortDateString() + "_" + _startTime.ToLongTimeString()).Replace('/', '-').Replace(':', '-');
        string fileName = _logFileName + "_" + _userID.ToString() + "_" + LevelManager.LevelName + "_" + timeStamp + ".csv";

        if (File.Exists(fileName))
        {
            var sr = File.AppendText(fileName);
            sr.WriteLine(result);
            sr.Close();
        }
        else
        {
            var sr = File.CreateText(fileName);
            var header = LevelManager.CSVHeaders;
            if (header.Count > 0)
            {
                var head = "";
                for (int i = 0; i < header.Count; i++)
                {
                    head += header[i];
                    if (i != header.Count - 1)
                        head += _csvSeparator;
                }
                sr.WriteLine(head);
            }
            sr.WriteLine(result);
            sr.Close();
        }
        Debug.Log(result);
    }
}

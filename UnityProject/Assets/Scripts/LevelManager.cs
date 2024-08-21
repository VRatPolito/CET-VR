using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
public class VRTunnellingProSetting
{
    public bool useAngVel = true;
    public bool useLinAccel = false;
    public bool useLinVel = false;
    public float minAngVel = 0;
    public float maxAngVel = 180;
    public float minLinAccel = 0;
    public float maxLinAccel = 0;
    public float minLinVel = 0;
    public float maxLinVel = 0;
}

public class LevelManager : MonoBehaviour
{
    [SerializeField]
    internal GameObject _player;
    [SerializeField]
    internal GameObject _playerHead;
    [SerializeField]
    internal List<string> _csvHeaders = new List<string>(new string[] { "UserID", "Scene" ,"Date", "Time",  "ComplTime", "TravelDist" });
    [SerializeField]
    internal string _levelName;
    [SerializeField]
    internal Canvas _sceneUI;
    [SerializeField]
    internal GameObject virtualNose, authenticNose, dotEffect, virtualCave, headSnapper, visionLock;
    internal bool _stopForSickness = false;
    internal bool _canStart = false;
    internal bool _started = false;
    internal bool _ended = false;
    internal bool _go = false;
    internal float _startTime;
    internal float _complTime;
    internal float _travelDistance;
    internal Vector3 _lastFramePos;
    internal InputManagement _input;
    public Transform Player { get { return _player.transform; } }
    public Transform PlayerHead { get { return _playerHead.transform; } }
    public string LevelName { get { return _levelName; } }
    public float StartTime { get { return _startTime; } }
    public List<string> CSVHeaders { get { return _csvHeaders; } }

    public UnityEvent<int, int> OnDSValueChanged = new UnityEvent<int, int>();

    [SerializeField]
    List<GameObject> _tutorial;
    [SerializeField]
    List<GameObject> _altTutorial;
    [SerializeField]
    VRTunnellingProSetting _tunnellingSettings;
    /*[SerializeField]
    float _debugResultTime = 1;
    float _nextDebugResult;*/
    internal float _nextLog;
    int _discomfortScale = 0;
    [SerializeField]
    InputField _dsValue;
    Transform _dsArrow;
    [SerializeField]
    float _flashArrowTime = 5;
    [SerializeField]
    float _flashArrowFreq = 1f;
    float _nextFlash;
    bool _flashArrow = false;
    float _flashTime = 0;
    float _nextDSRequest = float.MaxValue;
    Color _dsNormalColor;
    bool _loading = false;

    internal virtual void Awake()
    {
        if(_dsValue != null)
        {
            _dsArrow = _dsValue.transform.Find("Arrow");
            _dsNormalColor = _dsValue.colors.normalColor;
            _dsValue.text = "0";
        }
    }

    internal float GetTime()
    {
        if (_started)
            return Time.time - _startTime;
        else if (_ended)
            return _complTime - _startTime;
        else
            return -1;
    }

    internal virtual void Start()
    {
        _input = Player.gameObject.GetComponent<InputManagement>();
        if(GameManager.Instance != null && GameManager.Instance._isLeftHanded)
        {
            foreach(var t in _tutorial)
                t.gameObject.SetActive(false);
            foreach (var t in _altTutorial)
                t.gameObject.SetActive(true);
        }
        OnDSValueChanged.AddListener(ManageDS);
    }

    private void ManageDS(int value, int prevValue)
    {
        if(_started && !_ended)
        { 
            if(value == 10)
            {
                var cols = _dsValue.colors;
                cols.normalColor = Color.red;
                _dsValue.colors = cols;
                Invoke(nameof(LeaveForSickness), 3);
            }
            else
            {
                if (prevValue == 10)
                {
                    var cols = _dsValue.colors;
                    cols.normalColor = _dsNormalColor;
                    _dsValue.colors = cols;
                    CancelInvoke(nameof(LeaveForSickness));
                }
            }
        }
    }

    private void LeaveForSickness()
    {
        _stopForSickness = true;
        EndGame();
    }

    // Update is called once per frame
    internal virtual void Update()
    {

        if (Keyboard.current[Key.LeftCtrl].isPressed && Keyboard.current[Key.H].wasPressedThisFrame)
            _sceneUI.enabled = !_sceneUI.enabled;

        if (_started && !_ended)
        {
            if (Keyboard.current[Key.NumpadPlus].wasPressedThisFrame)
            {
                if (_discomfortScale < 10)
                {
                    _dsValue.text = (_discomfortScale + 1).ToString();
                }
            }
            else if (Keyboard.current[Key.NumpadMinus].wasPressedThisFrame)
            {
                if (_discomfortScale > 0)
                {
                    _dsValue.text = (_discomfortScale - 1).ToString();
                }
            }
            UpdateTravelDistance();

            /*if (Application.isEditor && Time.time >= _nextDebugResult)
            {
                char sep = ';';
                if (GameManager.Instance != null)
                    sep = GameManager.Instance._csvSeparator;
                Debug.Log(GetResultString(GetResults(Time.time - _startTime), sep));
                _nextDebugResult = Time.time + _debugResultTime;
            }*/

            if(Time.time >= _nextLog)
            {
                char sep = ';';
                if (GameManager.Instance != null)
                {
                    sep = GameManager.Instance._csvSeparator;
                    GameManager.Instance.ResultToFile((GetResultString(GetResults(Time.time - _startTime), sep)));
                    if(Application.isEditor)
                        Debug.Log(GetResultString(GetResults(Time.time - _startTime), sep));
                }
                _nextLog = Time.time + 60;
            }

            if (Keyboard.current[Key.LeftCtrl].isPressed && Keyboard.current[Key.L].wasPressedThisFrame)
            {
                _discomfortScale = 10;
                LeaveForSickness();
            }
            if (Keyboard.current[Key.LeftCtrl].isPressed && Keyboard.current[Key.Q].wasPressedThisFrame)
                EndGame();
        }
        else
            if (Keyboard.current[Key.LeftCtrl].isPressed && Keyboard.current[Key.Q].wasPressedThisFrame)
                LoadMainMenu();

        if(Time.time >= _nextDSRequest)
        {
            _flashArrow = true;
            _dsArrow.gameObject.SetActive(true);
            _flashTime = Time.time + _flashArrowTime;
            _nextFlash = Time.time + (1/_flashArrowFreq);
            _nextDSRequest = Time.time + 60;
        }

        if (_flashArrow)
        {
            if (Time.time >= _flashTime)
            {
                _dsArrow.gameObject.SetActive(false);
                _flashArrow = false;
            }
            else
            {
                if(Time.time >= _nextFlash)
                {
                    _dsArrow.gameObject.SetActive(!_dsArrow.gameObject.activeSelf);
                    _nextFlash = Time.time + (1/_flashArrowFreq);
                }
            }
        }
        
    }

    public virtual void StartGame()
    {
        if (!_started)
        {
            SetMitigationTechnique();
            _complTime = 0;
            _startTime = Time.time;
            if (GameManager.Instance != null)
                GameManager.Instance._startTime = DateTime.Now;
            _started = true;
            _lastFramePos = Player.position;
            _nextDSRequest = Time.time + 60;
            /*if (Application.isEditor)
                _nextDebugResult = Time.time + _debugResultTime;*/
            _nextLog = Time.time + 30;
        }
    }
    internal virtual float GetErrors(float time)
    {
        throw new NotImplementedException();
    }
    internal virtual float GetAccuracy()
    {
        throw new NotImplementedException();
    }
    internal virtual float GetOperationSpeed(float time)
    {
        throw new NotImplementedException();
    }
    internal virtual void UpdateTravelDistance()
    {
            _travelDistance += Vector3.Distance(
                Player.position,
                _lastFramePos
                );

            _lastFramePos = Player.position;
    }
    public virtual void EndGame(float loadAfter = 0)
    {
        if (_started && !_ended)
        {
            if(!_stopForSickness)
                _ended = true;
            char sep = ';';
            if (GameManager.Instance != null)
                sep = GameManager.Instance._csvSeparator;
            _complTime = Time.time - _startTime;
            GameManager.Instance.ResultToFile(GetResultString(GetResults(_complTime), sep));
            Invoke(nameof(LoadMainMenu), loadAfter);
        }
    }

    private void LoadMainMenu()
    {
        GameManager.Instance.LoadScene(GameManager.Instance._mainMenu);
    }


    internal virtual List<string> GetResults(float time)
    {
        var res = new List<string>();
        res.Add(_discomfortScale.ToString());
        if (_stopForSickness)
            res.Add("Y");
        else
            res.Add("N");
        if(_ended)
        {
            res.Add("Y");
            res.Add(FloatTimeToString(_complTime));
        }
        else
        {
            res.Add("N");
            res.Add(FloatTimeToString(time));
        }
        res.Add(_travelDistance.ToString("0.00"));
        res.Add(GetOperationSpeed(time).ToString("0.00"));
        res.Add(GetAccuracy().ToString("0.00"));
        res.Add(GetErrors(time).ToString("0.00")); 

        return res;
    }

    internal string GetResultString(List<string> results, char separator)
    {
        string timeStamp = (DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToLongTimeString()).Replace('/', '-').Replace(':', '-');

        string result = timeStamp + separator;

        for (int i = 0; i < results.Count; i++)
        {
            result += results[i];
            if (i != results.Count - 1)
                result += separator;
        }
        return result;
    }

    public static string FloatTimeToString(float time)
    {
        try
        {
            TimeSpan t = TimeSpan.FromSeconds(time);
            return ((int)t.TotalMinutes).ToString("D2") + ":" + (t.Seconds).ToString("D2");
        }
        catch (OverflowException)
        { }
        return "OF";
    }

    internal virtual void Reset()
    {
        _startTime = 0;
        _complTime = 0;
        _travelDistance = 0;
        _started = false;
        _ended = false;
        _go = false;
        _discomfortScale = 0;
        _dsValue.text = "0";
    }

    Dictionary<MitigationTechnique, Type> techniqueMap = new Dictionary<MitigationTechnique, Type>
    {
        {MitigationTechnique.AuthenticNose, typeof(AuthenticNose)},
        {MitigationTechnique.CircleEffet, typeof(CircleEffect)},
        {MitigationTechnique.GazeContingentDOF, typeof(DepthOfField)},
        {MitigationTechnique.DynamicColorBlur, typeof(DynamicColorBlur)},
        {MitigationTechnique.DotEffect, typeof(DotEffect)},
        {MitigationTechnique.DynamicFOV, typeof(DynamicFOV)},
        {MitigationTechnique.HeadSnapper,typeof(VisionSnapper)},
        {MitigationTechnique.VirtualNose, typeof(SingleNose)},
        {MitigationTechnique.VirtualCave, typeof(Virtual_Cave)},
        {MitigationTechnique.VisionLock, typeof(VisionLock)},
        {MitigationTechnique.VRTunnellingPro, typeof(Sigtrap.VrTunnellingPro.Tunnelling)} //VR tunneling
    };

    public void DSValueChanged(string value)
    {
        int val = -1;
        if(Int32.TryParse(value, out val))
        {
            if (val >= 0 && val <= 10)
            {
                var prev = _discomfortScale;
                _discomfortScale = val;
                OnDSValueChanged?.Invoke(_discomfortScale, prev);
            }
        }
    }

    internal void SetMitigationTechnique()
    {
        if (GameManager.Instance!=null)
            switch (GameManager.Instance._technique)
            {
                case MitigationTechnique.VirtualNose:
                    if (virtualNose != null)
                        virtualNose.gameObject.SetActive(true);
                    break;
                case MitigationTechnique.AuthenticNose:
                    _playerHead.GetComponent<Camera>().enabled = false;
                    if (authenticNose != null)
                        authenticNose.gameObject.SetActive(true);
                    break;
                case MitigationTechnique.DotEffect:
                    if (dotEffect != null)
                        dotEffect.gameObject.SetActive(true);
                    break;
                case MitigationTechnique.VirtualCave:
                    if(virtualCave != null)
                    {
                        virtualCave.transform.position = Player.transform.position;
                        virtualCave.transform.rotation = Player.transform.rotation;
                        virtualCave.gameObject.SetActive(true);
                    }
                    break;
                case MitigationTechnique.VisionLock:
                    if(visionLock != null)
                        visionLock.gameObject.SetActive(true);
                    break;
                case MitigationTechnique.HeadSnapper:
                    if (headSnapper != null)
                        headSnapper.gameObject.SetActive(true);
                    break;
                case MitigationTechnique.None:
                    break;
                case MitigationTechnique.VRTunnellingPro:
                    var vrtp = (Sigtrap.VrTunnellingPro.Tunnelling)PlayerHead.GetComponent(techniqueMap[MitigationTechnique.VRTunnellingPro]);
                    if(vrtp != null)
                    {
                        vrtp.useAcceleration = _tunnellingSettings.useLinAccel;
                        vrtp.useAngularVelocity = _tunnellingSettings.useAngVel;
                        vrtp.useAcceleration = _tunnellingSettings.useLinAccel;
                        vrtp.accelerationMax = _tunnellingSettings.maxLinAccel;
                        vrtp.accelerationMin = _tunnellingSettings.minLinAccel;
                        vrtp.angularVelocityMax = _tunnellingSettings.maxAngVel;
                        vrtp.angularVelocityMin = _tunnellingSettings.minAngVel;
                        vrtp.velocityMax = _tunnellingSettings.maxLinVel;
                        vrtp.velocityMin = _tunnellingSettings.minLinVel;
                        vrtp.enabled = true;
                    }                    
                    break;
                default:
                    Type type = techniqueMap[GameManager.Instance._technique];
                    (PlayerHead.GetComponent(type) as MonoBehaviour).enabled = true;
                    break;
            }
    }

}

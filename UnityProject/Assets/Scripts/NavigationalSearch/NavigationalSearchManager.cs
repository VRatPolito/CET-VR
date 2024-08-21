using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NavigationalSearchManager : LevelManager
{
    [SerializeField]
    AudioSource _coinSound;
    [SerializeField]
    AudioSource _timerSound;
    Vector3 startPos;
    Quaternion startRot;
    [SerializeField]
    List<Coin> _coins;
    [SerializeField]
    Text _coinCount;
    [SerializeField]
    internal Text _time;
    [SerializeField]
    internal Text _lap;
    [SerializeField]
    internal Text _totalLaps;
    [SerializeField]
    Text _tutorialText;
    /*
     * Operation speed: completion time in relation to a maximum (minimum speed)
     * and a minimum time (minimum distance at maximum speed allowed).
     * Times greater than the maximum are truncated to the maximum time
     */
    [SerializeField]
    float _minSpeedKMH = 2.0f; //reference speed for maximum completion time
    [SerializeField]
    float _currOptimalPath; //minimum path until the last coin collected (in case of interruptions)
    [SerializeField]
    bool allCoinsCollected = false;
    [SerializeField]
    GameObject hintArrow; //gameobject which will indicate the direction towards the next coin (if too much time has passed since the previous coin)
    [SerializeField]
    float arrowTime = 60f; //if _arrowTime_ seconds have passed since the last coin collected, activate the arrow to give a hint to the player
    Transform _pointedCoinPosition; //position of the pointed coin
    float _lastCoinCaptureTime = 0f; //instant of collection of the previous coin
    [SerializeField]
    GameObject _tutorialCanvas;
    [SerializeField]
    AudioClip _goSound;
    [SerializeField]
    AudioClip _countSound;
    [SerializeField]
    float tickingRiseTime = 120f; //during these seconds, the ticking volume increases from 0 to 1 gradually, and then stops at 1

    [SerializeField, Tooltip("Number of laps")]
    int _laps = 3; //number of laps of the circuit
    private int _collectedCoins = 0;
    private bool _firstCoin = true;
    internal int CollectedCoins { get { return _collectedCoins; } set { _collectedCoins = value; } }
    float _minSpeedMS;
    float _maxSpeedMS;
    //reference completion times
    float _minSpeedComplTime;
    float _maxSpeedComplTime;  
    AudioSource _tutorialSound;
    Vector3 _playerStartPos;    
    int currentLap = 0; //current lap
    
    // Start is called before the first frame update
    internal override void Start()
    {
        base.Start();
        _playerStartPos = Player.transform.position;
        _maxSpeedMS = Player.gameObject.GetComponent<JoystickMovement>().GetRunSpeed() / 3.6f; //km/h
        _minSpeedMS = _minSpeedKMH / 3.6f;
        _travelDistance = 0f;
        _currOptimalPath = 0f;
        _tutorialText.text += " " + _laps.ToString("0") +" laps in the scenario.";

        currentLap = 0;

        #region tutorialCanvas
        _tutorialCanvas.gameObject.SetActive(true); 
        _tutorialSound = _tutorialCanvas.gameObject.GetComponent<AudioSource>();
        _tutorialCanvas.transform.Find("PressToStartText").gameObject.SetActive(false);

        #endregion

        startPos = Player.transform.position;
        _lastFramePos = startPos;
        startRot = Player.transform.rotation;

        float length = 0.0f;
        for (int i = 0; i < _coins.Count - 1; i++)
            length += Vector3.Distance(_coins[i].transform.position, _coins[i + 1].transform.position);
        float onelap = length;
        length *= _laps;
        Debug.Log("One lap: " + onelap + " FullOptimalPath: " + length);

        #region coins
        for (int i = 0; i < _coins.Count; i++)
        {
            _coins[i].GetComponent<Coin>().OnCoinCollected.AddListener(CoinCollected);          
        }
        #endregion
         
        hintArrow.gameObject.SetActive(false);
        _pointedCoinPosition = _coins[0].transform;
    }

    //updates frame by frame the distance traveled by the player
    internal override void UpdateTravelDistance()
    {
        if (_collectedCoins >= 1 && !allCoinsCollected)//if you have not reached the end of the path
        {
            base.UpdateTravelDistance();
        }

    }
    internal override void Update()
    {
        base.Update();

        if (!_started && !_ended)
        {
            if (!_canStart && Keyboard.current[Key.LeftCtrl].isPressed && Keyboard.current[Key.I].wasPressedThisFrame)
            {
                StartCoroutine(StartCollecting());
            }
            else if (_canStart && _input.IsLeftTriggerClickedDown || _input.IsRightTriggerClickedDown || Keyboard.current[Key.Space].wasPressedThisFrame)
                 _go = true;
        }
        else if (_started && !_firstCoin && !_ended)
        {
            nextCoinHint();
            _time.text = FloatTimeToString(Time.time - _startTime);

            float perc = Mathf.InverseLerp(0, tickingRiseTime, Time.time - _startTime);
            _timerSound.volume = Mathf.Lerp(0, 1, perc);
        }
    }
    private IEnumerator StartCollecting()
    {
        //_tutorialCanvas.gameObject.SetActive(true);

        _tutorialCanvas.transform.Find("PressToStartText").gameObject.SetActive(true);
        _canStart = true;

        yield return new WaitWhile(() => _go == false);

        _tutorialSound.clip = _countSound;
        _tutorialSound.Play();
        yield return new WaitForSeconds(1);
        _tutorialSound.Play();
        yield return new WaitForSeconds(1);
        _tutorialSound.Play();
        yield return new WaitForSeconds(1);
        _tutorialSound.clip = _goSound;
        _tutorialSound.Play();
        yield return new WaitForSeconds(_goSound.length);
        StartWalking();
    }

    internal override void Reset()
    {
        Player.transform.position = startPos;
        Player.transform.rotation = startRot;
        Player.GetComponent<JoystickMovement>().enabled = false;
        base.Reset();
        _collectedCoins = 0;
        _coinCount.text = "0";
        _lap.text = "0";
        _time.text = "0";
        _currOptimalPath = 0;

        _timerSound.Stop();
        _timerSound.volume = 0f;

        currentLap = 0;

        for (int i = 1; i < _coins.Count; i++)
        {
            _coins[i].gameObject.SetActive(false);
        }

        _coins[0].gameObject.SetActive(true);
        #region tutorialCanvas
        _tutorialCanvas.gameObject.SetActive(true);

        _tutorialCanvas.transform.Find("PressToStartText").gameObject.SetActive(false);

        #endregion

        hintArrow.gameObject.SetActive(false);
        _pointedCoinPosition = _coins[0].transform;
        _lastCoinCaptureTime = Time.time;
        _firstCoin = true;
    }

    public void StartLap()
    {
        if (_firstCoin)
        {
            _firstCoin = false;
            StartGame();
        }
        else
        {
            _pointedCoinPosition = _coins[0].transform;
            _lastCoinCaptureTime = Time.time;
            hintArrow.gameObject.SetActive(false);
        }
    }

    void StartWalking()
    {
        _tutorialSound.gameObject.SetActive(false);
        _tutorialCanvas.gameObject.SetActive(false);
        Player.GetComponent<JoystickMovement>().enabled = true;
        _totalLaps.text = _laps.ToString();
        _lap.text = "1";
    }

    public override void StartGame()
    {       
        base.StartGame();
        _timerSound.volume = 0f;
        _timerSound.Play();
        _lastCoinCaptureTime = Time.time;
    }

    public void EndLap()
    {
        _coinSound.Play();
        LastCoinCollected();
        currentLap++;
        _lap.text = (currentLap+1).ToString();
        if (currentLap == _laps)
        {
            Player.GetComponent<JoystickMovement>().Blocked = true;
            _timerSound.Stop();
            EndGame(5);
        }
        else if (GameManager.Instance != null)
        {
            GameManager.Instance.ResultToFile(GetResultString(GetResults(Time.time - _startTime), GameManager.Instance._csvSeparator));
        }
    }

    public void CoinCollected()
    {
        int _lastCoinIndx, _prevIdx;
        _collectedCoins++;

        //last coin collected index(index in the array, therefore it starts from 0 up to count - 1)
        _lastCoinIndx = (_collectedCoins - 1) % (_coins.Count -1);

        //penultimate coin collected => last coin collected -1
        _prevIdx = _lastCoinIndx - 1;
        if (_lastCoinIndx == 0)
            _prevIdx = _coins.Count - 1;

        _coinSound.Play();
        //gradually add the minimum distance between individual waypoints

        if (_collectedCoins == 1)
        {
            _currOptimalPath += Mathf.Abs(Vector3.Distance(
                _coins[_lastCoinIndx].gameObject.transform.position,
                _playerStartPos
                ));
        }
        else
        {
            _currOptimalPath += Mathf.Abs(Vector3.Distance(
                _coins[_lastCoinIndx].gameObject.transform.position,
                _coins[_prevIdx].gameObject.transform.position
                ));
        }
        
        _coinCount.text = _collectedCoins.ToString();

        //refresh the coin pointed to by the arrow, deactivate it, and start the timer from the last coin collected
        _pointedCoinPosition = _coins[(_collectedCoins)%(_coins.Count)].transform;
        hintArrow.gameObject.SetActive(false);
        _lastCoinCaptureTime = Time.time;

        UpdateOSReferences();
    }

    //updates the direction of the hint arrow when too much time has passed since the last coin collected
    private void nextCoinHint()
    {
        if(Time.time >= _lastCoinCaptureTime + arrowTime)
        {
            hintArrow.gameObject.SetActive(true);
            var lookDir = _playerHead.transform.forward;
            lookDir.y = 0;
            var arrowPoint = new Ray(_playerHead.transform.position, lookDir);
            Vector3 pos = arrowPoint.GetPoint(3.5f);
            pos.y = Player.transform.position.y + 0.15f;
            hintArrow.transform.position = pos;
            hintArrow.transform.LookAt(_pointedCoinPosition, Vector3.up);
            Vector3 rot = hintArrow.transform.rotation.eulerAngles;
            rot.x = 90f;
            hintArrow.transform.rotation = Quaternion.Euler(rot);
        }
    }

    #region results
    //accuracy as optimal route / distance travelled
    internal override float GetAccuracy()
    {
        if (_collectedCoins < 1)
            return 1;

        return (float)(_currOptimalPath /_travelDistance);
    }

    //updates the O.S. reference values. (Operation Speed)
    void UpdateOSReferences()
    {
        _maxSpeedComplTime = _currOptimalPath / _maxSpeedMS;
        _minSpeedComplTime = _currOptimalPath / _minSpeedMS;

        Debug.Log("MaxSpeedComplTime: " + _maxSpeedComplTime + "[s] at speed:"+ _maxSpeedMS +" [m/s]\n");
        Debug.Log("MinxSpeedComplTime: " + _minSpeedComplTime + "[s] at speed:" + _minSpeedMS + " [m/s]\n");
    }

    internal override float GetOperationSpeed(float time)
    {
        float os = -1f;
        float clampedTime;

        if(_maxSpeedComplTime == 0 || _minSpeedComplTime == 0)
            return os;

        //time clamped in this interval
        clampedTime = Mathf.Clamp(time,_maxSpeedComplTime, _minSpeedComplTime);

        //op = 1 if minimum possible time, = 0 if max. Otherwise, intermediate values ​​(0.1)
        os = 1f - Mathf.InverseLerp( _maxSpeedComplTime, _minSpeedComplTime, clampedTime);
        
        return os;

    }

    internal override float GetErrors(float time)
    {
        return -1;
    }

    internal override List<string> GetResults(float time)
    {
        var res = base.GetResults(time);
        res.Add((currentLap).ToString());
        res.Add(_collectedCoins.ToString());
        return res;
    }
    #endregion
    void LastCoinCollected()
    {
        if (_collectedCoins == _laps * _coins.Count - 1) //if all the coins except the last one are collected
        {
            _collectedCoins++;            
            allCoinsCollected = true;
        }
    }
}

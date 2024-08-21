using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TrackRaceLevelManager : LevelManager
{
    [SerializeField]
    SafePointManager _safePointMan;
    [SerializeField]
    GameObject _vehicle;
    [SerializeField]
    Text _tutorialText;
    [SerializeField]
    int _laps = 5; //timed laps
    [SerializeField]
    int _testLaps = 1; //test runs
    [SerializeField]
    List<GameObject> walls; //walls to be demolished
    [SerializeField]
    Transform _startPos;
    [SerializeField]
    int _wallNum; //total number of walls encountered (number of walls on circuit * number of timed laps)
    [SerializeField]
    float _minSpeedKMH = 20f; //reference speed for maximum completion time
    [SerializeField]
    AudioSource _countDown;
    [SerializeField]
    Text _countDownText;
    [SerializeField]
    Text _lapText;
    [SerializeField]
    Text _TotalLapText;
    [SerializeField]
    AudioClip _goSound;
    [SerializeField]
    AudioClip _countSound;
    MSSceneControllerFree _vehicleManager;
    [SerializeField]
    Text _timeText;
    [SerializeField]
    Text _collisionsText;
    [SerializeField]
    Text _errorsText;
    [SerializeField]
    GameObject _tutorialCanvas;
    [SerializeField]
    HideMeshes _hideMeshes;

    private int _collisions = 0;
    private int _currLap = 0;
    public Transform Vehicle { get { return _vehicle.transform; } }
    Vector3 _vehicleStartPos;
    Quaternion _vehicleStartRot;
    Coroutine _startRoutine = null;
    bool _testRide = true;//if in the test run
    float _currOptimalPath = 0; //optimal path length
    float _minSpeedMS;
    float _maxSpeedMS;
    //completion times at max and min speed
    float _maxSpeedComplTime;
    float _minSpeedComplTime;
    bool _lapCounterArmed = true;
    VehicleOnTrack _vehicleOnTrack;
    int _errors = 0;
    /*errors: percentage of time spent off the track (off the road)*/
    float _timeOffTrack = 0f;  //time spent outside the circuit/ colliding with external elements


    // Start is called before the first frame update
    internal override void Start()
    {
        base.Start();
        _currLap = -1;
        _maxSpeedMS = Vehicle.gameObject.GetComponent<MSVehicleControllerFree>()._vehicleTorque.maxVelocityKMh / 3.6f; //km/h
        _minSpeedMS = _minSpeedKMH / 3.6f;
        _countDownText.text = "Press LEFT or RIGHT\nTRIGGER\nto\nSTART";
        _countDownText.fontSize = 4;
        _vehicleStartPos = _vehicle.transform.position;
        _vehicleStartRot = _vehicle.transform.rotation;
        _vehicleManager = FindObjectOfType<MSSceneControllerFree>();
        _vehicleOnTrack = Vehicle.GetComponent<VehicleOnTrack>();
        _tutorialText.text = "Complete " + (_laps + _testLaps).ToString("0") + " " + _tutorialText.text;

        _tutorialCanvas.gameObject.SetActive(true);
        _tutorialCanvas.transform.Find("PressToStartText").gameObject.SetActive(false);

        float length = 0.0f;
        for (int i = 0; i < walls.Count-1; i++)
            length += Vector3.Distance(walls[i].transform.position, walls[i + 1].transform.position);
        float onelap = length;
        length *= _laps;
        length += Vector3.Distance(_startPos.position, walls[0].transform.position) + Vector3.Distance(_startPos.position, walls[walls.Count - 2].transform.position);
        onelap += Vector3.Distance(_startPos.position, walls[0].transform.position) + Vector3.Distance(_startPos.position, walls[walls.Count - 2].transform.position);
        Debug.Log("One lap: " + onelap + " FullOptimalPath: " + length);

        #region walls
        for (int i = 0; i < walls.Count; i++)
        {
            walls[i].GetComponentInChildren<WallChecker>().OnWallSurpassed.AddListener(WallSurpassed);
        }
        #endregion


        UpdateOSReferences();
    }

    private void WallSurpassed(PlayerBreak wall)
    {

        if (_started && !_ended && !_testRide)
        {
            _wallNum++;
            var _prevIdx = wall._wallId - 1;
            if (wall._wallId == 0)
                _prevIdx = walls.Count - 1;

            _currOptimalPath += Mathf.Abs(Vector3.Distance(
                    walls[wall._wallId].gameObject.transform.position,
                    walls[_prevIdx].gameObject.transform.position
                    ));
            UpdateOSReferences();
        }
    }

    public void CountError() 
    {
        if (_started && !_ended && !_testRide)//if it's not in a test run, count the walls knocked down
        {
            _errors++;
            _errorsText.text = _errors.ToString();            
        }
    }

    public void ArmCountLap(GameObject obj, Collider coll)
    {
        if (_started && !_ended && coll.tag == "Player")
            _lapCounterArmed = true;         
    }

    public void CountLap(GameObject obj, Collider coll)
    {
        if (_started && !_ended && coll.tag == "Player" && _lapCounterArmed)
        {
            _currLap++;

            if (_currLap == 0 && _testRide)
            {
                _lapText.text = "1";
                _TotalLapText.text = (_testLaps).ToString() + "T";
            }
            else if (_currLap == _testLaps)
            {
                _testRide = false; //test runs completed
                _TotalLapText.text = _laps.ToString();
                _vehicleOnTrack._timeEnabled = true;
            }
            else if (_currLap == _testLaps + _laps)
            {
                _vehicleOnTrack._ended = true;
                _vehicleManager.EnableControls(false);
                EndGame(5);
            }
            else
            {
                if (_testRide)
                    _lapText.text = (_currLap + 1).ToString();
                else
                    _lapText.text = (_currLap - _testLaps + 1).ToString();
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ResultToFile(GetResultString(GetResults(Time.time - _startTime), GameManager.Instance._csvSeparator));
                }
            }
            _lapCounterArmed = false;
        }
    }


    internal override void Update()
    {
        base.Update();

        if (!_started && !_ended)
        {
            if (!_canStart && Keyboard.current[Key.LeftCtrl].isPressed && Keyboard.current[Key.I].wasPressedThisFrame)            
                StartCoroutine(StartRace());           
            else if (_canStart && _input.IsLeftTriggerClickedDown || _input.IsRightTriggerClickedDown || Keyboard.current[Key.Space].wasPressedThisFrame)
                _go = true;
        }
        else if (_started && !_ended)
        {
            _timeText.text = FloatTimeToString(GetTime());
            //the first frame in which I exceed the number of laps ==> endgame

        }

        if (Keyboard.current[Key.LeftCtrl].isPressed && Keyboard.current[Key.T].wasPressedThisFrame)
        {
            var rigidbody = _vehicle.GetComponent<Rigidbody>();
            rigidbody.velocity = new Vector3(0f, 0f, 0f);
            rigidbody.angularVelocity = new Vector3(0f, 0f, 0f);
            Vehicle.position = _safePointMan.GetLastPoint().position;
            Vehicle.rotation = _safePointMan.GetLastPoint().rotation;
        };

        if (Keyboard.current[Key.LeftCtrl].isPressed && Keyboard.current[Key.C].wasPressedThisFrame)
            _player.GetComponent<LimitTracking>().Reset();
       
    }

    private IEnumerator StartRace()
    {
        //_tutorialCanvas.gameObject.SetActive(true);
        _canStart = true;
        _tutorialCanvas.transform.Find("PressToStartText").gameObject.SetActive(true);
        yield return new WaitWhile(() => _go == false);

        _countDownText.fontSize = 20;
              
        _countDownText.text = "3";
        _countDown.clip = _countSound;
        //_countDown.gameObject.SetActive(true);
        _countDown.Play();
        yield return new WaitForSeconds(1);
        _countDownText.text = "2";
        _countDown.Play();
        yield return new WaitForSeconds(1);
        _countDownText.text = "1";
        _countDown.Play();
        yield return new WaitForSeconds(1);
        _countDownText.text = "GO!";
        _countDown.clip = _goSound;
        _countDown.Play();
        StartGame();
        _vehicleManager.EnableControls(true);
        _hideMeshes.Hide();
        yield return new WaitForSeconds(_goSound.length);
        _tutorialCanvas.gameObject.SetActive(false);
    }


    internal override void Reset()
    {
        base.Reset();
        _countDownText.text = "Press LEFT or RIGHT\nTRIGGER\nto\nSTART";
        _countDownText.fontSize = 4;
        var rigidbody = _vehicle.GetComponent<Rigidbody>();
        rigidbody.velocity = new Vector3(0f, 0f, 0f);
        rigidbody.angularVelocity = new Vector3(0f, 0f, 0f);
        _vehicle.transform.position = _vehicleStartPos;
        _vehicle.transform.rotation = _vehicleStartRot;
        _vehicleManager.EnableControls(false);
        _hideMeshes.Show();
        _currOptimalPath = 0;
        _collisions = 0;
        _errors = 0;
        _collisionsText.text = "0";
        _errorsText.text = "0";
        _currLap = -1;
        _wallNum = 0;
        _lapText.text = "0";
        _TotalLapText.text = "0";
        _testRide = true;
        _tutorialCanvas.gameObject.SetActive(true);
        _tutorialCanvas.transform.Find("PressToStartText").gameObject.SetActive(false);
        _lapCounterArmed = true;

        walls[0].GetComponentInChildren<BoxCollider>().enabled = true;
        for (int i = 1; i < walls.Count; i++)
        {
            walls[i].GetComponentInChildren<BoxCollider>().enabled = false;
        }

        if (_startRoutine != null)
            { 
            StopCoroutine(_startRoutine);
            _startRoutine = null;
            }
    }

    internal void LogCollision(int _wallIndex)
    {
        if (_started && !_ended)
        {
            if(!_testRide)//if not in the test run, count the walls knocked down
            {
                _collisions++;
                _collisionsText.text = _collisions.ToString();
            }
        }
    }

    internal override float GetAccuracy()
    {
        float ac = -1;
        if(_wallNum >0)
            ac = _collisions / (float)_wallNum;
        return ac;
    }

    //update the O.S. reference values (Operation Speed)
    void UpdateOSReferences()
    {
        _maxSpeedComplTime = _currOptimalPath / _maxSpeedMS;
        _minSpeedComplTime = _currOptimalPath / _minSpeedMS;

        Debug.Log("MaxSpeedComplTime: " + _maxSpeedComplTime + "[s] at speed:" + _maxSpeedMS + " [m/s]\n");
        Debug.Log("MinxSpeedComplTime: " + _minSpeedComplTime + "[s] at speed:" + _minSpeedMS + " [m/s]\n");
    }

    internal override void UpdateTravelDistance()
    {
        if (_started && !_testRide)//if you have not reached the end of the route and are not in the test lap
        {
            base.UpdateTravelDistance();
        }

    }
   
    internal override float GetOperationSpeed(float time)
    {
        float os = -1f;
       
        if (!_testRide)
        {
            float clampedTime;
            if (_maxSpeedComplTime == 0 || _minSpeedComplTime == 0)
                return os;

            //time clamped to this interval
            clampedTime = Mathf.Clamp(time, _maxSpeedComplTime, _minSpeedComplTime);

            //op = 1 if minimum possible time, = 0 if max. time. Otherwise, intermediate values ​​(0.1)
            os = 1f - Mathf.InverseLerp(_maxSpeedComplTime, _minSpeedComplTime, clampedTime);
        }

        return os;

    }

    //returns the percentage of time spent off the road (error)
    internal override float GetErrors(float time)
    {
        float ep = -1f;

        if (!_testRide)
        {
            _timeOffTrack = _vehicleOnTrack.offTrackTime;
            //Mathf.Clamp(_timeOffTrack, 0, time);

            if (_timeOffTrack == 0)
                ep = 1;
            else
                ep = 1 - (_timeOffTrack) / (time);
        }

        return ep; //0 if you have never left the track, 1 if you have always been off the track
    }

    internal override List<string> GetResults(float time)
    {
        var res = base.GetResults(time);
        res.Add((_currLap - _testLaps).ToString());
        res.Add(_collisions.ToString());
        res.Add(_errors.ToString());
        return res;
    }
}

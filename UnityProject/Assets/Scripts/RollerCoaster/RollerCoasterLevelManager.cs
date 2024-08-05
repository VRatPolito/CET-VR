using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RollerCoasterLevelManager : LevelManager
{
    [SerializeField]
    ButtonSequence sequenceSigns; //sequence
    [SerializeField]
    int _laps = 6; //total number of laps
    int _currLap = 0; //number of completed laps
    internal bool isLastLap = false;
    [SerializeField]
    PathCreation.Examples.PathFollower pathFollower;
    [SerializeField]
    PathCreation.PathCreator pathCreator; //curve
    [SerializeField]
    AudioSource RC_AudioSource;
    [SerializeField]
    internal Text _time;
    [SerializeField]
    internal Text _lapNum;
    [SerializeField]
    internal Text _totalLaps;
    [SerializeField]
    internal TextMeshPro _startSignText;
    [SerializeField]
    internal Text _speedText;
    [SerializeField]
    internal Text _errorsText;
    [SerializeField]
    internal int _surpassedSigns = 0;
    int _errors = 0;
    [SerializeField]
    private float minLapTime = 60f, maxLapTime = 150f; //minimum and maximum times for a lap (TESTED)
    [SerializeField]
    GameObject tutorialCanvas;
    [SerializeField]
    Text tutorialText;
    [SerializeField]
    AudioClip countSound, goSound;
    [SerializeField]
    AudioSource startSound;
    [SerializeField]
    internal GameObject cartBase;
    [SerializeField]
    internal ButtonSequence signs;
    [SerializeField]
    HideMeshes _hideMeshes;

    public float maxSpeed = 30f; //maximum carriage speed
    public float minSpeed = 15f; //minimum carriage speed
    public float speedStep = 2f; //speed increase/decrease step
    private float maxSpeedComplTime, minSpeedComplTime; //completion times at max and min speeds for all laps in total

    internal override void Awake()
    {
        base.Awake();
        pathFollower.Speed = minSpeed;
        pathFollower.enabled = false;
        sequenceSigns.OnErrorCommitted.AddListener(ErrorCommitted);
        sequenceSigns.OnSignSurpassed.AddListener(SignSurpassed);
    }

    private void SignSurpassed()
    {
        _surpassedSigns++;
    }

    private void ErrorCommitted()
    {
        _errors++;
        _errorsText.text = _errors.ToString();
    }

    internal override void Start()
    {
        base.Start();

        pathFollower.OnLapCompleted.AddListener(LapCompleted);
        pathFollower.updatePosition(0);

        float circuitLen = pathCreator.path.length;
        tutorialText.text += " " + _laps.ToString("0");
        tutorialCanvas.gameObject.SetActive(true);

        tutorialCanvas.transform.Find("PressToStartText").gameObject.SetActive(false);

        maxSpeedComplTime = minLapTime * _laps;
        minSpeedComplTime = maxLapTime * _laps;

        float length = circuitLen;
        length = circuitLen * _laps;
        Debug.Log("One lap: " + circuitLen + " All laps: " + length);
    }
    internal override void Update()
    {
        base.Update();

        if (!_started && !_ended)
        {
            if (!_canStart && Keyboard.current[Key.LeftCtrl].isPressed && Keyboard.current[Key.I].wasPressedThisFrame)
                StartCoroutine(StartRide());
            else if (_canStart && _input.IsLeftTriggerClickedDown || _input.IsRightTriggerClickedDown || Keyboard.current[Key.Space].wasPressedThisFrame)
                _go = true;
        }
        else if (_started && !_ended)
        {

            _time.text = FloatTimeToString(Time.time - _startTime);       
            _speedText.text = pathFollower.correctSpeed.ToString("0.00");

        }

    }

    public void LapCompleted(int lap)
    {
        if (_started && !_ended)
        {
            //if the experience is finished, finish the execution by collecting the results
            if (lap >= _laps)
            {
                RC_AudioSource.pitch = 1f;
                RC_AudioSource.Stop();
                pathFollower.resetPath(true, false);
                pathFollower.enabled = false;
                _hideMeshes.Show();
                EndGame(5);
            }
            else
            {
                _lapNum.text = (lap + 1).ToString();
                if (lap == _laps - 1)
                {
                    isLastLap = true; //last lap
                    _startSignText.text = "FINISH";
                }
                else if (lap >= 0 && lap < _laps - 1)
                {
                    ShowLapText();
                }

                if (GameManager.Instance != null)
                    GameManager.Instance.ResultToFile(GetResultString(GetResults(Time.time - _startTime), GameManager.Instance._csvSeparator));         
            }
            _currLap = lap;
        }
    }
    internal override void Reset()
    {
        pathFollower.Speed = minSpeed;
        pathFollower.resetPath(true);
        pathFollower.enabled = true;
        pathFollower.startTime = Time.time;
        pathFollower.updatePosition(0);
        _currLap = 0;
        tutorialCanvas.gameObject.SetActive(true);
        tutorialCanvas.transform.Find("PressToStartText").gameObject.SetActive(false);
        _time.text = "00:00";
        _lapNum.text = "0";
        _errorsText.text = "0";        
        _speedText.text = "0";
        _surpassedSigns = 0;
        RC_AudioSource.pitch = 1f;
        RC_AudioSource.Stop();
        sequenceSigns.Reset();
        _startSignText.fontSize = 6f;
        _startSignText.text = "START";
        _errors = 0;
        _hideMeshes.Show();
        base.Reset();

    }
    
    private IEnumerator StartRide()
    {
        //tutorialCanvas.gameObject.SetActive(true);
        tutorialCanvas.transform.Find("PressToStartText").gameObject.SetActive(true);
        _canStart = true;
        yield return new WaitWhile(() => _go == false);
        _startSignText.fontSize = 4f;
        _startSignText.text = "Starting in\n" + "5";
        startSound.clip = countSound;
        startSound.Play();
        yield return new WaitForSeconds(1);
        _startSignText.text = "Starting in\n" + "4";
        startSound.clip = countSound;
        startSound.Play();
        yield return new WaitForSeconds(1);
        _startSignText.text = "Starting in\n"+"3";
        startSound.clip = countSound;
        startSound.Play();
        yield return new WaitForSeconds(1);
        _startSignText.text = "Starting in\n" + "2";
        startSound.clip = countSound;
        startSound.Play();
        yield return new WaitForSeconds(1);
        _startSignText.text = "Starting in\n" + "1";
        startSound.clip = countSound;
        startSound.Play();
        yield return new WaitForSeconds(1);
        _startSignText.fontSize = 6f;
        _startSignText.text = "START";
        startSound.clip = goSound;
        startSound.Play();
        yield return new WaitForSeconds(goSound.length);
        StartGame();
    }
    
    public override void StartGame()
    {
        if (!_started)
        {
            base.StartGame();
            tutorialCanvas.transform.Find("PressToStartText").gameObject.SetActive(false);    

            tutorialCanvas.gameObject.SetActive(false);
            _totalLaps.text = _laps.ToString();

            sequenceSigns.activateFirstSign();
            _startSignText.fontSize = 6f;
            _startSignText.text = "START";

            RC_AudioSource.pitch = 1f;
            RC_AudioSource.Play();
            pathFollower.updatePosition(0);
            pathFollower.resetPath(false);
            pathFollower.enabled = true;
            pathFollower.startTime = Time.time;
            _hideMeshes.Hide();
            Invoke(nameof(ShowLapText), 1);
        }
    }

    void ShowLapText()
    {

        _startSignText.text = "LAP " + (_currLap+1).ToString();
    }

    //NB: CSV header names must be specified in the levemanager gameobjects
    //from the editor
    internal override List<string> GetResults(float time)
    {
        var res = base.GetResults(time);
        res.Add((_currLap).ToString());
        res.Add(GetAVGSpeed(time).ToString("0.00"));
        return res;
    }

    internal override float GetOperationSpeed(float time)
    {
        float os = -1f;
        float clampedTime;
       
        float perc = pathFollower.getCircuitLenPercentage();
        maxSpeedComplTime = minLapTime * (_currLap + perc); //laps already completed plus percentage of the last partial lap completed at the stop
        minSpeedComplTime = maxLapTime * (_currLap + perc);
        //time clamped to this interval
        if(time < minSpeedComplTime) 
            time = minSpeedComplTime;
        else if (time > maxSpeedComplTime)
            time = maxSpeedComplTime;
        clampedTime = Mathf.Clamp(time, maxSpeedComplTime, minSpeedComplTime);
        //Debug.Log(maxSpeedComplTime + " " + minSpeedComplTime + " " + _clampedTime);
        //op = 1 if minimum possible time, = 0 if max. time. Otherwise, intermediate values ​​(0.1)
        os = 1f - Mathf.InverseLerp(maxSpeedComplTime, minSpeedComplTime, clampedTime);
        
            //_op = -1;
        return os;
    }
    //average speed maintained by the user
    float GetAVGSpeed(float time)
    {
        float dist = pathFollower.getDistanceTravelled();
        return dist / time;
    }

    internal override float GetAccuracy()
    {
        return -1;
    }

    internal override float GetErrors(float time)
    {
        float ep = -1;
        if (_started || _ended)
        {
            if (_surpassedSigns > 0)
                ep = (float)sequenceSigns.correctPressures / _surpassedSigns;
            else
                ep = 1;
        }

        return ep;
    }

    public int getTotalLapNumber()
    {
        return _laps;
    }

}

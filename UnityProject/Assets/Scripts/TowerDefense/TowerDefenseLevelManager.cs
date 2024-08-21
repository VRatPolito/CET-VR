using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TowerDefenseLevelManager : LevelManager
{
    [SerializeField]
    CircularLimitTracking _limitTracking;
    [SerializeField]
    private PlatformManager _platform;
    [SerializeField]
    internal int bulletFired; //total number of bullets fired
    [SerializeField]
    internal int targetHits; //number of targets hit correctly
    [SerializeField]
    Text _accuracy;
    [SerializeField]
    internal Text _time;
    [SerializeField]
    Text roundText; //current wave index
    private Shooting _shooting;
    [SerializeField]
    internal Spawner targetSpawner;
    [SerializeField]
    GameObject leftController, rightController;
    [SerializeField]
    TextMeshPro precisionText, playerPointsText, enemyPointsText, gameStatusText;
    [SerializeField]
    GameObject gameStatusUI;
    [SerializeField]
    TextMeshProUGUI gameStatusUIText;
    [SerializeField]
    GameObject tutorialCanvas;
    [SerializeField]
    internal Text _errorsText;
    [SerializeField]
    AudioClip countSound, goSound;
    [SerializeField]
    AudioSource startSound;
    [SerializeField]
    Timer timers;
    [Tooltip("If true, player can rotate platform only when looking the forward direction on the platform itself (that is, the hole inside the transparent walls)")]
    [SerializeField]
    internal bool rotationConstrained = true;

    Vector3 startPos;
    Quaternion startRot;
    int _currWave = -1;
    Bullet_Behaviour[] _bullets;    
    float firstRoundTime = 0;
    bool _cooldown = false;
    Transform leftControllerModel, rightControllerModel;
    bool _hideLater = false;
    bool _showLater = false;
    bool secondRoundOnly = false;
   
    internal override void Awake()
    {
        base.Awake();
        _shooting = Player.GetComponent<Shooting>();
    }

    private void CountError()
    {
        _errorsText.text = (targetSpawner.groundReach + targetSpawner.towerReach).ToString();
        enemyPointsText.text = (targetSpawner.towerReach + targetSpawner.groundReach).ToString("0");
    }

    private void WaveStarted(int wave)
    {
        _currWave = wave;
        roundText.text = (_currWave+1).ToString();
        if(wave == 1)
        {
            _cooldown = false;
            _startTime = Time.time;
        }
    }
    private void WaveEnded(int wave)
    {
        if (wave == 0 && GameManager.Instance != null)
        {
            GameManager.Instance.ResultToFile(GetResultString(GetResults(Time.time - _startTime), GameManager.Instance._csvSeparator));
            firstRoundTime = Time.time - _startTime;
            _nextLog += targetSpawner.waveCoolDownTime;
            _cooldown = true;
        }
    }
    private void LastWaveEnded()
    {
        EndShooting();
    }

    internal override void Start()
    {
        base.Start();
        targetSpawner.OnGroundReach.AddListener(CountError);
        targetSpawner.OnTowerReach.AddListener(CountError);
        targetSpawner.OnWaveStarted.AddListener(WaveStarted);
        targetSpawner.OnWaveEnded.AddListener(WaveEnded);
        targetSpawner.OnGameEnded.AddListener(LastWaveEnded);
        leftControllerModel = leftController.transform.Find("XR Controller Left(Clone)");
        rightControllerModel = rightController.transform.Find("XR Controller Right(Clone)");
        startPos = Player.transform.localPosition;
        startRot = Player.transform.localRotation;

        if (targetHits == targetSpawner.towerReach + targetSpawner.groundReach)
        {
            gameStatusText.text = "DRAW";
            gameStatusText.color = Color.yellow;
        }
        else
        {
            if (targetHits > targetSpawner.towerReach + targetSpawner.groundReach)
            {
                gameStatusText.text = "WINNING";
                gameStatusText.color = Color.green;
            }
            else
            {
                gameStatusText.text = "LOSING";
                gameStatusText.color = Color.red;
            }
        }
        _accuracy.text = "0 %";
        _errorsText.text = "0";
        roundText.text = "0";
        precisionText.text = "100%";
        precisionText.color = Color.yellow;

        playerPointsText.text = "0";
        enemyPointsText.text = "0";
        gameStatusText.text = "DRAW";
        gameStatusText.color = Color.yellow;
        tutorialCanvas.gameObject.SetActive(true);

        tutorialCanvas.transform.Find("PressToStartText").gameObject.SetActive(false);

        _bullets = _shooting._bulletPool;
        for (int i = 0; i < _bullets.Length; i++)
        {
            _bullets[i].OnBulletFired.AddListener(BulletFired);
            _bullets[i].OnTargetHit.AddListener(BulletHit);
        }

        _limitTracking.Reset();
        Player.transform.localPosition = startPos;
        Player.transform.localRotation = startRot;
    }


    internal override void Update()
    {
        base.Update();

        if (Keyboard.current[Key.LeftCtrl].isPressed && Keyboard.current[Key.R].wasPressedThisFrame)
        {
            secondRoundOnly = true;
            StartCoroutine(StartShooting());
            _go = true;
        }
        if (_hideLater)
        {
            if (GameManager.Instance!= null && GameManager.Instance._isLeftHanded)
            {
                leftControllerModel = leftController.transform.Find("XR Controller Left(Clone)");
                if (leftControllerModel != null)
                {
                    foreach (var m in leftControllerModel.GetComponentsInChildren<MeshRenderer>())
                        m.enabled = false;
                    _hideLater = false;
                }
            }
            else
            {
                rightControllerModel = rightController.transform.Find("XR Controller Right(Clone)");
                if (rightControllerModel != null)
                {
                    foreach (var m in rightControllerModel.GetComponentsInChildren<MeshRenderer>())
                        m.enabled = false;
                    _hideLater = false;
                }
            }
        }
        else if (_showLater)
        {
            if (GameManager.Instance != null && GameManager.Instance._isLeftHanded)
            {
                leftControllerModel = leftController.transform.Find("XR Controller Left(Clone)");
                if (leftControllerModel != null)
                {
                    foreach (var m in leftControllerModel.GetComponentsInChildren<MeshRenderer>())
                        m.enabled = true;
                    _showLater = false;
                }
            }
            else
            {
                rightControllerModel = rightController.transform.Find("XR Controller Right(Clone)");
                if (rightControllerModel != null)
                {
                    foreach (var m in rightControllerModel.GetComponentsInChildren<MeshRenderer>())
                        m.enabled = true;
                    _showLater = false;
                }
            }
        }

        if (_started && !_ended && !_cooldown)
        {
            _accuracy.text = GetAccuracy().ToString("0.00");
            precisionText.text = (GetAccuracy()*100).ToString("0") + "%";
            float perc = Mathf.InverseLerp(0f, 1f, bulletFired == 0? 1f : ((float)targetHits / (float)bulletFired));
            precisionText.color = Color.Lerp(Color.white, Color.yellow, perc);
            _time.text = FloatTimeToString(firstRoundTime + (Time.time - _startTime));
        }
        else
        {
            if(!_canStart && Keyboard.current[Key.LeftCtrl].isPressed && Keyboard.current[Key.I].wasPressedThisFrame)
            {
                StartCoroutine(StartShooting());
            }
            else if (_canStart && _input.IsLeftTriggerClickedDown || _input.IsRightTriggerClickedDown || Keyboard.current[Key.Space].wasPressedThisFrame)
                _go = true;
        }

        if (Keyboard.current[Key.LeftCtrl].isPressed && Keyboard.current[Key.C].wasPressedThisFrame)
        {
            _limitTracking.Reset();
        }


    }
    
    private IEnumerator StartShooting()
    {
        //tutorialCanvas.gameObject.SetActive(true);
        timers.showTimers();
        _limitTracking.Reset();
        gameStatusUI.SetActive(false);

        tutorialCanvas.transform.Find("PressToStartText").gameObject.SetActive(true);

        _canStart = true;
        //wait until the player presses the two triggers, then a 10 second countdown starts
        yield return new WaitWhile(() => _go == false);
        for(int i = 3; i>=1; i--)
        {
            timers.setTimers("Starting in", i.ToString("00"));
            startSound.clip = countSound;
            startSound.Play();
            yield return new WaitForSeconds(1);
        }
        timers.setTimers("START", "");
        startSound.clip = goSound;
        startSound.Play();

        yield return new WaitForSeconds(goSound.length);


        //this.Reset();
        StartGame();
    }
    
    internal override void Reset()
    {

        base.Reset();
        firstRoundTime = 0;
        _limitTracking.Reset();
        _time.text = "00:00";
        Player.transform.localPosition = startPos;
        Player.transform.localRotation = startRot;
        precisionText.text = "100%";
        precisionText.color = Color.yellow;

        _platform.Blocked = true;
        _shooting.resetBullets();
        _shooting.firingEnabled = false;
        _shooting.enableLaserPointer(false);

        _errorsText.text = "0";
        _accuracy.text = "0 %";
        roundText.text = "0";
        bulletFired = 0;
        targetHits = 0;

        playerPointsText.text = "0";
        enemyPointsText.text = "0";

        gameStatusText.text = "DRAW";
        gameStatusText.color = Color.yellow;
        gameStatusUIText.text = "";
        gameStatusUIText.color = Color.white;
        gameStatusUI.SetActive(false);

        _platform.Reset();

        targetSpawner.Reset();

        tutorialCanvas.SetActive(true);

        _shooting.HideGun();

        tutorialCanvas.transform.Find("PressToStartText").gameObject.SetActive(false);
    }

    public void HideLeftController()
    {
        if (leftControllerModel != null)
        {
            foreach (var m in leftControllerModel.GetComponentsInChildren<MeshRenderer>())
                m.enabled = false;
        }
        else
        {
            _showLater = false;
            _hideLater = true;
        }

    }
    public void ShowLeftController()
    {
        if (leftControllerModel != null)
        {
            foreach (var m in leftControllerModel.GetComponentsInChildren<MeshRenderer>())
                m.enabled = true;
        }
        else
        {
            _showLater = true;
            _hideLater = false;
        }
    }
    public void HideRightController()
    {
        if (rightControllerModel != null)
        {
            foreach (var m in rightControllerModel.GetComponentsInChildren<MeshRenderer>())
                m.enabled = false;
        }
        else
        {
            _showLater = false;
            _hideLater = true;
        }
    }
    public void ShowRightController()
    {
        if (rightControllerModel != null)
        {
            foreach (var m in rightControllerModel.GetComponentsInChildren<MeshRenderer>())
                m.enabled = true;
        }
        else
        {
            _showLater = true;
            _hideLater = false;
        }
    }


    public override void StartGame()
    {

        base.StartGame();
        _limitTracking.Reset();
        tutorialCanvas.gameObject.SetActive(false);

        _platform.Blocked = false;

        //the player can only shoot after starting
        //_shooting.ShowGun();
        _shooting.firingEnabled = true;

        #region spawner
        targetSpawner.StartShooting(secondRoundOnly); //starts spawning enemies
        #endregion
    }

    #region endgame & results
    private void EndShooting()
    {
        gameStatusUI.SetActive(true);
        Color col = Color.yellow;
        string result = "";
        if (targetHits == targetSpawner.towerReach + targetSpawner.groundReach)
        {
            result = "DRAW";
        }
        else if (targetHits > targetSpawner.towerReach + targetSpawner.groundReach)
        {
            result = "VICTORY!";
            col = Color.green;
        }

        else if (targetHits < targetSpawner.towerReach + targetSpawner.groundReach)
        {
            result = "DEFEAT";
            col = Color.red;
        }
        gameStatusUIText.text = result;
        gameStatusUIText.color = col;

        EndGame(3);
    }


    internal override float GetOperationSpeed(float time)
    {
        return -1;
    }
    //NB: CSV header names must be specified in the levemanager gameobjects
    //from the editor
    internal override List<string> GetResults(float time)
    {
        var res = base.GetResults(time);
        res.Add((getRoundNum()).ToString());
        return res;
    }
  
    public void BulletFired()
    {
        bulletFired++;
    }
    public void BulletHit()
    {
        targetHits++;
        playerPointsText.text = targetHits.ToString("0");
        if (targetHits == targetSpawner.towerReach + targetSpawner.groundReach)
        {
            gameStatusText.text = "DRAW";
            gameStatusText.color = Color.yellow;
        }
        else
        {
            if (targetHits > targetSpawner.towerReach + targetSpawner.groundReach)
            {
                gameStatusText.text = "WINNING";
                gameStatusText.color = Color.green;
            }
            else
            {
                gameStatusText.text = "LOSING";
                gameStatusText.color = Color.red;
            }
        }
    }

    internal override float GetAccuracy()
    {

        float ac = -1;
        if (_started || _ended)
        { 
        if (bulletFired != 0)
            ac = targetHits / (float)bulletFired;
        else
            ac = 1;
        }
        return ac;
    }

    internal override float GetErrors(float time)
    {
        float ep = -1;
        if (_started || _ended)
        {
            int _enemyNum = targetSpawner.GetSpawnedEnemyNum();
            if (_enemyNum != 0)
                ep = 1 - ((targetSpawner.towerReach + targetSpawner.groundReach) / (float)_enemyNum);
            else
                ep = 1;
        }
        return ep;
    }

    //current wave number
    private int getRoundNum()
    {
        return _currWave;
    } 
    
    #endregion
}

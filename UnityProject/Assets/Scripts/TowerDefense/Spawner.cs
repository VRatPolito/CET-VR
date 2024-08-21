using UnityEngine;
using UnityEngine.Events;

public class Spawner : MonoBehaviour
{
    public class WaveEvent : UnityEvent<int> { };
    #region enemies
    public GameObject normalEnemy, flyingEnemy;
    [SerializeField]
    private GameObject targetParent; //to better organize targets in the Unity scene
    private GameObject t_parent;
    
    [SerializeField]
    private int enemiesPerWave = 110; //number of enemies per single wave: Wave duration/spawn interval (automatic adaptation of the number of enemies)
    [SerializeField]
    private int enemiesPerInterval; //number of enemies that spawn at the same time
    [SerializeField]
    internal int spawnedEnemy = 0; //per wave
    [SerializeField]
    internal int enemyNumber = 0; //total number of enemies spawned

    [SerializeField]
    bool _countSurvivedEnemiesAsErrors = false;

    internal int towerReach = 0; //for wave 1: number of enemies that have reached the tower
    internal int groundReach = 0; //for wave 2: number of enemies that have reached the ground

    public UnityEvent OnTowerReach = new UnityEvent();
    public UnityEvent OnGroundReach = new UnityEvent();
    #endregion

    #region spawn times
    public float spawnInterval = 3.0f; //nemici spawnati ogni spawnInterval secondi
    [SerializeField]
    private float minSpawnInterval = 2f, maxSpawnInterval = 9f;
    private int lastSpawnTime = -1; //last moment (in whole seconds) that an enemy spawned
    private float spawnRate_acc = 2.0f; //acceleration in enemy spawn rate (otherwise linear via lerp)
    [SerializeField, Tooltip("if true, during the second round the script will spawn both ground & flying enemies")]
    bool secondRoundMixed = false; 

    #endregion

    #region wave properties
    [SerializeField]
    internal float waveCoolDownTime = 10f; //time between the end of one wave and the beginning of another, or before the start
    [SerializeField]
    private float startCoolDownTime = 0f; //time before starting the experience
    private bool waveSpawn = false;
    internal bool spawn = false;

    //timed waves (2x 5 min)
    public int totalWaves = 2;
    internal int _currWave = 0;
    
    public float waveDuration = 300.0f; // in seconds
    public float waveEndingTime = 0f; //last moments of the wave in which no more enemies spawn
    #endregion

    #region other properties
    [SerializeField]
    private float curTime = 0.0f;

    private int spawnPoint_idx = 0; //enemies can be spawned at multiple spawn points (one at a time): spawnpoints are the children of the "Target Spawner" gameObj
    private int spawnPointNum; //total number of spawn points in the scene
    [SerializeField]
    private bool started = false; //indicates whether the experience has begun

    [SerializeField]
    private Timer timers;
    [SerializeField]
    private GameObject Terrain;
    public UnityEvent<int> OnWaveStarted = new WaveEvent();
    public UnityEvent<int> OnWaveEnded = new WaveEvent();
    public UnityEvent OnGameEnded = new UnityEvent();
    #endregion

    private void Awake()
    {
        OnWaveStarted.AddListener(WaveStarted);
        OnWaveEnded.AddListener(WaveEnded);
        if (enemiesPerInterval == 0)
            enemiesPerInterval = 1;
        spawnPointNum = this.transform.childCount; //has no children other than spawnpoints
        Random.InitState(System.DateTime.Now.Millisecond);
        //enemiesPerWave = Mathf.FloorToInt(waveDuration / spawnInterval) * enemiesPerInterval;
        spawn = false;
    }

    private void WaveEnded(int wave)
    {
        if(wave == 0)
            Terrain.gameObject.GetComponent<CustomTreeColliders>().TreeColliderSetActive(false); //second wave: tree colliders disabled
    }

    private void WaveStarted(int wave)
    {
        if(wave == 0)
            Terrain.gameObject.GetComponent<CustomTreeColliders>().TreeColliderSetActive(true); //first wave: tree colliders enabled

    }

    internal void StartShooting(bool secondRoundOnly = false)
    {
        waveSpawn = true; //enable the first wave spawn
        if(secondRoundOnly)
            _currWave = 1;
        spawn = true;
        towerReach = 0;
        groundReach = 0;
        started = true;
    }

    void Start()
    {
        t_parent = Instantiate(targetParent, Vector3.zero, Quaternion.identity);
        t_parent.name = "Enemies";
    }
    public void CountTowerReach()
    {
        towerReach++;
        OnTowerReach?.Invoke();
    }

    public void CountGroundReach()
    {
        groundReach++;
        OnGroundReach?.Invoke();
    }

    internal void Reset()
    {
        towerReach = 0;
        groundReach = 0;
        curTime = 0.0f;
        spawnPoint_idx = 0;
        _currWave = 0;        
        waveSpawn = true; 
        spawn = false;
        spawnedEnemy = 0;
        _groundEnemy_idx = 0;
        _flyingEnemy_idx = 0;
        _lastSpawnerIndex = -1;
        timers.resetTimers();
        Terrain.gameObject.GetComponent<CustomTreeColliders>().TreeColliderSetActive(true); //first wave: enable colliders
        //destroy all the children of "enemies" present in the scene
        if (t_parent.gameObject != null)
        {
            for (int i = 0; i < t_parent.transform.childCount; i++)
            {
                Destroy(t_parent.transform.GetChild(i).gameObject);
            }
        }
    }

    void Update()
    {
        //if the number of waves has not been completed
        if (started)
        {
            if (spawn)
            {
                if (curTime == 0)
                    OnWaveStarted?.Invoke(_currWave);
                //current instant of time (relative to the start of the wave)
                curTime += Time.deltaTime;
                //update gameobj timers in game
                timers.setTimers("Round " + (_currWave + 1) + "/" + totalWaves,
                    Mathf.Floor(curTime / 60).ToString("0") + ":" + Mathf.Floor((curTime) - 60 * Mathf.Floor(curTime / 60)).ToString("00") +
                    "/" +
                     Mathf.Floor((waveDuration + waveEndingTime) / 60).ToString("0") + ":" + Mathf.Floor((waveDuration + waveEndingTime) - 60 * Mathf.Floor((waveDuration + waveEndingTime) / 60)).ToString("00")
                    );


                float perc = Mathf.InverseLerp(0, waveDuration, spawnRate_acc * curTime);
                spawnInterval = Mathf.Lerp(maxSpawnInterval, minSpawnInterval, perc);
                spawnInterval = Mathf.Round(spawnInterval);

                if (waveSpawn)
                {
                    //spawns an enemy every spawnInterval sec, if it has not exceeded the max number
                    if ((int)curTime % spawnInterval == 0 && spawnedEnemy < enemiesPerWave && curTime <= waveDuration + waveEndingTime)
                    {
                        //no more enemiesPerInterval enemies per time instant
                        if ((int)curTime != lastSpawnTime)
                        {
                            spawnEnemy();
                            lastSpawnTime = (int)curTime;
                        }
                    }
                    else if (spawnedEnemy >= enemiesPerWave || (curTime >= waveDuration && curTime <= waveDuration + waveEndingTime))
                        waveSpawn = false;
                }

                //if it exceeds the duration of the wave, it moves on to the next
                if (curTime >= waveDuration + waveEndingTime)
                {
                    //enable wave spawning
                    waveSpawn = true;
                    //resets the current time
                    curTime = 0.0f;
                    //reset the number of enemies in the wave to zero
                    spawnedEnemy = 0;
                    //at the end of the wave I wait for the cooldown                   
                    spawn = false;
                    //manage enemies still alive
                    enemyOutOfTime();

                    OnWaveEnded?.Invoke(_currWave);
                    //the wave index advances
                    _currWave++;

                    if (_currWave >= totalWaves)
                    {
                        timers.setTimers("GAME", "OVER");
                        started = false; //esperienza finita
                        OnGameEnded?.Invoke();
                    }
                    else
                        Invoke(nameof(StopCoolDown), waveCoolDownTime);

                }
            }
            else //cooldown or end
            {
                curTime += Time.deltaTime;
                if (_currWave == 0)
                {
                    timers.setTimers("Starting in", (Mathf.Floor(startCoolDownTime) - Mathf.Floor(curTime)).ToString("00"));
                }
                else
                {
                    timers.setTimers("Next Round", (Mathf.Floor(waveCoolDownTime) - Mathf.Floor(curTime)).ToString("00"));
                }
            }
        }
    }

    private void StopCoolDown()
    {
        spawn = true;
        curTime = 0;
    }

    //spawns an enemy(Target) from one(or more) target spawners in the scene
    private int _groundEnemy_idx = 0;
    private int _flyingEnemy_idx = 0;

    private void spawnEnemy()
    {
        if(_currWave == 0) //wave of ground enemies
        {
            for (int i = 0; i < enemiesPerInterval; i++)
            {
                GameObject Enemy = Instantiate(normalEnemy, getSpawnPosition(), Quaternion.identity);
                Enemy.tag = "Target";
                Enemy.name = "Enemy_" + _groundEnemy_idx.ToString();
                Enemy.transform.parent = t_parent.transform;
                Enemy.GetComponent<GroundTarget_NAV_Agent>().setSpeed(0, waveDuration+waveEndingTime, curTime);
                _groundEnemy_idx++;
            }
        }
        else if(_currWave == 1) //wave of "flying" enemies too
        {
            for (int i = 0; i < enemiesPerInterval; i++)
            {
                if (spawnedEnemy % 2 == 0 || !secondRoundMixed) //if an even number or if an unmixed round, it spawns flying
                {
                    flyingEnemy.GetComponent<FlyingTarget_Movement>().setStartSpeed(0, waveDuration + waveEndingTime, curTime);
                    GameObject FlyingEnemy = Instantiate(flyingEnemy, getSpawnPosition(true), Quaternion.identity);
                    FlyingEnemy.tag = "FlyingTarget";
                    FlyingEnemy.name = "FlyingEnemy_" + _flyingEnemy_idx.ToString();
                    FlyingEnemy.transform.parent = t_parent.transform;
                    _flyingEnemy_idx++;
                }
                else if (secondRoundMixed && spawnedEnemy%2!=0) //also spawns earthly enemies if the round is mixed and the number is odd
                    {
                    GameObject Enemy = Instantiate(normalEnemy, getSpawnPosition(), Quaternion.identity);
                    Enemy.tag = "Target";
                    Enemy.name = "Enemy_" + _groundEnemy_idx.ToString();
                    Enemy.transform.parent = t_parent.transform;
                    Enemy.GetComponent<GroundTarget_NAV_Agent>().setSpeed(0, waveDuration + waveEndingTime, curTime);
                    _groundEnemy_idx++;
                }
            }
        }
        spawnedEnemy+= enemiesPerInterval;
        enemyNumber += enemiesPerInterval;
    }

    //select a random spawnPoint among those present, avoiding the previous one
    private int _lastSpawnerIndex = -1; 

    GameObject randomSpawnPoint()
    {
        spawnPoint_idx = (spawnPoint_idx + 1) % spawnPointNum;
        if(spawnPointNum != 0)
            return this.gameObject.transform.GetChild(spawnPoint_idx).gameObject; //returns the child to the corresponding index
        else
        {
            //if there are no spawnpoints in the scene, use the Target spawner
            return this.gameObject;
        }
    }

    //returns a random position around the spawnpoint (to make overlaps less likely)

    #region spawn properties
    [SerializeField, HideInInspector]
    private float maxSpawnOffset_orizzontal = 10; //offset massimo (per x o y) intorno allo spawnpoint

    //in caso di flying target, altezza minima e massima di spawn
    [SerializeField, HideInInspector]
    private float minH = 50;
    [SerializeField, HideInInspector]
    private float maxH = 175;
    #endregion

    Vector3 getSpawnPosition(bool flying = false)
    {
        float dx, dz;
        float h;
        dz = Random.Range(-maxSpawnOffset_orizzontal, maxSpawnOffset_orizzontal);
        dx = Random.Range(-maxSpawnOffset_orizzontal, maxSpawnOffset_orizzontal);


        Vector3 position = randomSpawnPoint().transform.position;

        position.x += dx;
        position.z += dz;

        if( _currWave == 1 && flying)//flying target
        {
            h = Random.Range(minH, maxH);
            position.y += h;
        }

        return position;

    }


    /*
    * When the cooldown starts, the remaining enemies are despawned and either counted as errors (as they were not eliminated in time) or not
    */
    public void enemyOutOfTime()
    {
        for(int i = 0; i< t_parent.transform.childCount; i++)
        {
            GameObject _enemy = t_parent.transform.GetChild(i).gameObject;
            if (_countSurvivedEnemiesAsErrors)
            {
                if (_enemy.CompareTag("FlyingTarget"))
                {
                    CountGroundReach(); //count enemies left alive outside the range as errors
                }
                else if (_enemy.CompareTag("Target"))
                {
                    CountTowerReach();
                }
            }
            StartCoroutine(_enemy.gameObject.GetComponent<Target_Behaviour>().Explode(_countSurvivedEnemiesAsErrors)); //display error particles
            Destroy(_enemy, 0.01f); //despawn enemies
        }
    }
    public int GetSpawnedEnemyNum()
    {
        return enemyNumber;
    }

    public string getCoolDown()
    {
        string s = "";

        if (!spawn)
        {
            s = " - cooldown";
        }
        else
        {
            s = " - spawning";
        }
        return s;
    }

    public bool isOnCoolDown()
    {
        return started && !spawn;
    }

    //to view spawnpoints easily on unity
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        
        for(int i = 0; i<spawnPointNum; i++)
        {
            Gizmos.DrawCube(this.transform.GetChild(i).transform.position, new Vector3(0.5f, 0.5f, 0.5f));
        }
    }



}

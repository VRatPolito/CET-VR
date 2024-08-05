using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Target_Behaviour : MonoBehaviour
{
    Spawner spawner;
    [SerializeField]
    private int targetWave; //wave to which the target belongs
    [SerializeField]
    private AudioClip spawnSound, hitSound, reachSound, movementSound;
    AudioSource _source;
    private bool visible = true;

    [SerializeField]
    GameObject fireworkGreen, fireworkRed;

    bool _done = false; //to avoid multiple repetitions

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _source.playOnAwake = false;
        if (GameManager.Instance != null)
            spawner = ((TowerDefenseLevelManager)GameManager.Instance.LevelManager).targetSpawner;
        else
            spawner = GameObject.FindGameObjectWithTag("Spawner").GetComponent<Spawner>();
    }
    void Start()
    {
        //when the object is instantiated, it emits a sound (to easily identify the position)
        _source.clip = spawnSound;
        _source.volume = 0.7f;
        _source.spatialBlend = 1f;
        _source.spread = 360f;
        _source.spatialize = true;
        _source.Play();

        targetWave = spawner.gameObject.GetComponent<Spawner>()._currWave; //to know which wave the spawned target belongs to

    }

    private void Update()
    {
        if(_source.clip == spawnSound && !_source.isPlaying)
        {
            _source.clip = movementSound;
            _source.volume = 1f;
            _source.loop = true;
            _source.spatialBlend = 1f;
            _source.spread = 360f;
            _source.spatialize = true;
            _source.pitch = 1f;
            _source.Play();
        }
        
        bool isFlying = (this.tag == "FlyingTarget")? true : false;

        //spawner.GetComponent<EnemyRadar>().turnOnNearestLight(this.transform, isFlying);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Bullet"))
        {
            if (!_done)
            {
                GetComponent<Rigidbody>().isKinematic = true; //avoid bounce back
                _source.Stop();
                _source.clip = hitSound;
                _source.loop = false;
                _source.spatialBlend = 0.5f;
                _source.spread = 360f;
                _source.pitch = 1f;
                _source.spatialize = true;
                _source.Play();

                showGameObject(gameObject, false);
                StartCoroutine(Explode(false));
                //this.gameObject.SetActive(false);
                for(int i=0; i < this.gameObject.transform.childCount; i++)
                {
                    transform.GetChild(i).gameObject.SetActive(false);
                }

                gameObject.transform.GetChild(0).gameObject.SetActive(false);
                Destroy(gameObject, hitSound.length);
                _done = true;
            }
           
        }

        else if(collision.gameObject.CompareTag("Tower"))
        {
            
            if(gameObject.CompareTag("Target")) //if ground enemy
            {
                
                //error sound
                if (!_done)
                {
                    //Debug.Log("TowerReach\n");
                    GetComponent<Rigidbody>().isKinematic = true; //avoid bounce back
                    _source.Stop();
                    _source.loop = false;
                    _source.clip = reachSound;
                    _source.spatialBlend = 1f;
                    _source.spread = 360f;
                    _source.pitch = 1f;
                    _source.spatialize = true;
                    _source.Play();
                    spawner.CountTowerReach();
                    showGameObject(gameObject, false);
                    _done = true;
                    StartCoroutine(Explode(true));
                    gameObject.transform.GetChild(0).gameObject.SetActive(false);
                    Destroy(gameObject, reachSound.length/2f);
                }
                
                
            }
        }
        else if (collision.gameObject.CompareTag("Ground"))
        {
            if (this.gameObject.CompareTag("FlyingTarget")) //if flying target
            {
                
                //error sound
                if (!_done )
                {                   
                    //Debug.Log("GroundReach\n");
                    GetComponent<Rigidbody>().isKinematic = true; //avoid bounce back
                    _source.Stop();
                    _source.loop = false;
                    _source.clip = reachSound;
                    _source.spatialBlend = 0.6f;
                    _source.spread = 360f;
                    _source.pitch = 1f;
                    _source.spatialize = true;
                    _source.Play();
                    spawner.CountGroundReach();
                    showGameObject(gameObject, false);
                    _done = true;
                    StartCoroutine(Explode(true));
                    gameObject.transform.GetChild(0).gameObject.SetActive(false);
                    Destroy(gameObject, reachSound.length / 2f);
                    
                }
                
                
            }
        }

        //avoid inferference
        else if (collision.gameObject.CompareTag("Target") || collision.gameObject.CompareTag("FlyingTarget"))
        {
            GetComponent<Rigidbody>().isKinematic = true; //avoid bounce back
        }

    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Target") || collision.gameObject.CompareTag("FlyingTarget"))
        {
            GetComponent<Rigidbody>().isKinematic = false; //avoid bounce back
        }
    }
    //hide or make visible a gameObject or its children (if composed)
    void showGameObject(GameObject obj, bool show)
    {
        if(obj.gameObject.GetComponent<MeshRenderer>() != null)
        {
            obj.gameObject.GetComponent<MeshRenderer>().enabled = show;
        }
        else //hide any children (for composite meshes like the robot)
        {
            for(int i = 0; i < obj.transform.childCount; i++)
            {
                GameObject child = obj.transform.GetChild(i).gameObject;
                if(child.gameObject.GetComponent<MeshRenderer>() != null)
                {
                    child.gameObject.GetComponent<MeshRenderer>().enabled = show;
                }
            }
        }
        visible = show;

    }
    public bool isVisible()
    {
        return visible;
    }

    //Hit target animation
    public IEnumerator Explode(bool error = false, float time = 0)
    {
        GameObject firework = error ? fireworkRed : fireworkGreen;
        Vector3 pos = this.transform.position;
        pos.x += 1f;
        GameObject f = Instantiate(firework, pos, Quaternion.identity);
        GameObject f_particle = f.transform.GetChild(0).gameObject;
        Destroy(f, 1.5f);
        var m = f_particle.GetComponent<ParticleSystem>().main;
        m.loop = false;

        f_particle.GetComponent<ParticleSystem>().Play();
        yield return new WaitForSeconds(1.2f);
        f_particle.GetComponent<ParticleSystem>().Stop();
       // Destroy(f);
        yield return new WaitForSeconds(0);
    }


}

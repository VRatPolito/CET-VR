using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBreak : MonoBehaviour
{
    [SerializeField]
    GameObject _wall;
    [SerializeField]
    Transform _kinematicWall;
    Transform _wallInstance;
    internal int _wallId;
    Coroutine _routine;
    bool broken = false;
    AudioSource _source;
    [SerializeField, Range(5f, 30f)]
    float repairTime = 15f;
    CollisionManager _manager;
    TrackRaceLevelManager _levelManager;

    // Start is called before the first frame update
    void Awake()
    {
        _manager = _kinematicWall.GetComponent<CollisionManager>();
        _source = GetComponent<AudioSource>();
        if (GameManager.Instance != null)
            _levelManager = ((TrackRaceLevelManager)GameManager.Instance.LevelManager);
        else
            _levelManager = FindObjectOfType<TrackRaceLevelManager>();
    }

    private void Start()
    {
        _manager.collisionDetected.AddListener(BreakWall);
    }

    private void BreakWall(GameObject first, GameObject other)
    {
        if(!broken && other.tag == "Player")
        {
            _kinematicWall.gameObject.SetActive(false);
            _source.Play();
            _wallInstance = Instantiate(_wall, transform).transform;
            _routine = StartCoroutine(RepairWall());
            _levelManager.LogCollision(_wallId);
             broken = true;
        }
    }

    private IEnumerator RepairWall()
    {
        yield return new WaitForSeconds(0.05f);
        foreach (var b in _wallInstance.GetComponentsInChildren<BrickLine>())
            b.SetBricksLayer("VehicleIgnore");
        //yield return new WaitForSeconds(4.95f);
        yield return new WaitForSeconds(repairTime - 0.05f);
        _kinematicWall.gameObject.SetActive(true);
        Destroy(_wallInstance.gameObject);
        _wallInstance = null;
        broken = false;
    }
}

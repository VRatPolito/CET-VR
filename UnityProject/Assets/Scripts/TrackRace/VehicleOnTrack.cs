using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VehicleOnTrack : MonoBehaviour
{
    public bool isOffTrack = false;
    public bool isOnCollision = false;
    [SerializeField]
    private GameObject[] _errorColliders; //list of elements whose collision is considered an error
    [SerializeField]
    private List<Collider> _allColliderObj;
    [SerializeField]
    GameObject Terrain;
    [SerializeField]
    GameObject _raycastEmitter;
    public float offTrackTime = 0f;
    public bool _timeEnabled = false; //when it becomes true, it starts counting time
    public bool _ended = false; //stops calculating time if the experience is over
    bool wasOffTrack = false;

    public UnityEvent OnOffTrack = new UnityEvent();
    public UnityEvent OnOnTrack = new UnityEvent();

    private void Start()
    {

        for (int i = 0; i < _errorColliders.Length; i++)
        {
            //save all the individual colliders
            for (int j=0; j< _errorColliders[i].transform.childCount; j++)
            {
                if(_errorColliders[i].transform.GetChild(j).GetComponentInChildren<Collider>() != null)
                    _allColliderObj.Add(_errorColliders[i].transform.GetChild(j).GetComponentInChildren<Collider>());
            }
            if(_errorColliders[i].transform.childCount == 0)
            {
                _allColliderObj.AddRange(new List<Collider>() { _errorColliders[i].GetComponentInChildren<Collider>() });
            }
        }
    }
    private void Update()
    {
        wasOffTrack = isOffTrack;
        isOffTrack = isOutOfTrack();
        if (isOffTrack)
        {
            if(!wasOffTrack)
                OnOffTrack?.Invoke();
            if (_timeEnabled && !_ended)
                offTrackTime += Time.deltaTime;
        }
        else if(wasOffTrack)
            OnOnTrack?.Invoke();
    }
    
    private void OnCollisionStay(Collision collision)
    {
        bool _colliderFound = false;
        //when it is off track, we always consider error. If inside the track, we check for collisions with objects
        if (!isOffTrack)
        {
            for (int i = 0; i < _allColliderObj.Count; i++)
            {
                if (_allColliderObj[i].gameObject == collision.gameObject)
                {
                    _colliderFound = true;
                    break;
                }
            }
            if (_colliderFound)
            {
                isOnCollision = true;
                if (_timeEnabled && !_ended)
                    offTrackTime += Time.fixedDeltaTime;
            }
            else
            {
                isOnCollision = false;
            }
        }

    }
    private void OnCollisionExit(Collision collision)
    {
        bool _colliderFound = false;

        for (int i = 0; i < _allColliderObj.Count; i++)
        {
            if (_allColliderObj[i].gameObject == collision.gameObject)
            {
                _colliderFound = true;
                break;
            }
        }
        if (_colliderFound)
        {
            isOnCollision = false;
        }
    }

    private bool isOutOfTrack()
    {
        var dir = -Vector3.up; //the direction is downward, to control the terrain
        RaycastHit hit;
        bool OffTrack = false; //by default, the vehicle is on the track

        if (Physics.Raycast(_raycastEmitter.transform.position, dir, out hit))
        {
            //Debug.Log("Hit: " + hit.collider.gameObject.name + "\n");
            if(hit.collider == Terrain.gameObject.GetComponent<TerrainCollider>())
            {
                OffTrack = true; //vehicle off the track
            }
        }
        return OffTrack;
    }
}

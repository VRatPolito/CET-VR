using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerColliderManager : MonoBehaviour
{
    Transform _head;
    [SerializeField]
    float height = 0.0f;
    Vector3 prevpos = Vector3.zero;
    private CharacterController _charc;

    public enum Position { Standing, Crouched, Proned };
    Position Posizione = Position.Standing;
    // Use this for initialization
    private void Awake()
    {
        if (GameManager.Instance != null) 
        { 
            _head = GameManager.Instance.LevelManager.PlayerHead;
            _charc = GetComponent<CharacterController>();
        }
        else
        {
            _head = Camera.main.transform;
            _charc = GetComponent<CharacterController>();
        }
    }

    public void EnableCollider()
    {
        if (_charc != null)
            _charc.enabled = true;
    }

    public void DisableCollider()
    {

        if (_charc != null)
            _charc.enabled = false;
    }

    void Start()
    {
        if (height == 0.0f)
            height = GameManager.Instance.LevelManager.PlayerHead.localPosition.y;

        if (_charc != null)
            _charc.height = height;

        var headpos = _head.position;
        ManageCollider(Vector3.zero);
        prevpos = headpos;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        var headpos = _head.position;
        if (_head.position != prevpos)
        {
            var offset = _head.position - transform.position;
            ManageCollider(offset);
            prevpos = headpos;
        }
    }

    private void ManageCollider(Vector3 offset)
    {
        if (_head.localPosition.y >= height)
        {
            if (_charc != null)
            {
                _charc.height = height;
                _charc.center = new Vector3(offset.x, height / 2, offset.z);
            }

            if (Posizione != Position.Standing)
                Posizione = Position.Standing;
        }
        else if (_head.localPosition.y >= 1 && _head.localPosition.y < height)
        {
            if (_charc != null)
            {
                _charc.height = _head.localPosition.y; ;
                _charc.center = new Vector3(offset.x, _head.localPosition.y / 2, offset.z);
            }

            if (Posizione != Position.Standing)
                Posizione = Position.Standing;
        }
        else if (_head.localPosition.y < 1)
        {
            if (_charc != null)
            {
                _charc.height = 1;
                _charc.center = new Vector3(offset.x, 0.5f, offset.z);
            }

            if (Posizione != Position.Crouched)
                Posizione = Position.Crouched;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingArea : MonoBehaviour
{
    [SerializeField]
    SphereCollider _leftHand;
    [SerializeField]
    SphereCollider _rightHand;
    [SerializeField]
    GameObject _cylinder;
    [SerializeField]
    GameObject _prism;
    [SerializeField]
    GameObject _gun;
    [SerializeField]
    Shooting _shooting;
    bool _cylinderEntered = true;
    bool _prismEntered = false;


    //when it enters the area it can shoot
    public void TriggerEnter(GameObject collider, Collider other)
    {
        if (collider == _prism)
            _prismEntered = true;
        else if (collider == _cylinder)
            _cylinderEntered = true;
        if ((_prismEntered || _cylinderEntered) && ((_shooting._isLeftHanded && other == _leftHand) || (!_shooting._isLeftHanded && other == _rightHand)))
        {
            _shooting.ShowGun();
        }
    }

    //when it exits the area it can't shoot
    public void TriggerExit(GameObject collider, Collider other)
    {
        if (other.gameObject == _gun)
        {
            if (collider == _prism)
                _prismEntered = false;
            else if (collider == _cylinder)
                _cylinderEntered = false;

            if (!_prismEntered && !_cylinderEntered)
            {
                _shooting.HideGun();
            }
        }
    }
}

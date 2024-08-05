using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeParenting : MonoBehaviour {
    [SerializeField]
    Transform StartingFakeParent;
    Transform FakeParent;
    public Vector3 FakelocalPosition;
    Quaternion startParentRotationQ, StartRotationQ;
    public bool IgnoreFakelocalPosition = false;
    public Vector3 RotationMult = Vector3.one;
    public event Action OnFakeParentingUpdated;
    // Use this for initialization
    void Start () {
        if(StartingFakeParent != null)
            SetFakeParent(StartingFakeParent);
	}

    public void SetFakeParent(Transform Parent)
    {
        FakeParent = Parent;
        FakelocalPosition = Vector3.zero;
        if (Parent != null)
            {
            StartRotationQ = transform.rotation;
            startParentRotationQ = Parent.rotation;
            }
        else
            {
            StartRotationQ = Quaternion.identity;
            startParentRotationQ = Quaternion.identity;
            }
    }

   public void SetFakeParent(Transform Parent, Vector3 FakelocalPos)
    {
        FakeParent = Parent;
        if (Parent != null)
        {
            FakelocalPosition = FakelocalPos;
            StartRotationQ = transform.rotation;
            startParentRotationQ = Parent.rotation;
        }
        else
        {
            FakelocalPosition = Vector3.zero;
            StartRotationQ = Quaternion.identity;
            startParentRotationQ = Quaternion.identity;
        }
    }

    public void SetFakeParent(Transform Parent, Vector3 FakelocalPos, Quaternion FakelocalRot, bool resetStartParentRot = false)
    {
        FakeParent = Parent;
        if (Parent != null)
        {
            FakelocalPosition = FakelocalPos;
            StartRotationQ = FakelocalRot;
            if (resetStartParentRot)
                startParentRotationQ = Quaternion.identity;
            else
                startParentRotationQ = Parent.rotation;
        }
        else
        {
            FakelocalPosition = Vector3.zero;
            StartRotationQ = Quaternion.identity;
            startParentRotationQ = Quaternion.identity;
        }
    }


    // Update is called once per frame
    private void LateUpdate()
    {
        Matrix4x4 parentMatrix;
        if (FakeParent != null)
        {
            parentMatrix = Matrix4x4.TRS(FakeParent.position, FakeParent.rotation, FakeParent.lossyScale);
            if(!IgnoreFakelocalPosition)
                transform.position = parentMatrix.MultiplyPoint3x4(FakelocalPosition);
            transform.rotation = FakeParent.rotation * Quaternion.Inverse(startParentRotationQ);
            if (RotationMult != Vector3.one)
                transform.rotation = Quaternion.Euler(RotationMult.x * transform.eulerAngles.x, RotationMult.y * transform.eulerAngles.y, RotationMult.z * transform.eulerAngles.z);
            transform.rotation = transform.rotation * StartRotationQ;

            if (OnFakeParentingUpdated != null)
                OnFakeParentingUpdated.Invoke();
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideMeshes : MonoBehaviour
{
    [SerializeField]
    List<MeshRenderer> _meshes;
    [SerializeField]
    internal bool _hideMeshes;
    // Start is called before the first frame update
    void Start()
    {
        if (_hideMeshes)
            Hidden(true);
    }

    void Hidden(bool value)
    {
        foreach (var m in _meshes)
            if(value)
                m.gameObject.layer = LayerMask.NameToLayer("PlayerInvisible");
            else
                m.gameObject.layer = LayerMask.NameToLayer("Vehicle");
    }

    public void Hide()
    {
        Hidden(true);
    }
    public void Show()
    {
        Hidden(false);
    }
}

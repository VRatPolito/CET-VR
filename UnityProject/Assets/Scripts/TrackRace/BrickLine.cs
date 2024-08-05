using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickLine : MonoBehaviour
{
    [SerializeField]
    GameObject[] _bricks;

    internal void SetBricksLayer(string layer)
    {
        foreach (var b in _bricks)
            b.layer = LayerMask.NameToLayer(layer);
    }
}

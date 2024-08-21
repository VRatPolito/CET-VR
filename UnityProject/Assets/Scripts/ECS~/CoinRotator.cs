using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public class CoinRotator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        Entity entity = entityManager.CreateEntity(typeof(Rotation), typeof(RotationSpeed));
    }
}

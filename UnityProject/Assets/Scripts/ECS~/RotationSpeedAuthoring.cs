using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

// Authoring component
public class RotationSpeedAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{

    [SerializeField]
    internal float _degreesPerSecond = 1f;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new RotationSpeed { DegreesPerSecond = _degreesPerSecond });
    }
}

// Runtime component
public struct RotationSpeed : IComponentData
{
    public float DegreesPerSecond;
}

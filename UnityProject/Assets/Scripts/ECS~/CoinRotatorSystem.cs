using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class CoinRotatorSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref RotationSpeed rotationSpeed, ref Rotation rotation) => {
            rotation.Value = rotation.Value * Quaternion.Euler(Vector3.up * rotationSpeed.DegreesPerSecond * Time.DeltaTime);
        });
    }
}

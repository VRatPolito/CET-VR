using UnityEngine;
using System.Collections;
using UnityStandardAssets.Water;

[ExecuteInEditMode]
public class WaterTileStereo : MonoBehaviour
{
    public PlanarReflectionStereo reflection;
    public WaterBase waterBase;

    public void Start()
    {
        AcquireComponents();
    }


    void AcquireComponents()
    {
        if (!reflection)
        {
            if (transform.parent)
            {
                reflection = transform.parent.GetComponent<PlanarReflectionStereo>();
            }
            else
            {
                reflection = transform.GetComponent<PlanarReflectionStereo>();
            }
        }

        if (!waterBase)
        {
            if (transform.parent)
            {
                waterBase = transform.parent.GetComponent<WaterBase>();
            }
            else
            {
                waterBase = transform.GetComponent<WaterBase>();
            }
        }
    }


#if UNITY_EDITOR
        public void Update()
        {
            AcquireComponents();
        }
#endif


    public void OnWillRenderObject()
    {
        if(Camera.current.name != "Camera (eye)")
        {
            return;
        }
        if (reflection)
        {
            reflection.WaterTileBeingRendered(transform, Camera.current);
        }
        if (waterBase)
        {
            waterBase.WaterTileBeingRendered(transform, Camera.current);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint_Gizmos : MonoBehaviour
{
    // Start is called before the first frame update
    [ExecuteInEditMode]

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(this.transform.position, new Vector3(0.5f, 0.5f, 0.5f));
    }
}

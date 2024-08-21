using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomTreeColliders : MonoBehaviour
{
    [SerializeField]
    GameObject _simplifiedTreeCollider;
    // Start is called before the first frame update
    GameObject _terrainObj;
    Terrain _terrain;

    List<GameObject> treeColliders;

    private void Start()
    {
        _terrainObj = GameObject.FindGameObjectWithTag("Ground");
        _terrain = Terrain.activeTerrain;
        treeColliders = new List<GameObject>();

        foreach (TreeInstance t in _terrain.terrainData.treeInstances)
        {
            GameObject _tc = Instantiate(_simplifiedTreeCollider, treePosToWorldPoint(t.position), Quaternion.identity);
            _tc.transform.parent = _terrainObj.transform;
            _tc.tag = "Obstacle";
            treeColliders.Add(_tc);
        }

    }

    public void TreeColliderSetActive(bool active)
    {
        foreach(GameObject _tc in treeColliders)
        {
            _tc.SetActive(active);
        }
    }
    Vector3 treePosToWorldPoint(Vector3 treePos)
    {
        float terrainWidth = _terrain.terrainData.size.x;
        float terrainheight = _terrain.terrainData.size.z;
        float terrainY = _terrain.terrainData.size.y;
        Vector3 pos = new Vector3(treePos.x * terrainWidth, treePos.y * terrainY, treePos.z * terrainheight);
        pos.x += _terrainObj.gameObject.transform.position.x;
        pos.z += _terrainObj.gameObject.transform.position.z;
        pos.y += _terrainObj.gameObject.transform.position.y;
        pos.y += _simplifiedTreeCollider.transform.GetChild(0).transform.localScale.y/2f;
        return pos;
    }

    public void IgnoreCollisionWithTrees(GameObject other)
    {
        foreach(GameObject tree in treeColliders)
        {
            Physics.IgnoreCollision(other.gameObject.GetComponent<Collider>(),
                                    tree.transform.GetChild(0).gameObject.GetComponent<Collider>());
        }
    }

}

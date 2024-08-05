using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Virtual_Cave : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject _caveCubePrefab;
    public GameObject player;
    private GameObject caveCubeObj;
    private Vector3 _playerPos;

    #region Scale
    [Header("Scale")]
    [Range(1.5f, 10.0f)]
    public float length;
    [Range(1.5f, 10.0f)]
    public float width;
    [Range(3.0f, 10.0f)]
    public float height; 
    private Vector3 _scale;
    [Range(-10.0f, 10.0f)]
    public float offset = 0f; //for corrections on grid height
    #endregion
    #region Rotation

    public enum CaveFollowRotationMode
    {
        /// <summary>
        /// Cave Rotation fixed.
        /// </summary>
        OFF,
        /// <summary>
        /// Cave rotation follow rotation on player y_axis (vertical).
        /// </summary>
        Y_AXIS,
        /// <summary>
        /// Cave rotation follow rotation on all player axes
        /// </summary>
        ALL_AXIS
    }
    [Header("Rotation Properies")]
    [SerializeField]
    CaveFollowRotationMode rotationFollowMode = CaveFollowRotationMode.ALL_AXIS; //by default, it will follow all rotations of the player (Not his camera!)
    private Vector3 _startRotation;
    #endregion
    #region ShaderProperties
    [Header("Material Properties")]
    [Range(0.001f, 0.01f)]
    public float gridLineThickness;
    #endregion
    void Start()
    {
        _playerPos = player.transform.position;
        _playerPos.y += offset;
        caveCubeObj = Instantiate(_caveCubePrefab, _playerPos, Quaternion.identity);
        caveCubeObj.gameObject.SetActive(true);
        updateCubeRotation();
        _startRotation = caveCubeObj.transform.eulerAngles;
        //material
        caveCubeObj.GetComponent<MeshRenderer>().material.SetFloat("_Thickness", gridLineThickness);
        
    }
    private void Awake()
    {
        //default values
        width = 3.0f;
        length = 4.0f;
        height = 3.0f;
        _scale = new Vector3(width, height, length);
        gridLineThickness = 0.001f;
        //rotationFollowMode = CaveFollowRotationMode.Y_AXIS; //by default, it will follow all rotations of the player (Not his camera!)
    }
    // Update is called once per frame
    void Update()
    {

        _scale.x = width;
        _scale.y = height;
        _scale.z = length;
        if (rotationFollowMode == CaveFollowRotationMode.ALL_AXIS)
        {
            caveCubeObj.gameObject.transform.parent = player.transform;
            Vector3 localPos = caveCubeObj.gameObject.transform.localPosition;
            localPos.y = offset;
            caveCubeObj.gameObject.transform.localPosition = localPos;

            //updateCubePosition();
        }
        else
        {
            updateCubePosition();
            if (rotationFollowMode != CaveFollowRotationMode.OFF)
                updateCubeRotation();
            else if (rotationFollowMode == CaveFollowRotationMode.OFF && !caveCubeObj.transform.eulerAngles.Equals(_startRotation)) //se durante la playmode disattivo la followRotation, ritorna alla rotazione originale
                caveCubeObj.transform.eulerAngles = _startRotation;
        }
        if (!caveCubeObj.transform.localScale.Equals(_scale))
        {
            //update the cube scale
            caveCubeObj.transform.localScale = _scale;
        }

        caveCubeObj.GetComponent<MeshRenderer>().material.SetFloat("_Thickness", gridLineThickness);
    }

    private void OnDisable()
    {
        if(caveCubeObj!=null)
            caveCubeObj.SetActive(false); //disable the cube
    }
    private void OnEnable()
    {
        if (caveCubeObj != null)
        {
            caveCubeObj.SetActive(true); //enable the cube
        }

    }

    void updateCubePosition()
    {
        float height = 0f;
        _playerPos = player.transform.position;
        height = _playerPos.y + offset; // offset to match the ground
        _playerPos.y = height;
        caveCubeObj.transform.position = _playerPos;
    }
    Vector3 rotation;
    void updateCubeRotation()
    {
        if (rotation == null)
            rotation = new Vector3();

        rotation.y = player.transform.eulerAngles.y;
        if (rotationFollowMode == CaveFollowRotationMode.Y_AXIS) {
            rotation.x = 0;
            rotation.z = 0;
            caveCubeObj.transform.eulerAngles = rotation;
        }
        else if(rotationFollowMode == CaveFollowRotationMode.ALL_AXIS)
        {
            caveCubeObj.transform.eulerAngles = player.transform.eulerAngles;
        }
        
        
    }

}

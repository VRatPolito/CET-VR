using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class CharacterControllerVR : MonoBehaviour {
    
    bool _initialized = false;
    public bool Blocked = false;
    Vector3 _startCenterEyeLocalPos = Vector3.zero;
    Vector3 _prevCenterEyeLocalPos;
    [Tooltip("The SteamVR CameraRig object's Transform")]
    public Transform CameraRig;
    [Tooltip("The SteamVR CameraEye object's Transform")]
    public Transform CameraEye;
    [SerializeField]
    [Tooltip("The CharacterController component attached to the Player")]
    private CharacterController _playerCollider;
    [SerializeField]
    bool _setCustomGravity = false;
    [SerializeField]
    private Vector3 _customGravity = new Vector3(0, -9.81f, 0);
    private Vector3 _defaultGravity;
    //Queue<Vector3> _externalMotion = new Queue<Vector3>();
    public Vector3 center => _playerCollider.center;
    public float height => _playerCollider.height;
    public float radius => _playerCollider.radius;
    public float slopeLimit => _playerCollider.slopeLimit;
    public float stepOffset => _playerCollider.stepOffset;
    public bool isGrounded => _playerCollider.isGrounded;

    private void OnEnable()
    {
        Initialize();
        if (_setCustomGravity)
        {
            _defaultGravity = Physics.gravity;
            Physics.gravity = _customGravity;
        }
    }

    private void OnDisable()
    {
        _initialized = false;
        if (_setCustomGravity)
            Physics.gravity = Physics.gravity;
    }

    public void Initialize()
    {
        if (!_initialized)
        {
            //Save the initial VRNode.CenterEye localPosition
            InputDevices.GetDeviceAtXRNode(XRNode.CenterEye).TryGetFeatureValue(CommonUsages.devicePosition, out _startCenterEyeLocalPos);
            //Move the CameraRig in order to keep the the player centered
            CameraRig.localPosition = new Vector3(-_startCenterEyeLocalPos.x, CameraRig.localPosition.y, -_startCenterEyeLocalPos.z);
            
            _initialized = true;
        }
    }

    void Update()
    {
        if (!_initialized)
            Initialize();

        if (!Blocked)
        {
            //Get the actual VRNode.CenterEye localPosition
            Vector3 NewCenterEyeLocalPos;
            InputDevices.GetDeviceAtXRNode(XRNode.CenterEye).TryGetFeatureValue(CommonUsages.devicePosition, out NewCenterEyeLocalPos);
            //If I already set the CameraEye localPosition once and it was at the same localPosition

            float x, y, z;

            CheckBoundaries(NewCenterEyeLocalPos, out x, out y, out z);

            CameraRig.localPosition = new Vector3(x, y, z);

            var moveDirection = transform.TransformDirection(NewCenterEyeLocalPos - _prevCenterEyeLocalPos);
      
            moveDirection += Physics.gravity * Time.deltaTime;
            _playerCollider.Move(moveDirection);

            _prevCenterEyeLocalPos = NewCenterEyeLocalPos;
        }
    }
    void CheckBoundaries(Vector3 NewCenterEyeLocalPos,out float x, out float y, out float z)
    {
        x = y = z = 0;
        //Generate the Vector2(x, z) versions of StartCenterEyeLocalPos and NewCenterEyeLocalPos for the circular limit check
        Vector2 StartCenterEyeLocalPosXZ = new Vector2(_startCenterEyeLocalPos.x, _startCenterEyeLocalPos.z);
        var NewCenterEyeLocalPosXZ = new Vector2(NewCenterEyeLocalPos.x, NewCenterEyeLocalPos.z);


        //Calculate the offsets between the actual and the initial VRNode.CenterEye localPositions
        //Calculate the vector useful to generate the max radial position in case of outofbounds
        var newPosXZ = (NewCenterEyeLocalPosXZ - StartCenterEyeLocalPosXZ).normalized;
        var offsx = NewCenterEyeLocalPos.x - _startCenterEyeLocalPos.x;
        var offsz = NewCenterEyeLocalPos.z - _startCenterEyeLocalPos.z;
        var offsy = NewCenterEyeLocalPos.y - _startCenterEyeLocalPos.y;
        
        newPosXZ *= 0;
        
        x = -_startCenterEyeLocalPos.x - offsx + newPosXZ.x;
        y = CameraRig.localPosition.y;
        z = -_startCenterEyeLocalPos.z - offsz + newPosXZ.y;        
    }

    internal void Move(Vector3 moveDirection)
    {
        if (gameObject.activeSelf && enabled)
            _playerCollider.Move(moveDirection);
    }
}

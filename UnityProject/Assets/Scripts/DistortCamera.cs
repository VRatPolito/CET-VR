using UnityEngine;
using UnityEngine.InputSystem;

public class DistortCamera : MonoBehaviour
{
    //Coroutine _camRoutine;
    float _currVelocity;
    [SerializeField]
    float _smoothTime = 0.01f;
    Vector2 _newShift;
    [SerializeField]
    float _vFOV = 110;
    [SerializeField]
    float _resHeight = 1080;
    [SerializeField]
    float _resWidth = 1020;
    float _hFOV;
    float _vAngle;
    float _hAngle;
    Quaternion _initialRot;
    Vector3 _initialPos;
    [SerializeField]
    Transform _hMD;
    [SerializeField]
    Camera cam;
    Transform _rotator;
    // Start is called before the first frame update
    void Start()
    {
        _vAngle = 0;
        _hAngle = 0;
        _rotator = transform.Find("Rotator");
        cam.usePhysicalProperties = true;
        _newShift = cam.lensShift;
        if (!ExampleUtil.isPresent())
        {
            _hMD.localPosition = Vector3.up;
            cam.fieldOfView = _vFOV;
            _hFOV = Camera.VerticalToHorizontalFieldOfView(_vFOV, cam.aspect);
        }
        else
            _hFOV = Camera.VerticalToHorizontalFieldOfView(_vFOV, _resWidth / _resHeight);
        transform.localPosition = _hMD.position;
        transform.localRotation = _hMD.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        _rotator.localPosition = Vector3.zero;
        _rotator.localRotation = Quaternion.identity;

        if (Keyboard.current[Key.UpArrow].wasPressedThisFrame)
        {
            //_newShift.y = Mathf.SmoothDamp(cam.lensShift.y, -1, ref _currVelocity, _smoothTime);
            _vAngle = Mathf.SmoothDamp(_vAngle, _vFOV / 2, ref _currVelocity, _smoothTime);

        }
        else if (Keyboard.current[Key.DownArrow].wasPressedThisFrame)
        {
            //_newShift.y = Mathf.SmoothDamp(cam.lensShift.y, 1, ref _currVelocity, _smoothTime);
            _vAngle = Mathf.SmoothDamp(_vAngle, -_vFOV / 2, ref _currVelocity, _smoothTime);
        }
        else if (/*_newShift.y*/_vAngle > 0.001f ||/*_newShift.y*/_vAngle < -0.001f)
        {
            //_newShift.y = Mathf.SmoothDamp(cam.lensShift.y, 0, ref _currVelocity, _smoothTime);
            _vAngle = Mathf.SmoothDamp(_vAngle, 0, ref _currVelocity, _smoothTime);

        }
        else
        {
            //_newShift.y = 0;
            _vAngle = 0;
        }

        if (Keyboard.current[Key.LeftArrow].wasPressedThisFrame)
        {
            //_newShift.x = Mathf.SmoothDamp(cam.lensShift.x, 1, ref _currVelocity, _smoothTime);
            _hAngle = Mathf.SmoothDamp(_hAngle, _hFOV / 2, ref _currVelocity, _smoothTime);
        }
        else if (Keyboard.current[Key.RightArrow].wasPressedThisFrame)
        {
            //_newShift.x = Mathf.SmoothDamp(cam.lensShift.x, -1, ref _currVelocity, _smoothTime);
            _hAngle = Mathf.SmoothDamp(_hAngle, -_hFOV / 2, ref _currVelocity, _smoothTime);

        }
        else if (/*_newShift.x*/_hAngle > 0.001f || /*_newShift.x*/_hAngle < -0.001f)
        {
            //_newShift.x = Mathf.SmoothDamp(cam.lensShift.x, 0, ref _currVelocity, _smoothTime);
            _hAngle = Mathf.SmoothDamp(_hAngle, 0, ref _currVelocity, _smoothTime);

        }
        else
        {
            //_newShift.x = 0;
            _hAngle = 0;
        }
        
        _newShift = new Vector2(RotToShift(_hAngle, _hFOV, 0.5f), RotToShift(-_vAngle, _vFOV, 0.5f));

        /*if (_camRoutine == null)
            _camRoutine = StartCoroutine(ModifyCamera());*/
    }
    private void LateUpdate()
    {
        _rotator.RotateAround(_hMD.position, -cam.transform.up, _hAngle);
        _rotator.RotateAround(_hMD.position, -cam.transform.right, _vAngle);
        cam.lensShift = _newShift;
    }

    /*IEnumerator ModifyCamera()
    {
        yield return new WaitForEndOfFrame();
        cam.lensShift = _newShift;
        _camRoutine = null;
    }*/

    float RotToShift(float angle, float max, float size)
    {
        return Mathf.Lerp(-size, size, Mathf.InverseLerp(-max/2, max/2, angle));
    }
}

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

/*
 * Rotation entirely managed by the non-dominant Joystick/Controller. The dominant controller is used for the shooting mechanism
 */
public class PlatformManager : MonoBehaviour
{
    InputManagement _input;
    public bool Blocked
    {
        get { return _blocked; }
        set
        {
            _blocked = value;
        }
    }
    bool _blocked = false;

    #region platform settings
    TowerDefenseLevelManager _manager;
    [SerializeField]
    GameObject _shootingPlatform;
    
    [SerializeField]
    float minPlatformRotSpeed = 20;
    [SerializeField]
    float maxPlatformRotSpeed = 360;
    [SerializeField]
    float _padDeadZone = 0.1f;
    #endregion

    #region player settings
    [SerializeField]
    GameObject _playerCamera;
    [SerializeField]
    MeshCollider _platformWalls;
    [SerializeField]
    BoxCollider _platformFloor;
    bool isPlayerLookingForward;
    [SerializeField]
    GameObject visualSuggestion; //suggestions for the user regarding the shooting
    [SerializeField]
    TextMeshProUGUI suggestionText;
    [SerializeField]
    UnityEngine.UI.Image suggestionBackground;
    [SerializeField]
    UnityEngine.UI.Image arrow;
    [SerializeField]
    Sprite arrowLeft, arrowRight, arrowDown;
    [SerializeField]
    Transform _platformWallHole;
    #endregion

    float smoothStartRot;
    [SerializeField]
    float platformRotSmoothTime = 0.5f; //time to bring the rotation from 0 to the desired speed
    float previewAxisValueRot = 0; //valore al frame precedente
    bool rotatedLastFrame = false;
    [SerializeField]
    bool _useBothHands = true;
    #region controller settings
    Coroutine _visualSuggRoutine;
    void Awake()
    {
        _input = GetComponent<InputManagement>();
        if (GameManager.Instance != null)
            _manager = (TowerDefenseLevelManager)GameManager.Instance.LevelManager;
        else
            _manager = GameObject.FindObjectOfType<TowerDefenseLevelManager>();
    }
    #endregion

    internal void Reset()
    {
        visualSuggestion.SetActive(false);
        _visualSuggRoutine = null;
        rotatedLastFrame = false;
        _shootingPlatform.transform.rotation = Quaternion.Euler(Vector3.zero);
    }


    void Update()
    {
        if (!Blocked)
        {
            if (_manager.GetComponent<TowerDefenseLevelManager>().rotationConstrained)
                checkLookingDirection();
            else
                isPlayerLookingForward = true;
            movePlatform();
        }
    }


    //checks if the player is looking at a wall or if he is looking in the front direction on the platform
private void checkLookingDirection()
    {
        isPlayerLookingForward = true;
        Vector3 hitPoint = Vector3.zero;
        var hits = Physics.RaycastAll(new Ray(_playerCamera.transform.position, _playerCamera.transform.forward));
        
        foreach (var hit in hits)
        {
            if (hit.collider == _platformWalls || hit.collider == _platformFloor)
            {
                isPlayerLookingForward = false;
                hitPoint = hit.point;
                break;
            }
        }

        //rotate the suggestion arrow in the correct direction, pointing at the hole in the wall and then the forward direction
        if (!isPlayerLookingForward)
        {
            Vector3 platform2rotator_vector = _platformFloor.gameObject.transform.position - _platformWallHole.gameObject.transform.position;
            //platform2rotator_vector.y = 0;

            Vector3 platform2hitpoint_vector = _platformFloor.gameObject.transform.position - hitPoint;
            //platform2hitpoint_vector.y = 0;

            float orizzontal_angle = Vector3.SignedAngle(platform2rotator_vector, platform2hitpoint_vector, Vector3.up);

            if (hitPoint.y >= _platformFloor.gameObject.transform.position.y + 3.25f)
            {
                arrow.sprite = arrowDown;
            }
            else if (orizzontal_angle > 0)
            {
                //left arrow
                arrow.sprite = arrowLeft;
            }
            else if (orizzontal_angle <= 0)
            {
                arrow.sprite = arrowRight;
                //right arrow
            }
        }
        else
        {
            visualSuggestion.SetActive(false);
            _visualSuggRoutine = null;
        }
    }

    private void movePlatform()
    {

        #region right handed
        if (GameManager.Instance != null && !GameManager.Instance._isLeftHanded)
        {
            if ((_input._axisMode == InputMode.Touch && _input.IsLeftPadTouched) || (_input._axisMode == InputMode.Click && _input.IsLeftPadPressed))
            {
                //if the player is looking in the forward direction (hole in the walls in the platform), or if this limit is never imposed, rotate
                if (isPlayerLookingForward)
                {
                    #region rotation w/ leftpad
                    if (_input.LeftPadAxis.padX > _padDeadZone) //rotate to the right
                    {
                        Vector3 rot = _shootingPlatform.transform.rotation.eulerAngles;

                        if (previewAxisValueRot < _padDeadZone || !rotatedLastFrame) //first press of rotation to the right
                        {
                            smoothStartRot = Time.time;

                            previewAxisValueRot = _input.LeftPadAxis.padX;
                        }
                        float smoothMultiplier = Mathf.InverseLerp(smoothStartRot, smoothStartRot + platformRotSmoothTime, Time.time);
                        smoothMultiplier = Mathf.Clamp(smoothMultiplier, 0f, 1f);

                        //all fixed, rotate y axis (vertical)

                        rot.y += getRotSpeedLerp(_input.LeftPadAxis.padX) * smoothMultiplier * Time.deltaTime;

                        _shootingPlatform.transform.rotation = Quaternion.Euler(rot);

                        rotatedLastFrame = true;

                    }
                    else if (_input.LeftPadAxis.padX < -_padDeadZone)//rotate to the left
                    {
                        Vector3 rot = _shootingPlatform.transform.rotation.eulerAngles;

                        if (previewAxisValueRot > -_padDeadZone || !rotatedLastFrame) //first press of rotation to the left
                        {
                            smoothStartRot = Time.time;

                            previewAxisValueRot = _input.LeftPadAxis.padX;
                        }

                        float smoothMultiplier = Mathf.InverseLerp(smoothStartRot, smoothStartRot + platformRotSmoothTime, Time.time);
                        smoothMultiplier = Mathf.Clamp(smoothMultiplier, 0f, 1f);
                        //all fixed, rotate y axis(vertical)

                        rot.y -= getRotSpeedLerp(_input.LeftPadAxis.padX) * smoothMultiplier * Time.deltaTime;
                        _shootingPlatform.transform.rotation = Quaternion.Euler(rot);
                        rotatedLastFrame = true;
                    }
                    #endregion
                }
                else
                {
                    rotatedLastFrame = false;
                    if (_input.LeftPadAxis.padX > _padDeadZone || _input.LeftPadAxis.padX < -_padDeadZone)
                    {
                        if (_visualSuggRoutine == null)
                            _visualSuggRoutine = StartCoroutine(visSuggestion());
                    }
                    else
                    {
                        visualSuggestion.SetActive(false);
                        _visualSuggRoutine = null;
                    }
                }
            }
            else
            {
                visualSuggestion.SetActive(false);
                _visualSuggRoutine = null;
                rotatedLastFrame = false;
            }
        }
        #endregion
        #region left handed
        else
        {
            if ((_input._axisMode == InputMode.Touch && _input.IsRightPadTouched) || (_input._axisMode == InputMode.Click && _input.IsRightPadPressed))
            {
                //if the player is looking in the forward direction (hole in the walls in the platform), or if this limit is never imposed, rotate
                if (isPlayerLookingForward)
                {
                    #region rotation w/ rightpad
                    if (_input.RightPadAxis.padX > _padDeadZone) //rotate to the right
                    {
                        Vector3 rot = _shootingPlatform.transform.rotation.eulerAngles;

                        if (previewAxisValueRot < _padDeadZone || !rotatedLastFrame) //first press of rotation to the right
                        {
                            smoothStartRot = Time.time;

                            previewAxisValueRot = _input.RightPadAxis.padX;
                        }
                        float smoothMultiplier = Mathf.InverseLerp(smoothStartRot, smoothStartRot + platformRotSmoothTime, Time.time);
                        smoothMultiplier = Mathf.Clamp(smoothMultiplier, 0f, 1f);
                        //all fixed, rotate y axis(vertical)

                        rot.y += getRotSpeedLerp(_input.RightPadAxis.padX) * smoothMultiplier * Time.deltaTime;

                        _shootingPlatform.transform.rotation = Quaternion.Euler(rot);
                        rotatedLastFrame = true;

                    }
                    else if (_input.RightPadAxis.padX < -_padDeadZone)//rotate to the left
                    {
                        Vector3 rot = _shootingPlatform.transform.rotation.eulerAngles;
                        if (previewAxisValueRot > -_padDeadZone || !rotatedLastFrame) //first press of rotation to the right
                        {
                            smoothStartRot = Time.time;

                            previewAxisValueRot = _input.RightPadAxis.padX;
                        }
                        float smoothMultiplier = Mathf.InverseLerp(smoothStartRot, smoothStartRot + platformRotSmoothTime, Time.time);
                        smoothMultiplier = Mathf.Clamp(smoothMultiplier, 0f, 1f);
                        //all fixed, rotate y axis(vertical)

                        rot.y -= getRotSpeedLerp(_input.RightPadAxis.padX) * smoothMultiplier * Time.deltaTime;

                        _shootingPlatform.transform.rotation = Quaternion.Euler(rot);
                        rotatedLastFrame = true;
                    }
                    #endregion
                }
                else
                {
                    rotatedLastFrame = false;
                    if (_input.RightPadAxis.padX > _padDeadZone || _input.RightPadAxis.padX < -_padDeadZone)
                    {
                        if (_visualSuggRoutine == null)
                            _visualSuggRoutine = StartCoroutine(visSuggestion());
                    }
                    else
                    {
                        visualSuggestion.SetActive(false);
                        _visualSuggRoutine = null;
                    }
                }
            }
            else
            {
                visualSuggestion.SetActive(false);
                _visualSuggRoutine = null;
                rotatedLastFrame = false;
            }
            #endregion
        }
    }
    IEnumerator visSuggestion()
    {
        Color col, colI, colA;

        colI = suggestionBackground.color;
        col = suggestionText.color;
        colA = arrow.material.color;

        if (!visualSuggestion.activeSelf)
        {
            visualSuggestion.SetActive(true);
            col.a = 1f;
            colI.a = 0.4f;
            colA.a = 1f;
            suggestionText.color = col;
            suggestionBackground.color = colI;
            arrow.material.color = colA;

            while (suggestionText.color.a > 0 && !isPlayerLookingForward)
            {
                yield return new WaitForSeconds(0.2f);
                col.a -= 0.1f;
                colI.a -= 0.04f;
                colA.a -= 0.1f;
                suggestionText.color = col;
                suggestionBackground.color = colI;
                arrow.material.color = colA;
            }
            visualSuggestion.SetActive(false);
        }

        if (isPlayerLookingForward)
            {
                col.a = 0;
                colI.a = 0;
                colA.a = 0;
                suggestionText.color = col;
                suggestionBackground.color = colI;
                arrow.material.color = colA;
                visualSuggestion.SetActive(false);
            }
    }

    /*calculates the rotation speed based on interpolation between min and max, based on the axis returned by the trackpad/thumbstick*/
    float getRotSpeedLerp(float axisValue)
    {
        float val = Mathf.Abs(axisValue);
        float speed;

        float perc = Mathf.InverseLerp(0.2f, 1f, val);
        speed = Mathf.Lerp(minPlatformRotSpeed, maxPlatformRotSpeed, perc);


        return speed;

    }
    
}

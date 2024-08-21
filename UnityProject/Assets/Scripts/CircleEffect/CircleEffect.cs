using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CircleEffect : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject player;
    public Material circleEffectMaterial; //create material from shader and attatch here
    public Camera mainCamera;
    public Camera sceneCaptureCamera;

    //update rate of the peripheral FOV (in seconds)
    [Range(0.0f, 10.0f)]
    public float updateRate;

    public float decaySpeed = 20f;

    public float minViewValue = .5f;
    public float maxViewValue = 1.5f;

    Vector3 lastPosition;
    Vector3 lastOrientation;

    Rigidbody rigidbody;
    bool usingRigidbody;

    public float decayRate = -.001f;
    public float angluarSpeedModifier = .1f;
    public float translationSpeedModifier = .1f;
    public float testRad = .5f;


    Vector3 angularVelocity;
    Vector3 translationalVelocity;

    float checkTime = .1f;

    public bool usePlayer = false;

    IEnumerator TrackVelocities()
    {
        Vector3 lastRotation = new Vector3(0, 0, 0);
        Vector3 lastPosition = usePlayer? player.transform.position : transform.position;
        while (true)
        {
            Vector3 rot = usePlayer ? player.transform.eulerAngles : transform.eulerAngles;
            Vector3 rotationDelta = rot - lastRotation;
            lastRotation = usePlayer ? player.transform.eulerAngles : transform.eulerAngles;

            Vector3 pos = usePlayer ? player.transform.position : transform.position;
            Vector3 translationDelta = pos - lastPosition;
            lastPosition = usePlayer ? player.transform.position : transform.position;



            angularVelocity = rotationDelta / checkTime;
            translationalVelocity = translationDelta / checkTime;

            yield return new WaitForSeconds(checkTime);
        }
    }

    private void Awake()
    {
        updateRate = 5f; //default value
        //InvokeRepeating("updateMaterial", 0.0f, updateRate); //update the peripheral FOV every updateRate seconds
    }

    void Start()
    {
        if (player == null)
        {
            player = this.gameObject;
        }
        if (circleEffectMaterial == null)
        {
            circleEffectMaterial = Resources.Load("GingerVR-master/SicknessReductionTechniques/Materials/CircleEffectMat") as Material;
        }

        lastPosition = player.transform.position;
        lastOrientation = player.transform.eulerAngles;

        rigidbody = player.GetComponent<Rigidbody>();
        if (rigidbody == null)
        {
            usingRigidbody = false;
            StartCoroutine(TrackVelocities());
        }
        else
        {
            usingRigidbody = true;
        }
        setUpCaptureCamera();
        
        InvokeRepeating("updateMaterial", 0.0f, updateRate); //update the peripheral FOV every updateRate seconds
    }
    


    public float viewRadius = 0f; //0 to .5
    private float nextActionTime = 0.0f;

    // Update is called once per frame
    void Update()
    {
        float translationalSpeed;
        float rotationalSpeed;


        translationalSpeed = translationalVelocity.magnitude;
        rotationalSpeed = angularVelocity.magnitude;


        float cRate = ((translationalSpeed - decaySpeed) * translationSpeedModifier) + (rotationalSpeed * angluarSpeedModifier);



        if (translationalSpeed <= decaySpeed)
        {
            viewRadius -= decayRate;


        }
        else
        {
            viewRadius -= cRate;

        }
        if (viewRadius > maxViewValue)
        {
            viewRadius = maxViewValue;

        }
        if (viewRadius < minViewValue)
        {
            viewRadius = minViewValue;
        }

        circleEffectMaterial.SetFloat("_viewRadius", viewRadius);


        lastPosition = player.transform.position;

    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {

        RenderTexture renderTexture = RenderTexture.GetTemporary(src.width, src.height);

        Graphics.Blit(src, renderTexture); //copies source texture to destination texture

        //apply the render texture as many iterations as specified

        RenderTexture tempTexture = RenderTexture.GetTemporary(src.width, src.height); //creates a quick temporary texture for calculations
        Graphics.Blit(renderTexture, tempTexture, circleEffectMaterial);
        RenderTexture.ReleaseTemporary(renderTexture); //releases the temporary texture we got from GetTemporary 
        renderTexture = tempTexture;

        Graphics.Blit(renderTexture, dst);
        RenderTexture.ReleaseTemporary(renderTexture);
    }

    //render a frame every "updateRate" seconds
    void updateMaterial()
    {
        sceneCaptureCamera.gameObject.SetActive(true);
        sceneCaptureCamera.transform.rotation = mainCamera.transform.rotation;
        sceneCaptureCamera.gameObject.SetActive(false);
        sceneCaptureCamera.Render();
        circleEffectMaterial.SetTexture("_SecondaryTexture", sceneCaptureCamera.activeTexture);
        //Debug.Log(nextActionTime + " Updated\n");
        
    }

    //to make the two cameras consistent in position
    void setUpCaptureCamera()
    {
        sceneCaptureCamera.transform.position = mainCamera.transform.position;
        sceneCaptureCamera.aspect = mainCamera.aspect; //same aspect ratio for both cams
        sceneCaptureCamera.fieldOfView = mainCamera.fieldOfView;
    }
}

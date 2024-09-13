#define RENDER_PER_FRAME
using UnityEngine;
using System.Collections;
using UnityStandardAssets.Water;
using System;
using System.Collections.Generic;
//using Valve.VR;

[ExecuteInEditMode]
[RequireComponent(typeof(WaterBase))]
public class PlanarReflectionStereo : MonoBehaviour
{
    public float m_ReflectionTextureScale = 1.0f;
    [SerializeField]
    [Tooltip("Mirror will use up to the anti-aliasing level specified in QualitySettings, without exceeding this value (1, 2, 4, or 8)")]
    private int maxAntiAliasing = 1;
    private int antiAliasing = 1;
    private class ReflectionData
    {
        public RenderTexture texture;
        public MaterialPropertyBlock propertyBlock;
    }
    private Dictionary<Camera, ReflectionData> m_Reflections = new Dictionary<Camera, ReflectionData>(); // Camera -> ReflectionData table

    public Camera m_CameraRigEye;
    public LayerMask reflectionMask;
    public bool reflectSkybox = false;
    public Color clearColor = Color.grey;
    public String reflectionSampler = "_ReflectionTex";
    public float clipPlaneOffset = 0.07F;

    Vector3 m_Oldpos;
    Camera m_ReflectionCamera;
    Material m_SharedMaterial;
    Dictionary<Camera, bool> m_HelperCameras;

    //private static bool s_InsideRendering = false;
    //private static int TexturePropertyID;

    private static readonly Rect LeftEyeRect = new Rect(0.0f, 0.0f, 0.5f, 1.0f);
    private static readonly Rect RightEyeRect = new Rect(0.5f, 0.0f, 0.5f, 1.0f);
    private static readonly Rect DefaultRect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);

    public void Start()
    {
        m_SharedMaterial = ((WaterBase)gameObject.GetComponent(typeof(WaterBase))).sharedMaterial;
    }

    private void OnValidate()
    {
        // Enfore only valid values for maxAntiAliasing
        if (maxAntiAliasing != 1 && maxAntiAliasing != 2 && maxAntiAliasing != 4 && maxAntiAliasing != 8)
        {
            maxAntiAliasing = 1;
        }

        antiAliasing = Mathf.Min(QualitySettings.antiAliasing, maxAntiAliasing);

        // Apparently when anti-aliasing is turned off in the quality settings, the value is 0 rather than 1 as expected... :(
        antiAliasing = Mathf.Max(1, antiAliasing);
    }


    Camera CreateReflectionCameraFor(Camera cam)
    {
        String reflName = gameObject.name + "Reflection" + cam.name;
        GameObject go = GameObject.Find(reflName);

        if (!go)
        {
            go = new GameObject(reflName, typeof(Camera));
        }
        if (!go.GetComponent(typeof(Camera)))
        {
            go.AddComponent(typeof(Camera));
        }
        Camera reflectCamera = go.GetComponent<Camera>();

        reflectCamera.backgroundColor = clearColor;
        reflectCamera.clearFlags = reflectSkybox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;

        SetStandardCameraParameter(reflectCamera, reflectionMask);

        if (!reflectCamera.targetTexture)
        {
            //reflectCamera.targetTexture = CreateTextureFor(cam);
            reflectCamera.targetTexture = CreateTextureFor(m_CameraRigEye);
            //reflectCamera.targetTexture.antiAliasing = antiAliasing;

            //Valve.VR.
        }

        return reflectCamera;
    }


    void SetStandardCameraParameter(Camera cam, LayerMask mask)
    {
        cam.cullingMask = mask & ~(1 << LayerMask.NameToLayer("Water"));
        cam.backgroundColor = Color.black;
        cam.enabled = false;
    }


    RenderTexture CreateTextureFor(Camera cam)
    {

        Debug.Log("cam: " + cam.pixelWidth + ", " + cam.pixelHeight + ", name: " + cam.name);
        Debug.Log("vr scale: " + UnityEngine.XR.XRSettings.eyeTextureResolutionScale + ", vr eye texture width: " + UnityEngine.XR.XRSettings.eyeTextureWidth + ", vr eye texture height: " + UnityEngine.XR.XRSettings.eyeTextureHeight);
        Debug.Log("vr render viewport scale" + UnityEngine.XR.XRSettings.renderViewportScale);
        Debug.Log("screen: " + Screen.width + ", " + Screen.height + ", sceneResolutionScale: " + SteamVR_Camera.sceneResolutionScale);
        RenderTexture rt = new RenderTexture(Mathf.FloorToInt(cam.pixelWidth * m_ReflectionTextureScale),
            Mathf.FloorToInt(cam.pixelHeight * m_ReflectionTextureScale), 24);
        rt.hideFlags = HideFlags.DontSave;
        return rt;
    }


    public void RenderHelpCameras(Camera currentCam)
    {
        if (null == m_HelperCameras)
        {
            m_HelperCameras = new Dictionary<Camera, bool>();
        }

        if (!m_HelperCameras.ContainsKey(currentCam))
        {
            m_HelperCameras.Add(currentCam, false);
        }
        if (m_HelperCameras[currentCam])
        {
            return;
        }

        if (!m_ReflectionCamera)
        {
            m_ReflectionCamera = CreateReflectionCameraFor(currentCam);
        }

        RenderReflectionFor(currentCam, m_ReflectionCamera);

        m_HelperCameras[currentCam] = true;
    }


    public void LateUpdate()
    {
        if (null != m_HelperCameras)
        {
            m_HelperCameras.Clear();
        }
    }

    public void WaterTileBeingRendered(Transform tr, Camera currentCam)
    {

#if RENDER_PER_FRAME
        RenderHelpCameras(currentCam);
#endif
        if (m_ReflectionCamera && m_SharedMaterial)
        {
            m_SharedMaterial.SetTexture(reflectionSampler, m_ReflectionCamera.targetTexture);
        }
    }

    void RenderMirror(RenderTexture targetTexture, Vector3 camPosition, Quaternion camRotation, Matrix4x4 camProjectionMatrix, Rect camViewport)
    {
        // Copy camera position/rotation/projection data into the reflectionCamera
        m_ReflectionCamera.ResetWorldToCameraMatrix();
        m_ReflectionCamera.transform.position = camPosition;
        m_ReflectionCamera.transform.rotation = camRotation;
        m_ReflectionCamera.projectionMatrix = camProjectionMatrix;
        m_ReflectionCamera.targetTexture = targetTexture;
        m_ReflectionCamera.rect = camViewport;

        // find out the reflection plane: position and normal in world space
        Vector3 pos = transform.position;
        Vector3 normal = transform.up;

        // Reflect camera around reflection plane
        Vector4 worldSpaceClipPlane = Plane(pos, normal);
        m_ReflectionCamera.worldToCameraMatrix *= CalculateReflectionMatrix(worldSpaceClipPlane);

        // Setup oblique projection matrix so that near plane is our reflection
        // plane. This way we clip everything behind it for free.
        Vector4 cameraSpaceClipPlane = CameraSpacePlane(m_ReflectionCamera, pos, normal);
        m_ReflectionCamera.projectionMatrix = m_ReflectionCamera.CalculateObliqueMatrix(cameraSpaceClipPlane);

        // Set camera position and rotation (even though it will be ignored by the render pass because we
        // have explicitly set the worldToCameraMatrix). We do this because some render effects may rely 
        // on the position/rotation of the camera.
        m_ReflectionCamera.transform.position = m_ReflectionCamera.cameraToWorldMatrix.GetPosition();
        m_ReflectionCamera.transform.rotation = m_ReflectionCamera.cameraToWorldMatrix.GetRotation();

        bool oldInvertCulling = GL.invertCulling;
        GL.invertCulling = !oldInvertCulling;
        m_ReflectionCamera.Render();
        GL.invertCulling = oldInvertCulling;
    }

    void RenderMirror(Vector3 camPosition, Quaternion camRotation, Matrix4x4 camProjectionMatrix, Rect camViewport)
    {
        // Copy camera position/rotation/projection data into the reflectionCamera
        m_ReflectionCamera.ResetWorldToCameraMatrix();
        m_ReflectionCamera.transform.position = camPosition;
        m_ReflectionCamera.transform.rotation = camRotation;
        m_ReflectionCamera.projectionMatrix = camProjectionMatrix;
        m_ReflectionCamera.rect = camViewport;

        // find out the reflection plane: position and normal in world space
        Vector3 pos = transform.position;
        Vector3 normal = transform.up;

        // Reflect camera around reflection plane
        Vector4 worldSpaceClipPlane = Plane(pos, normal);
        m_ReflectionCamera.worldToCameraMatrix *= CalculateReflectionMatrix(worldSpaceClipPlane);

        // Setup oblique projection matrix so that near plane is our reflection
        // plane. This way we clip everything behind it for free.
        Vector4 cameraSpaceClipPlane = CameraSpacePlane(m_ReflectionCamera, pos, normal);
        m_ReflectionCamera.projectionMatrix = m_ReflectionCamera.CalculateObliqueMatrix(cameraSpaceClipPlane);

        // Set camera position and rotation (even though it will be ignored by the render pass because we
        // have explicitly set the worldToCameraMatrix). We do this because some render effects may rely 
        // on the position/rotation of the camera.
        m_ReflectionCamera.transform.position = m_ReflectionCamera.cameraToWorldMatrix.GetPosition();
        m_ReflectionCamera.transform.rotation = m_ReflectionCamera.cameraToWorldMatrix.GetRotation();

        //bool oldInvertCulling = GL.invertCulling;
        //GL.invertCulling = !oldInvertCulling;
        m_ReflectionCamera.Render();
        //GL.invertCulling = oldInvertCulling;
    }

    private static Matrix4x4 GetSteamVRProjectionMatrix(Camera cam, Valve.VR.EVREye eye)
    {
        //steam vr plugin 1.1.1
        Valve.VR.HmdMatrix44_t proj = SteamVR.instance.hmd.GetProjectionMatrix(eye, cam.nearClipPlane, cam.farClipPlane);
        
        //steam vr plugin 1.2
        //Valve.VR.HmdMatrix44_t proj = SteamVR.instance.hmd.GetProjectionMatrix(eye, cam.nearClipPlane, cam.farClipPlane);
        Matrix4x4 m = new Matrix4x4();
        m.m00 = proj.m0;
        m.m01 = proj.m1;
        m.m02 = proj.m2;
        m.m03 = proj.m3;
        m.m10 = proj.m4;
        m.m11 = proj.m5;
        m.m12 = proj.m6;
        m.m13 = proj.m7;
        m.m20 = proj.m8;
        m.m21 = proj.m9;
        m.m22 = proj.m10;
        m.m23 = proj.m11;
        m.m30 = proj.m12;
        m.m31 = proj.m13;
        m.m32 = proj.m14;
        m.m33 = proj.m15;
        return m;
    }

    // Given position/normal of the plane, calculates plane in camera space.
    private static Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal)
    {
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(pos);
        Vector3 cnormal = m.MultiplyVector(normal).normalized;
        return Plane(cpos, cnormal);
    }

    private static Vector4 Plane(Vector3 pos, Vector3 normal)
    {
        return new Vector4(normal.x, normal.y, normal.z, -Vector3.Dot(pos, normal));
    }

    public void OnEnable()
    {
        Shader.EnableKeyword("WATER_REFLECTIVE");
        Shader.DisableKeyword("WATER_SIMPLE");
    }


    public void OnDisable()
    {
        Shader.EnableKeyword("WATER_SIMPLE");
        Shader.DisableKeyword("WATER_REFLECTIVE");
    }


    void RenderReflectionFor(Camera cam, Camera reflectCamera)
    {
        if (!reflectCamera)
        {
            return;
        }

        if (m_SharedMaterial && !m_SharedMaterial.HasProperty(reflectionSampler))
        {
            return;
        }

        reflectCamera.cullingMask = reflectionMask & ~(1 << LayerMask.NameToLayer("Water"));

        SaneCameraSettings(reflectCamera);

        reflectCamera.backgroundColor = clearColor;
        reflectCamera.clearFlags = reflectSkybox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
        if (reflectSkybox)
        {
            if (cam.gameObject.GetComponent(typeof(Skybox)))
            {
                Skybox sb = (Skybox)reflectCamera.gameObject.GetComponent(typeof(Skybox));
                if (!sb)
                {
                    sb = (Skybox)reflectCamera.gameObject.AddComponent(typeof(Skybox));
                }
                sb.material = ((Skybox)cam.GetComponent(typeof(Skybox))).material;
            }
        }

        GL.invertCulling = true;
        /*
        Transform reflectiveSurface = transform; //waterHeight;

        Vector3 eulerA = cam.transform.eulerAngles;

        reflectCamera.transform.eulerAngles = new Vector3(-eulerA.x, eulerA.y, eulerA.z);
        reflectCamera.transform.position = cam.transform.position;

        Vector3 pos = reflectiveSurface.transform.position;
        pos.y = reflectiveSurface.position.y;
        Vector3 normal = reflectiveSurface.transform.up;
        float d = -Vector3.Dot(normal, pos) - clipPlaneOffset;
        Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

        Matrix4x4 reflection = Matrix4x4.zero;
        reflection = CalculateReflectionMatrix(reflection, reflectionPlane);
        m_Oldpos = cam.transform.position;
        Vector3 newpos = reflection.MultiplyPoint(m_Oldpos);

        reflectCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;
        Vector4 clipPlane = CameraSpacePlane(reflectCamera, pos, normal, 1.0f);

        Matrix4x4 projection = cam.projectionMatrix;
        projection = CalculateObliqueMatrix(projection, clipPlane);
        reflectCamera.projectionMatrix = projection;

        reflectCamera.transform.position = newpos;
        Vector3 euler = cam.transform.eulerAngles;
        reflectCamera.transform.eulerAngles = new Vector3(-euler.x, euler.y, euler.z);
        */

        // Sure would be nice if we could automatically do stereo instanced rendering to a split texture
        if (cam.stereoEnabled)
        {
            if (cam.stereoTargetEye == StereoTargetEyeMask.Both || cam.stereoTargetEye == StereoTargetEyeMask.Left)
            {
                Vector3 eyePos = cam.transform.TransformPoint(SteamVR.instance.eyes[0].pos);
                Quaternion eyeRot = cam.transform.rotation * SteamVR.instance.eyes[0].rot;
                Matrix4x4 projectionMatrix = GetSteamVRProjectionMatrix(cam, Valve.VR.EVREye.Eye_Left);
                RenderMirror(eyePos, eyeRot, projectionMatrix, LeftEyeRect);
            }

            if (cam.stereoTargetEye == StereoTargetEyeMask.Both || cam.stereoTargetEye == StereoTargetEyeMask.Right)
            {
                Vector3 eyePos = cam.transform.TransformPoint(SteamVR.instance.eyes[1].pos);
                Quaternion eyeRot = cam.transform.rotation * SteamVR.instance.eyes[1].rot;
                Matrix4x4 projectionMatrix = GetSteamVRProjectionMatrix(cam, Valve.VR.EVREye.Eye_Right);

                RenderMirror(eyePos, eyeRot, projectionMatrix, RightEyeRect);
            }
        }
        else
        {
            RenderMirror(cam.transform.position, cam.transform.rotation, cam.projectionMatrix, DefaultRect);
        }


        //reflectCamera.Render();

        GL.invertCulling = false;
    }

    // Calculates reflection matrix around the given plane
    private static Matrix4x4 CalculateReflectionMatrix(Vector4 plane)
    {
        Matrix4x4 reflectionMat = Matrix4x4.identity;

        reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
        reflectionMat.m01 = (-2F * plane[0] * plane[1]);
        reflectionMat.m02 = (-2F * plane[0] * plane[2]);
        reflectionMat.m03 = (-2F * plane[3] * plane[0]);

        reflectionMat.m10 = (-2F * plane[1] * plane[0]);
        reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
        reflectionMat.m12 = (-2F * plane[1] * plane[2]);
        reflectionMat.m13 = (-2F * plane[3] * plane[1]);

        reflectionMat.m20 = (-2F * plane[2] * plane[0]);
        reflectionMat.m21 = (-2F * plane[2] * plane[1]);
        reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
        reflectionMat.m23 = (-2F * plane[3] * plane[2]);

        reflectionMat.m30 = 0F;
        reflectionMat.m31 = 0F;
        reflectionMat.m32 = 0F;
        reflectionMat.m33 = 1F;

        return reflectionMat;
    }

    void SaneCameraSettings(Camera helperCam)
    {
        helperCam.depthTextureMode = DepthTextureMode.None;
        helperCam.backgroundColor = Color.black;
        helperCam.clearFlags = CameraClearFlags.SolidColor;
        helperCam.renderingPath = RenderingPath.Forward;
    }


    static Matrix4x4 CalculateObliqueMatrix(Matrix4x4 projection, Vector4 clipPlane)
    {
        Vector4 q = projection.inverse * new Vector4(
            Sgn(clipPlane.x),
            Sgn(clipPlane.y),
            1.0F,
            1.0F
            );
        Vector4 c = clipPlane * (2.0F / (Vector4.Dot(clipPlane, q)));
        // third row = clip plane - fourth row
        projection[2] = c.x - projection[3];
        projection[6] = c.y - projection[7];
        projection[10] = c.z - projection[11];
        projection[14] = c.w - projection[15];

        return projection;
    }


    static Matrix4x4 CalculateReflectionMatrix(Matrix4x4 reflectionMat, Vector4 plane)
    {
        reflectionMat.m00 = (1.0F - 2.0F * plane[0] * plane[0]);
        reflectionMat.m01 = (-2.0F * plane[0] * plane[1]);
        reflectionMat.m02 = (-2.0F * plane[0] * plane[2]);
        reflectionMat.m03 = (-2.0F * plane[3] * plane[0]);

        reflectionMat.m10 = (-2.0F * plane[1] * plane[0]);
        reflectionMat.m11 = (1.0F - 2.0F * plane[1] * plane[1]);
        reflectionMat.m12 = (-2.0F * plane[1] * plane[2]);
        reflectionMat.m13 = (-2.0F * plane[3] * plane[1]);

        reflectionMat.m20 = (-2.0F * plane[2] * plane[0]);
        reflectionMat.m21 = (-2.0F * plane[2] * plane[1]);
        reflectionMat.m22 = (1.0F - 2.0F * plane[2] * plane[2]);
        reflectionMat.m23 = (-2.0F * plane[3] * plane[2]);

        reflectionMat.m30 = 0.0F;
        reflectionMat.m31 = 0.0F;
        reflectionMat.m32 = 0.0F;
        reflectionMat.m33 = 1.0F;

        return reflectionMat;
    }


    static float Sgn(float a)
    {
        if (a > 0.0F)
        {
            return 1.0F;
        }
        if (a < 0.0F)
        {
            return -1.0F;
        }
        return 0.0F;
    }


    Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
    {
        Vector3 offsetPos = pos + normal * clipPlaneOffset;
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(offsetPos);
        Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;

        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }
}


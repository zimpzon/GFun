using UnityEngine;

[RequireComponent(typeof(Camera))]
public class LightingCamera : MonoBehaviour
{
    public RenderTextureFormat LightingTextureFormat;
    public Camera ParentCamera;
    public float LightingResolution = 1.0f;
    public LightingImageEffect lightingImageEffect_;

    Camera lightingCam_;

    public void SetAmbientLightColor(Color color)
        => lightingCam_.backgroundColor = color;

    public Color GetAmbientLightColor()
        => lightingCam_.backgroundColor;

    private void Awake()
    {
        lightingCam_ = GetComponent<Camera>();
        lightingImageEffect_ = ParentCamera.GetComponent<LightingImageEffect>();
    }

    private void Update()
    {
        lightingCam_.backgroundColor = lightingImageEffect_.CurrentValues.AmbientLight;
        EnsureLightingTextureSize();
    }

    void EnsureLightingTextureSize()
    {
        int texW = Mathf.RoundToInt(ParentCamera.pixelWidth * LightingResolution);
        int texH = Mathf.RoundToInt(ParentCamera.pixelHeight * LightingResolution);
        if (lightingCam_.targetTexture == null || lightingCam_.targetTexture.width != texW || lightingCam_.targetTexture.height != texH)
        {
            lightingCam_.targetTexture = RenderTexture.GetTemporary(texW, texH, 0, LightingTextureFormat);
            lightingImageEffect_.LightingTexture = lightingCam_.targetTexture;
        }
    }

    private void OnEnable()
    {
        AlignWithParentCamera(ParentCamera);
        EnsureLightingTextureSize();
    }

    private void OnDisable()
    {
        RenderTexture.ReleaseTemporary(lightingCam_.targetTexture);
        lightingCam_.targetTexture = null;
        lightingImageEffect_.LightingTexture = null;
    }

    void AlignWithParentCamera(Camera parent)
    {
        // Render before main camera
        lightingCam_.depth = parent.depth - 1;

        // Align size with main camera
        lightingCam_.orthographicSize = parent.orthographicSize;
    }
}

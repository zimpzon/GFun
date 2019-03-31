using UnityEngine;

namespace HordeEngine
{
    [RequireComponent(typeof(Camera))]
    public class LightingCamera : MonoBehaviour
    {
        public RenderTextureFormat LightingTextureFormat;
        public Camera ParentCamera;
        public float LightingResolution = 1.0f;
        public LightingImageEffect lightingImageEffect_;

        Camera lightingCam_;

        private void Awake()
        {
            lightingCam_ = GetComponent<Camera>();
            lightingImageEffect_ = ParentCamera.GetComponent<LightingImageEffect>();
        }

        private void OnEnable()
        {
            AlignWithParentCamera(ParentCamera);

            int texW = Mathf.RoundToInt(ParentCamera.pixelWidth * LightingResolution);
            int texH = Mathf.RoundToInt(ParentCamera.pixelHeight * LightingResolution);
            lightingCam_.targetTexture = RenderTexture.GetTemporary(texW, texH, 0, LightingTextureFormat);
            lightingImageEffect_.LightingTexture = lightingCam_.targetTexture;
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
}

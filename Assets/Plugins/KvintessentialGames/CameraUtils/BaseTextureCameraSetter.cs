using UnityEngine;

namespace KvinterGames.CameraUtils
{
    [ExecuteAlways]
    public abstract class BaseTextureCameraSetter : MonoBehaviour
    {
        public Camera textureCamera;

        protected CameraTextureSource cameraTextureSource;

        public RenderTexture RenderTexture => cameraTextureSource?.renderTexture;

        private void Start()
        {
            if (textureCamera == null || !textureCamera.isActiveAndEnabled)
            {
                return;
            }

            if (!textureCamera.gameObject.TryGetComponent(out cameraTextureSource))
            {
                cameraTextureSource = textureCamera.gameObject.AddComponent<CameraTextureSource>();
            }

            cameraTextureSource.OnTextureCreated += SetTexture;
            SetTexture(cameraTextureSource.renderTexture);
        }

        public void SetCamera(Camera camera)
        {
            textureCamera = camera;
            cameraTextureSource = camera.gameObject.GetComponent<CameraTextureSource>();
            if (cameraTextureSource == null)
            {
                cameraTextureSource = camera.gameObject.AddComponent<CameraTextureSource>();
            }

            cameraTextureSource.OnTextureCreated += SetTexture;
            SetTexture(cameraTextureSource.renderTexture);
        }

        protected abstract void SetTexture(RenderTexture texture);

        [ContextMenu("Set Texture")]
        private void OnValidate()
        {
            if (textureCamera == null || cameraTextureSource == null)
            {
                return;
            }

            SetTexture(cameraTextureSource.renderTexture);
        }
    }
}
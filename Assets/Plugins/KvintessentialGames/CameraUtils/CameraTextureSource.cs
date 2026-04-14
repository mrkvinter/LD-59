using System;
using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KvinterGames.CameraUtils
{
    [ExecuteAlways]
    public class CameraTextureSource : MonoBehaviour
    {
        public RenderTexture renderTexture;

        [SerializeReference, InlineProperty, HideLabel, Header("Texture Size")]
        private ITextureSize textureSizeType = new ScreenSizeTextureSize();
        // public bool useScreenSize = true;
        // public Vector2Int screenSize;

        private Camera camera;
        private Vector2Int textureSize;

        public event Action<RenderTexture> OnTextureCreated;

        private void Awake()
        {
            camera = GetComponent<Camera>();
            CreateTexture();
        }


        private void CreateTexture()
        {
            if (renderTexture != null)
            {
                renderTexture.Release();
                renderTexture = null;
            }

            textureSize = textureSizeType.Size;
            renderTexture = new RenderTexture(textureSize.x, textureSize.y, 16, RenderTextureFormat.ARGBFloat,
                RenderTextureReadWrite.Linear)
            {
                filterMode = FilterMode.Point,
                useMipMap = false
            };
            renderTexture.useMipMap = false;
            renderTexture.Create();

            camera.targetTexture = renderTexture;

            OnTextureCreated?.Invoke(renderTexture);
        }

        private void Update()
        {
            if (textureSizeType.IsNeedUpdate(textureSize.x, textureSize.y))
            {
                CreateTexture();
            }
        }

#if UNITY_EDITOR

        [Button]
        private void SaveTexture()
        {
            var texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();
            RenderTexture.active = null;

            var bytes = texture2D.EncodeToPNG();
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var directoryInfo = Directory.GetParent(Application.dataPath)!;
            var directory = Path.Combine(directoryInfo.FullName, "Recordings", nameof(CameraTextureSource));
            var path = Path.Combine(directory, $"{gameObject.name}_{timestamp}.png");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllBytes(path, bytes);
        }
#endif

        public interface ITextureSize
        {
            public int Width { get; }
            public int Height { get; }
            public Vector2Int Size => new Vector2Int(Width, Height);

            public bool IsNeedUpdate(int width, int height) => Width != width || Height != height;
        }

        [Serializable]
        public class ScreenSizeTextureSize : ITextureSize
        {
            public int Width => Screen.width;
            public int Height => Screen.height;
        }

        [Serializable]
        public class CustomTextureSize : ITextureSize
        {
            [SerializeField] private Vector2Int _size;

            public int Width => _size.x;
            public int Height => _size.y;
        }

        [Serializable]
        public class PercentScreenSizeTextureSize : ITextureSize
        {
            [SerializeField, MinValue(0.1f), MaxValue(1)]
            private float _percent;

            public int Width => (int)(Screen.width * _percent);
            public int Height => (int)(Screen.height * _percent);
        }
    }
}
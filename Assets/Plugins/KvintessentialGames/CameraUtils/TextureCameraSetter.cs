using UnityEngine;
using UnityEngine.UI;

namespace KvinterGames.CameraUtils
{
    [ExecuteAlways]
    public class TextureCameraSetter : BaseTextureCameraSetter
    {
        public RawImage renderTexture;
        
        protected override void SetTexture(RenderTexture texture)
        {
            renderTexture.texture = texture;
        }
    }
}
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Utilities
{
    public class SpriteRendererAnimation : BaseImageAnimation
    {
        [SerializeField, ReadOnly] private SpriteRenderer spriteRenderer;

        protected override void SetSprite(Sprite sprite)
        {
            spriteRenderer.sprite = sprite;
        }

        private void OnValidate()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
}
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Utilities
{
    public class ImageAnimation : BaseImageAnimation
    {
        [SerializeField, ReadOnly] private Image image;

        protected override void SetSprite(Sprite sprite)
        {
            image.sprite = sprite;
        }

        private void OnValidate()
        {
            if (image == null)
                image = GetComponent<Image>();
        }
    }
}
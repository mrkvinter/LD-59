using Sirenix.OdinInspector;
using UnityEngine;

namespace KvintessentialGames.TextAnimations
{
    public class SineRotationTextAnimation : BaseSineTextAnimation
    {
        [Title("Sine Rotation Settings")]
        [SerializeField] private Vector2 magnitude;

        public override void ApplyCharTransform(ref CharTransformation charTransformation)
        {
            var sinValue = GetSinValue(charTransformation.Position.x);
            var rotation = magnitude.x + sinValue * (magnitude.y - magnitude.x);

            charTransformation.Rotation += rotation;
        }
    }
}
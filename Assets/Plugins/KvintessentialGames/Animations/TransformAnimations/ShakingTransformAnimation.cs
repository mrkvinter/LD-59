using KvinterGames.Animations.TransformAnimations;
using UnityEngine;

namespace Plugins.KvintessentialGames.Animations.TransformAnimations
{
    public class ShakingTransformAnimation : BaseTransformAnimation
    {
        [SerializeField] private float frequency;
        [SerializeField] private float strength = 2.5f;
        [SerializeField] private float angle = 5f;

        private float timer;

        protected override void ApplyTransform()
        {
            timer += Time.deltaTime;

            if (timer > frequency)
            {
                timer = 0;
                var vectorShift = Random.insideUnitCircle * (strength * 0.1f);
                var rotateShift = Random.Range(-angle, angle);

                transform.position = originalPosition + new Vector3(vectorShift.x, vectorShift.y, 0);
                transform.rotation = originalRotation * Quaternion.Euler(0, 0, rotateShift);
            }
        }
    }
}
using UnityEngine;

namespace Game.Utilities.Extensions
{
    public static class GameObjectExtensions
    {
        public static void SetSafeActive(this GameObject gameObject, bool active)
        {
            if (gameObject == null)
                return;

            gameObject.SetActive(active);
        }
        
        public static void SetSafeActive(this Transform transform, bool active)
        {
            if (transform == null)
                return;

            transform.gameObject.SetActive(active);
        }
    }
}
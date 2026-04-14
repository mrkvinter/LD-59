using TMPro;
using UnityEngine;

namespace DebugUtilities
{
    public class HeightSetter : MonoBehaviour
    {
        [SerializeField] private GameObject target;
        [SerializeField] private TMP_Text text;

        [ContextMenu("Update info")]
        public void UpdateInfo()
        {
            var bounds = new Bounds();

            foreach (var targetCollider in target.GetComponents<Collider>())
            {
                bounds.Encapsulate(targetCollider.bounds);
            }

            var height = bounds.size.y;
            text.text = $"{height:G}m";
        }

        private void OnValidate()
        {
            UpdateInfo();
        }
    }
}
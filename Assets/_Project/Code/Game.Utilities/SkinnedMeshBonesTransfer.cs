using Sirenix.OdinInspector;
using UnityEngine;

namespace Code.Game.Editor
{
    public class SkinnedMeshBonesTransfer : MonoBehaviour
    {
        public SkinnedMeshRenderer sourceMeshRenderer;
        public Transform[] bones;

        [Button]
        public void Transfer()
        {
            var targetMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            targetMeshRenderer.bones = sourceMeshRenderer.bones;
            bones = sourceMeshRenderer.bones;
        }
    }
}
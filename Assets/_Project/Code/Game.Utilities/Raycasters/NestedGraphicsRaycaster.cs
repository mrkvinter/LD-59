
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Utilities.Raycasters
{
    public class NestedGraphicsRaycaster : GraphicRaycaster
    {
        [SerializeField] private BaseParentRaycaster parentRaycaster;
        
        public BaseParentRaycaster ParentRaycaster => parentRaycaster;
        
        public override void Raycast(PointerEventData eventData, System.Collections.Generic.List<RaycastResult> resultAppendList)
        {
            if (parentRaycaster == null || !parentRaycaster.IsRaycasting)
            {
                base.Raycast(eventData, resultAppendList);
            }
        }
    }
}
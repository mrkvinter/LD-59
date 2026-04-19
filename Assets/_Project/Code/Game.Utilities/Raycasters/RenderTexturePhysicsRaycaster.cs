using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Utilities.Raycasters
{
    public class RenderTexturePhysicsRaycaster : PhysicsRaycaster
    {
        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            Camera cam = eventCamera;
            if (cam == null)
            {
                base.Raycast(eventData, resultAppendList);
                return;
            }

            Vector2 original = eventData.position;
            float u = original.x / Screen.width;
            float v = original.y / Screen.height;
            eventData.position = new Vector2(u * cam.pixelWidth, v * cam.pixelHeight);
            try
            {
                base.Raycast(eventData, resultAppendList);
            }
            finally
            {
                eventData.position = original;
            }
        }
    }
}
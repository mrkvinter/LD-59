using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Utilities
{
    public class InsideGraphicRaycaster : GraphicRaycaster
    {
        [SerializeField] private RectTransform target;
    
        private Canvas targetCanvas;

        protected override void Awake()
        {
            base.Awake();

            targetCanvas = target.GetComponentInParent<Canvas>();
        }

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            var canvasSize = targetCanvas.pixelRect.size;
            var targetSize = ((RectTransform)targetCanvas.transform).sizeDelta;
            var ratioScale = targetSize / canvasSize;

            var offset = GetOffset(target, targetCanvas);

            var originalPosition = eventData.position;
            var originalPressPosition = eventData.pressPosition;
            
            eventData.position -= offset;
            eventData.pressPosition -= offset;
            eventData.position *= ratioScale;
            eventData.pressPosition *= ratioScale;
            base.Raycast(eventData, resultAppendList);
            eventData.position = originalPosition;
            eventData.pressPosition = originalPressPosition;
        }

        private static Vector2 GetOffset(RectTransform targetTransform, Canvas canvas)
        {
            var corners = new Vector3[4];
            targetTransform.GetWorldCorners(corners);
            var min = new Vector3(float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue);
            for (var i = 0; i < 4; i++)
            {
                var corner = corners[i];
                min = Vector3.Min(min, corner);
                max = Vector3.Max(max, corner);
            }
        
            var minScreen = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, min);

            return minScreen;
        }
    }
}
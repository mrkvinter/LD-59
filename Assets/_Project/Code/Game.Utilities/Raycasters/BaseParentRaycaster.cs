using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Utilities.Raycasters
{
    public interface INestedRaycaster
    {
        BaseRaycaster Raycaster { get; }
    }
    [ExecuteAlways]
    [RequireComponent(typeof(MeshRenderer))]
    public class BaseParentRaycaster : BaseRaycaster
    {
        [SerializeField] private BaseRaycaster[] raycasters;
        
        private MeshRenderer meshRenderer;
        private Bounds? bounds;
        private List<RaycastResult> results = new();
        private List<INestedRaycaster> nestedRaycasters = new();
        
        private MeshRenderer MeshRenderer => meshRenderer != null ? meshRenderer : meshRenderer = GetComponent<MeshRenderer>();
        [ShowInInspector] private Bounds Bounds => MeshRenderer.bounds;
        [ShowInInspector] private Vector2 Size => GetWorldRect().size;
        [ShowInInspector] private float Ratio => Size.x / Size.y;
        public override Camera eventCamera => Camera.main;
        public bool IsRaycasting { get; private set; }

        public void AddNestedRaycaster(INestedRaycaster nestedRaycaster)
        {
            nestedRaycasters.Add(nestedRaycaster);
        }

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (!IsActive())
            {
                return;
            }
            
            IsRaycasting = true;
            // var ray = eventCamera.ScreenPointToRay(eventData.position);
            var rect = GetWorldRect();
            var offset = rect.min;
            
            var originalPosition = eventData.position;
            var originalPressPosition = eventData.pressPosition;
            
            for (var i = 0; i < raycasters.Length; i++)
            {
                results.Clear();
                var raycaster = raycasters[i];
                var ratioScale = raycaster.eventCamera.pixelRect.width / rect.width;
                eventData.position = originalPosition - offset;
                eventData.pressPosition = originalPressPosition - offset;
                eventData.position *= ratioScale;
                eventData.pressPosition *= ratioScale;
                raycaster.Raycast(eventData, results);
                foreach (var raycastResult in results)
                {
                    // var result = raycastResult;
                    // result.module = this;
                    resultAppendList.Add(raycastResult);
                }
                Debug.Log($"Position: {eventData.position}, PressPosition: {eventData.pressPosition}");
            }
            
            eventData.position = originalPosition;
            eventData.pressPosition = originalPressPosition;
            IsRaycasting = false;
        }

        private Rect GetWorldRect()
        {
            var min = Bounds.min;
            var max = Bounds.max;
            
            var minScreen = eventCamera.WorldToScreenPoint(min);
            var maxScreen = eventCamera.WorldToScreenPoint(max);

            return new Rect(minScreen, maxScreen - minScreen);
        }

        private void OnGUI()
        {
            var min = Bounds.min;
            var max = Bounds.max;
            
            var screenSize = eventCamera.WorldToScreenPoint(Vector3.one);
            var minScreen = screenSize - eventCamera.WorldToScreenPoint(min);
            var maxScreen = screenSize - eventCamera.WorldToScreenPoint(max);

            var rect = new Rect(minScreen, maxScreen - minScreen);
            
            GUI.Box(rect, "TEST");
            
            GUI.Box(new Rect(minScreen, Vector2.one * 100), "MIN");
            GUI.Box(new Rect(maxScreen, Vector2.one * 100), "MAX");
            
            Debug.Log($"Min: {minScreen}, Max: {maxScreen}");
        }
        [ExecuteAlways]
        [RequireComponent(typeof(MeshRenderer))]
        public class RaycasterMeshForwarder : BaseRaycaster
        {
            [SerializeField] private BaseRaycaster[] raycasters;
        
            private MeshRenderer meshRenderer;
            private Bounds? bounds;
            private List<RaycastResult> results = new();
        
            private MeshRenderer MeshRenderer => meshRenderer != null ? meshRenderer : meshRenderer = GetComponent<MeshRenderer>();
            [ShowInInspector] private Bounds Bounds => MeshRenderer.bounds;
            [ShowInInspector] private Vector2 Size => GetWorldRect().size;
            [ShowInInspector] private float Ratio => Size.x / Size.y;
            public override Camera eventCamera => Camera.main;

            public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
            {
                if (!IsActive())
                {
                    return;
                }
            
                // var ray = eventCamera.ScreenPointToRay(eventData.position);
                var rect = GetWorldRect();
                var offset = rect.min;
            
                var originalPosition = eventData.position;
                var originalPressPosition = eventData.pressPosition;
            
                for (var i = 0; i < raycasters.Length; i++)
                {
                    results.Clear();
                    var raycaster = raycasters[i];
                    var ratioScale = raycaster.eventCamera.pixelRect.width / rect.width;
                    eventData.position = originalPosition - offset;
                    eventData.pressPosition = originalPressPosition - offset;
                    eventData.position *= ratioScale;
                    eventData.pressPosition *= ratioScale;
                    raycaster.Raycast(eventData, results);
                    foreach (var raycastResult in results)
                    {
                        // var result = raycastResult;
                        // result.module = this;
                        resultAppendList.Add(raycastResult);
                    }
                    Debug.Log($"Position: {eventData.position}, PressPosition: {eventData.pressPosition}");
                }
            
                eventData.position = originalPosition;
                eventData.pressPosition = originalPressPosition;
            }

            private Rect GetWorldRect()
            {
                var min = Bounds.min;
                var max = Bounds.max;
            
                var minScreen = eventCamera.WorldToScreenPoint(min);
                var maxScreen = eventCamera.WorldToScreenPoint(max);

                return new Rect(minScreen, maxScreen - minScreen);
            }

            private void OnGUI()
            {
                var min = Bounds.min;
                var max = Bounds.max;
            
                var screenSize = eventCamera.WorldToScreenPoint(Vector3.one);
                var minScreen = screenSize - eventCamera.WorldToScreenPoint(min);
                var maxScreen = screenSize - eventCamera.WorldToScreenPoint(max);

                var rect = new Rect(minScreen, maxScreen - minScreen);
            
                GUI.Box(rect, "TEST");
            
                GUI.Box(new Rect(minScreen, Vector2.one * 100), "MIN");
                GUI.Box(new Rect(maxScreen, Vector2.one * 100), "MAX");
            
                Debug.Log($"Min: {minScreen}, Max: {maxScreen}");
            }
        }
    }
}
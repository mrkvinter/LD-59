using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Code.Cheats
{
    public class InputInfoCheatCategory : ICheatCategory
    {
        private string lastClickedObject = "none";

        public void Draw()
        {
            var mouse = Mouse.current;
            if (mouse == null)
            {
                GUILayout.Label("No mouse device");
                return;
            }

            var mouseScreen = mouse.position.ReadValue();
            GUILayout.Label($"Mouse (screen): ({mouseScreen.x:F0}, {mouseScreen.y:F0})");

            var cam = Camera.main;
            if (cam != null)
            {
                var mouseWorld = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, cam.nearClipPlane));
                GUILayout.Label($"Mouse (world):  ({mouseWorld.x:F2}, {mouseWorld.y:F2}, {mouseWorld.z:F2})");
            }

            GUILayout.Space(8);

            var overPointer = GetObjectUnderPointer(cam, mouseScreen);
            GUILayout.Label($"Under pointer: {overPointer ?? "none"}");

            GUILayout.Space(8);

            if (mouse.leftButton.wasPressedThisFrame)
                lastClickedObject = overPointer ?? "none";

            GUILayout.Label($"Last clicked:  {lastClickedObject}");

            GUILayout.Space(8);

            var focused = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            GUILayout.Label($"Current focus: {(focused != null ? focused.name : "none")}");

            GUILayout.Space(8);

            GUILayout.Label($"LMB: {mouse.leftButton.isPressed}  RMB: {mouse.rightButton.isPressed}  MMB: {mouse.middleButton.isPressed}");
        }

        private static string GetObjectUnderPointer(Camera cam, Vector2 mouseScreen)
        {
            if (EventSystem.current != null)
            {
                var pointerData = new PointerEventData(EventSystem.current) { position = mouseScreen };
                var uiResults = new System.Collections.Generic.List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, uiResults);
                if (uiResults.Count > 0)
                    return $"{uiResults[0].gameObject.name} [UI]";
            }

            if (cam != null)
            {
                var ray = cam.ScreenPointToRay(mouseScreen);
                if (Physics.Raycast(ray, out var hit3d))
                    return $"{hit3d.collider.gameObject.name} [{hit3d.collider.GetType().Name}]";

                var hit2d = Physics2D.GetRayIntersection(ray);
                if (hit2d.collider != null)
                    return $"{hit2d.collider.gameObject.name} [2D]";
            }

            return null;
        }
    }
}
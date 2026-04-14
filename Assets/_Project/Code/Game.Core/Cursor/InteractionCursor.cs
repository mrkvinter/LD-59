using Game.Utilities.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Cursor
{
    public class InteractionCursor : MonoBehaviour
    {
        [SerializeField] private Image mainCursor;

        [SerializeField] private Transform defaultCursor;
        [SerializeField] private Transform attackCursor;
        [SerializeField] private Transform hoveredAttackCursor;
        [SerializeField] private Transform interactCursor;
        [SerializeField] private Transform lookAtCursor;
        [SerializeField] private Transform unavailableCursor;
        
        private PointerType pointerType;
        private RectTransform rectTransform;
        private RectTransform canvasRectTransform;
        private Canvas canvas;
        private Camera cursorCamera;
        private Vector2 cursorPosition;

        private void Awake()
        {
            UnityEngine.Cursor.visible = false;
            rectTransform = GetComponent<RectTransform>();
            canvas = rectTransform.GetComponentInParent<Canvas>();
            canvasRectTransform = canvas.GetComponent<RectTransform>();
        }
        
        public void SetCamera(Camera camera)
        {
            cursorCamera = camera;
        }

        protected void FixedUpdate()
        {
            UnityEngine.Cursor.visible = false;
            cursorPosition = Input.mousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, cursorPosition, canvas.worldCamera, out var screenPosition);
            rectTransform.anchoredPosition = screenPosition;
        }

        public void SetPointer(PointerType pointerType)
        {
            this.pointerType = pointerType;
            UpdatePointer();
        }

        private void UpdatePointer()
        {
            defaultCursor.SetSafeActive(pointerType == PointerType.Default);
            attackCursor.SetSafeActive(pointerType == PointerType.Attack);
            hoveredAttackCursor.SetSafeActive(pointerType == PointerType.HoveredAttack);
            interactCursor.SetSafeActive(pointerType == PointerType.Interact);
            lookAtCursor.SetSafeActive(pointerType == PointerType.LookAt);
            unavailableCursor.SetSafeActive(pointerType == PointerType.Unavailable);
        }

        public enum PointerType
        {
            Default,
            Attack,
            HoveredAttack,
            
            Interact,
            LookAt, 
            
            Unavailable,
        }
    }
}
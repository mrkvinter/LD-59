using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Utilities
{
    public class PointerEventsHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public event System.Action<PointerEventData> OnPointerEnterEvent;
        public event System.Action<PointerEventData> OnPointerExitEvent;
        public event System.Action<PointerEventData> OnPointerClickEvent;

        public void OnPointerEnter(PointerEventData eventData) => OnPointerEnterEvent?.Invoke(eventData);

        public void OnPointerExit(PointerEventData eventData) => OnPointerExitEvent?.Invoke(eventData);
        public void OnPointerClick(PointerEventData eventData) => OnPointerClickEvent?.Invoke(eventData);
    }
}
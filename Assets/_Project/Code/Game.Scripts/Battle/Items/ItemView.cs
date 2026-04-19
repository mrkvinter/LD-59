using System;
using Code.Game.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Code.Game.Scripts.Battle.Items
{
    public class ItemView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private float hoverScale = 1.15f;

        public Action OnUse;
        public Item Item { get; set; }

        private Vector3 baseScale;
        private SceneLinks sceneLinks = G.Resolve<SceneLinks>();

        private void Awake()
        {
            baseScale = transform.localScale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Item is not { IsSelectable: true }) return;

            transform.localScale = baseScale * hoverScale;
            ShowDescription();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.localScale = baseScale;
            HideDescription();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Item is not { IsSelectable: true }) return;
            
            OnUse?.Invoke();
        }

        public void ShowDescription()
        {
            if (Item == null) return;
            sceneLinks.ItemDescription.gameObject.SetActive(true);
            sceneLinks.ItemDescription.Show(Item.Name, Item.Description);
        }
        
        public void HideDescription()
        {
            sceneLinks.ItemDescription.gameObject.SetActive(false);
        }
    }
}
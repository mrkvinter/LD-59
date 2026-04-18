using System;
using System.Linq;
using RG.DefinitionSystem.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.Game.Scripts.Battle
{
    public class CardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image spriteRenderer;
        [SerializeField] private float hoverScale = 1.15f;

        public Action OnClick;

        private Vector3 baseScale;
        
        public Sign SelectedSign { get; private set; }
        
        private void Awake()
        {
            baseScale = transform.localScale;
        }

        public void SetSign(Sign sign)
        {
            SelectedSign = sign;
            var signDef = DefManager.GetDefMap<SignDef>().DefinitionsEntries
                .FirstOrDefault(e => e.Sign == sign);

            if (signDef == null) 
            {
                spriteRenderer.gameObject.SetActive(false);
                return;
            }
            spriteRenderer.sprite = signDef.Sprite;
            spriteRenderer.gameObject.SetActive(true);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            transform.localScale = baseScale * hoverScale;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.localScale = baseScale;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke();
        }
    }
}
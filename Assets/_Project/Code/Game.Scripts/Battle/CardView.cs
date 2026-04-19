using System;
using System.Linq;
using RG.DefinitionSystem.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Code.Game.Scripts.Battle
{
    public class CardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private float hoverScale = 1.15f;

        public Action OnClick;

        private Vector3 baseScale;
        
        public Sign SelectedSign { get; private set; }
        public bool IsSelectable { get; set; } = true;
        
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
                return;
            }
            meshRenderer.material = signDef.Material;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!IsSelectable) return;
            transform.localScale = baseScale * hoverScale;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.localScale = baseScale;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsSelectable) return;
            OnClick?.Invoke();
        }
    }
}
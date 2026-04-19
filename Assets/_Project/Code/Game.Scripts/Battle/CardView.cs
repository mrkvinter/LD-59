using System;
using System.Linq;
using Game.Utilities;
using RG.DefinitionSystem.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Code.Game.Scripts.Battle
{
    public class Card
    {
        private SignDef signDef;

        public Sign Sign { get; private set; }
        public bool IsBLocked { get; private set; }
        public CardView View { get; private set; }
        public SignDef SignDef => signDef;

        public Card(SignDef signDef)
        {
            this.signDef = signDef;
            Sign = signDef.Sign;
        }

        public void SetView(CardView view)
        {
            View = view;
            View.SetSign(Sign);
            View.Card = this;
        }
        
        public void SetIsBLocked(bool isBLocked, Action onStateChanged = null)
        {
            IsBLocked = isBLocked;
            View.IsSelectable = !IsBLocked;
            var state = isBLocked ? CardView.KnifeState_Blocked : CardView.KnifeState_Default;
            View.KnifeStatefulObject.SetState(state, callback: onStateChanged);
        }
    }
    public class CardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public const string KnifeState_Default = "Default";
        public const string KnifeState_Blocked = "Blocked";

        public StatefulObject KnifeStatefulObject;

        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private float hoverScale = 1.15f;

        public Action OnClick;

        private Vector3 baseScale;
        
        public Sign SelectedSign { get; private set; }
        public bool IsSelectable { get; set; } = true;
        public Card Card { get; set; }
        
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
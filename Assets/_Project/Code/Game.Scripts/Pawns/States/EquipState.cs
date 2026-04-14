using System;
using Animancer;
using UnityEngine;

namespace Code.Game.Scripts.Pawns.States
{
    [CreateAssetMenu(menuName = "Animation/States/Equip")]
    public class EquipState : PawnState
    {
        [SerializeField] private ClipTransition animation;

        public event Action OnEquipEnd;

        public override Priority StatePriority => Priority.Medium;

        protected override void OnInitialize()
        {
            animation.Events.OnEnd = HandleEndEvent;
        }

        private void HandleEndEvent()
        {
            OnEquipEnd?.Invoke();

            StateMachine.ForceSetDefaultState();
        }

        protected override void OnEnter()
        {
            StateMachine.PlayTransitionClip(animation);
        }

        protected override void OnExit()
        {
            OnEquipEnd = null;
        }
    }
}
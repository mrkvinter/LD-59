using System;
using Animancer;
using UnityEngine;

namespace Code.Game.Scripts.Pawns.States
{
    [CreateAssetMenu(menuName = "Animation/States/Death")]
    public class DeathState : PawnState
    {
        [SerializeField] private ClipTransition deathAnimation;

        public event Action OnEnd;
        public override Priority StatePriority => Priority.High;

        protected override void OnInitialize()
        {
            deathAnimation.Events.OnEnd = HandleEndEvent;
        }

        protected override void OnEnter()
        {
            StateMachine.PlayTransitionClip(deathAnimation);
        }

        private void HandleEndEvent()
        {
            OnEnd?.Invoke();
            OnEnd = null;
        }
    }
}
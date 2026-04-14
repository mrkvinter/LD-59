using System;
using Animancer;
using UnityEngine;

namespace Code.Game.Scripts.Pawns.States
{
    public class ActionState : PawnState
    {
        [SerializeField] private ClipTransition animation;
        
        public event Action OnEnd;
        
        public override Priority StatePriority => Priority.Medium;
        
        protected override void OnInitialize()
        {
            animation.Events.OnEnd = HandleEndEvent;
        }

        private void HandleEndEvent()
        {
            OnEnd?.Invoke();
            StateMachine.ForceSetDefaultState();
        }

        protected override void OnEnter()
        {
            StateMachine.PlayTransitionClip(animation);
        }

        protected override void OnExit()
        {
            OnEnd = null;
        }
    }
}
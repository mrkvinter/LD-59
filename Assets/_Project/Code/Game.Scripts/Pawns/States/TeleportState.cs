using System;
using Animancer;
using UnityEngine;

namespace Code.Game.Scripts.Pawns.States
{
    [CreateAssetMenu(menuName = "Animation/States/Teleport")]
    public class TeleportState : PawnState
    {
        [SerializeField] private ClipTransition disappearTransition;
        [SerializeField] private ClipTransition appearTransition;

        public event Action OnDisappearEnd;
        public event Action OnAppearEnd;

        public override Priority StatePriority => Priority.Medium;

        protected override void OnInitialize()
        {
            disappearTransition.Events.OnEnd += OnDisappearAnimationEnd;
            appearTransition.Events.OnEnd += OnAppearAnimationEnd;
        }

        private void OnDisappearAnimationEnd()
        {
            OnDisappearEnd?.Invoke();

            StateMachine.PlayTransitionClip(appearTransition);
        }

        private void OnAppearAnimationEnd()
        {
            OnAppearEnd?.Invoke();

            StateMachine.ForceSetDefaultState();
        }

        protected override void OnEnter()
        {
            StateMachine.PlayTransitionClip(disappearTransition);
        }

        protected override void OnExit()
        {
            OnDisappearEnd = null;
            OnAppearEnd = null;
        }
    }
}
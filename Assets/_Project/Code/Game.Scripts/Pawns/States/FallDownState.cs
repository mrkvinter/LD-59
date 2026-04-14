using System;
using Animancer;
using UnityEngine;

namespace Code.Game.Scripts.Pawns.States
{
    [CreateAssetMenu(menuName = "Animation/States/FallDown")]
    public class FallDownState : PawnState
    {
        [Header("Fail to floor")]
        [SerializeField] private ClipTransition failToFloorAnimation;

        [Header("Fail to abyss")]
        [SerializeField] private ClipTransition failToAbyssAnimation;

        public FallDownType FallType { get; set; }

        public float FallDuration => FallType == FallDownType.Floor ? failToFloorAnimation.Length : failToAbyssAnimation.Length;

        public event Action OnEnd;

        public override Priority StatePriority => Priority.Medium;

        protected override void OnInitialize()
        {
            failToFloorAnimation.Events.OnEnd += HandleEndAnimation;
            failToAbyssAnimation.Events.OnEnd += HandleEndAnimation;
        }

        private void HandleEndAnimation()
        {
            OnEnd?.Invoke();
            StateMachine.TrySetDefaultState();
        }

        protected override void OnEnter()
        {
            if (FallType == FallDownType.Floor)
                StateMachine.PlayTransitionClip(failToFloorAnimation, true);
            else if (FallType == FallDownType.Abyss)
                StateMachine.PlayTransitionClip(failToAbyssAnimation);
            else
                Debug.LogError("Can't fall down with type " + FallType);
        }
        
        protected override void OnExit()
        {
            FallType = FallDownType.None;
            OnEnd = null;
        }
        
        public enum FallDownType
        {
            None,
            Floor,
            Abyss
        }
    }
}
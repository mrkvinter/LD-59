using Animancer;
using UnityEngine;

namespace Code.Game.Scripts.Pawns.States
{
    [CreateAssetMenu(menuName = "Animation/States/Lying")]
    public class LyingState : PawnState
    {
        [SerializeField] private ClipTransition lyingAnimation;

        public override Priority StatePriority => Priority.High;

        public override bool CanExitState
        {
            get
            {
                var next = StateMachine.NextState;
                return next is StandUpState || next.StatePriority >= StatePriority;
            }
        }

        protected override void OnEnter()
        {
            StateMachine.PlayTransitionClip(lyingAnimation);
        }
    }
}
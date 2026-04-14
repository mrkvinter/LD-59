using System;

namespace Code.Game.Scripts.Pawns.States
{
    public class HitReactionState : SinglePawnState
    {
        public event Action OnEnd;

        public override Priority StatePriority => Priority.Medium;
        
        protected override void OnAnimationEnd()
        {
            OnEnd?.Invoke();
            StateMachine.ForceSetDefaultState();
        }

        protected override void OnExit()
        {
            OnEnd = null;
        }
        
    }
}
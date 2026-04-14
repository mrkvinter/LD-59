namespace Code.Game.Scripts.Pawns.States
{
    public class JumpStartState : SinglePawnState
    {
        public InAirStartState InAir;

        public override Priority StatePriority => Priority.Medium;
        public override bool 
            CanExitState => AnimationState == null || AnimationState.NormalizedTime >= 1f;
        
        protected override void OnAnimationEnd()
        {
            StateMachine.ForceSetState(InAir);
        }
    }
    
    public class InAirStartState : SinglePawnState
    {
        private bool isGrounded;

        public override Priority StatePriority => Priority.Low;
        public override bool CanEnterState => StateMachine.CurrentState is not InAirStartState;
        public override bool CanExitState => base.CanExitState && StateMachine.NextState is JumpLandState;
    }
    
    public class JumpLandState : LinearMixerPawnState 
    {
        public override Priority StatePriority => Priority.Low;

        protected override void OnAnimationEnd()
        {
            state.Events(this).OnEnd = StateMachine.ForceSetDefaultState;
        }

    }
}
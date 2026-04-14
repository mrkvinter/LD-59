using Animancer;

namespace Code.Game.Scripts.Pawns.States
{
    public class SinglePawnState : PawnState
    {
        private ClipTransition animation;

        public ClipTransition Animation
        {
            get => animation;
            set
            {
                animation = value;
                PlayAnimation();
            }
        }

        public AnimancerState AnimationState { get; private set; }

        public float Length => Animation.Length;

        protected override void OnEnter() => PlayAnimation();

        protected virtual void PlayAnimation()
        {
            if (IsPlaying)
            {
                AnimationState = StateMachine.PlayTransitionClip(Animation);
                OnStateCreate();
                AnimationState.NormalizedTime = 0;
                AnimationState.Events(this).OnEnd = OnAnimationEnd;
            }
        }
        
        protected virtual void OnStateCreate() { }
        
        protected virtual void OnAnimationEnd() { }
    }
}
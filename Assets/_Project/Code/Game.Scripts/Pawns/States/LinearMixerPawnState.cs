using Animancer;
using Cysharp.Threading.Tasks.Triggers;

namespace Code.Game.Scripts.Pawns.States
{
    public class LinearMixerPawnState : PawnState
    {
        private LinearMixerTransition animation;
        protected LinearMixerState state;

        public LinearMixerTransition Animation
        {
            get => animation;
            set
            {
                animation = value;
                PlayAnimation();
            }
        }

        protected override void OnEnter() => PlayAnimation();

        protected override void OnExit()
        {
            state?.Events(this).Clear();
            state = null;
        }

        private void PlayAnimation()
        {
            if (IsPlaying)
            {
                state = StateMachine.PlayTransitionClip<LinearMixerTransition, LinearMixerState>(animation);
                state.Events(this).OnEnd = OnAnimationEnd;
            }
        }

        protected virtual void OnAnimationEnd()
        {
            
        }
    }
}
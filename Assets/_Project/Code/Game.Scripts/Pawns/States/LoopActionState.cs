using Animancer;
using UnityEngine;

namespace Code.Game.Scripts.Pawns.States
{
    [CreateAssetMenu(menuName = "Animation/States/LoopAction")]
    public class LoopActionState : PawnState
    {
        [SerializeField] private ClipTransition animation;
        
        public override Priority StatePriority => Priority.Medium;
        
        protected override void OnInitialize()
        {
        }

        protected override void OnEnter()
        {
            StateMachine.PlayTransitionClip(animation);
        }
        
        public void Stop()
        {
            StateMachine.ForceSetDefaultState();
        }
    }
}
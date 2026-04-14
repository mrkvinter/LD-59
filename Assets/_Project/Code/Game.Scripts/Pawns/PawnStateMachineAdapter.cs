using Animancer;
using Code.Game.Scripts.EntitySystem;
using Code.Game.Scripts.Pawns.States;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Code.Game.Scripts.Pawns
{
    public class PawnStateMachineAdapter : EntityComponentAdapter<PawnStateMachine, Pawn>
    {
        [SerializeField] private AnimancerComponent animancer;
        [SerializeField] private ClipTransition idleAnimation;
        [SerializeField] private ClipTransition gettingDamageAnimation;
        [SerializeField] private ClipTransition jumpStartAnimation;
        [SerializeField] private ClipTransition inAirAnimation;
        [SerializeField] private LinearMixerTransition jumpLandAnimation;
        [SerializeField] private LinearMixerTransition moveAnimation;

        [ShowInInspector] public PawnStateMachine StateMachine => Component;
        [ShowInInspector] public string CurrentState => StateMachine?.CurrentState.ToString();

        protected override PawnStateMachine CreateComponent()
        {
            var pawnAnimations = new PawnStateMachine(Entity, animancer);
            pawnAnimations.GetState<IdleState>().Animation = idleAnimation;
            pawnAnimations.GetState<MoveState>().Animation = moveAnimation;
            pawnAnimations.GetState<JumpStartState>().Animation = jumpStartAnimation;
            pawnAnimations.GetState<InAirStartState>().Animation = inAirAnimation;
            pawnAnimations.GetState<JumpLandState>().Animation = jumpLandAnimation;
            pawnAnimations.GetState<HitReactionState>().Animation = gettingDamageAnimation;
            pawnAnimations.SetDefault<IdleState>();

            return pawnAnimations;
        }
    }
}
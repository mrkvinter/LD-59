using System;
using System.Collections.Generic;
using Animancer;
using Animancer.FSM;
using Code.Game.Scripts.Pawns.States;
using Code.Game.Scripts.EntitySystem;
using VContainer.Unity;

namespace Code.Game.Scripts.Pawns
{
    public class PawnStateMachine : StateMachine<IPawnState>.WithDefault, IEntityComponent, IInitializable, ITickable
    {
        private readonly Dictionary<Type, States.PawnState> animationStates = new();
        private readonly AnimancerComponent animancer;
        private readonly Pawn pawn;

        public AnimancerComponent Animancer => animancer;

        public IdleState Idle { get; private set; }
        public MoveState Move { get; private set; }
        public JumpStartState JumpStart { get; private set; }
        public InAirStartState InAir { get; private set; }
        public JumpLandState JumpLand { get; private set; }
        public HitReactionState HitReaction { get; private set; }

        public PawnStateMachine(Pawn pawn, AnimancerComponent animancer)
        {
            this.animancer = animancer;
            this.pawn = pawn;

            Idle = GetState<IdleState>();
            Move = GetState<MoveState>();
            JumpStart = GetState<JumpStartState>();
            InAir = GetState<InAirStartState>();
            JumpLand = GetState<JumpLandState>();
            HitReaction = GetState<HitReactionState>();

            JumpStart.InAir = InAir;
            
            pawn.Health.OnDamage += OnDamaged;
        }

        public void OnDamaged()
        {
            TrySetState(HitReaction);
        }

        public void SetGrounded(bool isGrounded)
        {
            if (isGrounded && CurrentState is InAirStartState)
            {
                TrySetState(JumpLand);
            }

            if (!isGrounded && CurrentState is MoveState)
                TrySetState(InAir);
        }

        public void SetSpeed(float speed)
        {
            if (CurrentState.StatePriority <= Move.StatePriority)
            {
                TrySetState(Move);
            }

            Move.Speed = speed;
        }

        public void SetJump(bool isJump)
        {
            if (isJump)
            {
                TrySetState(JumpStart);
            }
        }

        public void SetMotionSpeed(float speed)
        {
            
        }

        public void SetFreeFall(bool isFreeFall)
        {
            if (isFreeFall)
            {
                TrySetState(InAir);
            }
        }

        public void Initialize()
        {
            DefaultState = GetState<IdleState>();
            ForceSetDefaultState();
        }

        public void Tick()
        {
            Move.Speed = pawn.MovementComponent.Speed;
        }

        public AnimancerState PlayTransitionClip(ITransition transition, bool rootMotion = false)
        {
            var state = animancer.Play(transition);
            animancer.Animator.applyRootMotion = rootMotion;

            return state;
        }

        public TState PlayTransitionClip<TTransition, TState>(TTransition transition, bool rootMotion = false)
            where TTransition : ITransition
            where TState : AnimancerState
        {
            var state = animancer.Play(transition);
            animancer.Animator.applyRootMotion = rootMotion;

            return state as TState;
        }

        public T GetState<T>() where T : States.PawnState, new()
        {
            var type = typeof(T);
            if (animationStates.TryGetValue(type, out var state))
                return state as T;
        
            var animationState = new T();
            animationState.Initialize(this);
            animationStates.Add(type, animationState);
        
            return animationState;
        }

        public void SetDefault<T>()
            where T : States.PawnState, new()
        {
            DefaultState = GetState<T>();
        }
    }
}
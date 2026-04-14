using System;
using Animancer.FSM;
using Code.Game.Scripts.Pawns;
using Mono.CSharp;
using UnityEngine;

namespace Code.Game.Scripts.Pawns.States
{
    public interface IPawnState : IState
    {
        Priority StatePriority { get; }
    }

    public abstract class PawnState : IPawnState
    {
        public event Action OnExitEvent;

        public virtual bool CanEnterState => true;

        public virtual bool CanExitState
        {
            get
            {
                var next = StateMachine.NextState;
                return next.StatePriority >= StatePriority;
            }
        }
        
        public virtual Priority StatePriority => Priority.Low;
        
        public bool IsPlaying { get; protected set; }

        protected PawnStateMachine StateMachine { get; private set; }

        public void Initialize(PawnStateMachine pawnStateMachine)
        {
            Debug.Log($"Initializing Animation State {GetType().Name}");
            StateMachine = pawnStateMachine;
            OnInitialize();
        }

        protected virtual void OnInitialize()
        {
        }

        public void OnEnterState()
        {
            IsPlaying = true;
            OnEnter();
        }

        public void OnExitState()
        {
            OnExitEvent?.Invoke();
            OnExit();
            IsPlaying = false;
            OnExitEvent = null;
        }

        protected virtual void OnEnter() { }
        protected virtual void OnExit() { }

        public override string ToString() => $"{GetType().Name} (Priority: {StatePriority})";
    }
    
    public enum Priority
    {
        Low,
        Medium,
        High,
    }
}
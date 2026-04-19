using System;
using Animancer;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Utilities
{
    public class StatefulObject : MonoBehaviour
    {
        [SerializeField] private AnimancerComponent animancerComponent;
        [SerializeField] private Transform visual;
        [SerializeField] private State[] states;
        [SerializeField] private string initialState;

        private State currentState;
        public State CurrentState => currentState;
        
        [ShowInInspector] public string CurrentStateName => currentState?.Name;

        private void Awake()
        {
            SetState(initialState, true);
        }

        public void SetState(string stateName, bool instant = false, Action callback = null)
        {
            var newState = Array.Find(states, state => state.Name == stateName);
            if (newState == null)
            {
                Debug.LogError($"State {stateName} not found");
                return;
            }

            visual.gameObject.SetActive(true);
            currentState = newState;
            var animState = animancerComponent.Play(currentState.Clip, currentState.TransitionDuration);
            if (instant)
            {
                animState.NormalizedTime = 1;
                visual.gameObject.SetActive(currentState.Visible);
                callback?.Invoke();
            }
            else
            {
                animState.Events(this).OnEnd = () =>
                {
                    visual.gameObject.SetActive(currentState.Visible);
                    callback?.Invoke();
                };
            }
        }
        
        public async UniTask SetStateAsync(string stateName, bool instant = false)
        {
            var tcs = new UniTaskCompletionSource();
            SetState(stateName, instant, () => tcs.TrySetResult());
            await tcs.Task;
        }


        [Serializable]
        public class State
        {
            public string Name;
            public ClipTransition Clip;
            public float TransitionDuration = 0.25f;
            public bool Visible;
        }
    }
}
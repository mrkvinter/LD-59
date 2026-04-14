using System;
using UnityEngine;

namespace Game.Utilities
{
    public class TriggerEventsHandler : MonoBehaviour
    {
        public event Action<Collider> OnTriggerEnterEvent;
        public event Action<Collider> OnTriggerExitEvent;
        public event Action<Collider> OnTriggerStayEvent;

        private void OnTriggerEnter(Collider other) => OnTriggerEnterEvent?.Invoke(other);
        private void OnTriggerExit(Collider other)  => OnTriggerExitEvent?.Invoke(other);
        private void OnTriggerStay(Collider other)  => OnTriggerStayEvent?.Invoke(other);
    }
}
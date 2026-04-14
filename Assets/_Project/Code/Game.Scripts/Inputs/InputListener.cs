using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Game.Scripts.Inputs
{
    public class InputListener : MonoBehaviour, PlayerInputActions.IPlayerActions
    {
        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool sprint;
        public bool attack;
        public bool useItem_1;

        public event Action AttackEvent;
        public event Action UseItemOneEvent;

        public bool IsKeyboard =>
            (Keyboard.current != null && Keyboard.current.wasUpdatedThisFrame) ||
            (Mouse.current != null && Mouse.current.wasUpdatedThisFrame);

        private PlayerInputActions controls;

        public void OnEnable()
        {
            if (controls == null)
            {
                controls = new PlayerInputActions();
                controls.Player.SetCallbacks(this);
            }
            controls.Player.Enable();
        }

        public void OnDisable()
        {
            controls.Player.Disable();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            move = context.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            look = context.ReadValue<Vector2>();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            jump = context.ReadValueAsButton();
        }

        public void OnSprint(InputAction.CallbackContext context) { }

        public void OnUseItem_1(InputAction.CallbackContext context)
        {
            if (context.performed) UseItemOneEvent?.Invoke();
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.performed) AttackEvent?.Invoke();
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(true);
        }

        private void SetCursorState(bool newState)
        {
            UnityEngine.Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
}
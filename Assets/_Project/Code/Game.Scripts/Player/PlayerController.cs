using UnityEngine;

namespace Code.Game.Scripts.Player
{
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float speed = 5.0f;
        [SerializeField] private float jumpForce = 5.0f;

        private CharacterController characterController;
        private Camera mainCamera;
        private bool isGrounded;

        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            mainCamera = Camera.main;
        }

        private void Update()
        {
            isGrounded = characterController.isGrounded;
            Move();
        }

        private void Move()
        {
            var (x, y) = GetInput();
            var moveDirection = mainCamera.transform.TransformDirection(new Vector3(x, 0, y));
            moveDirection.y = 0;
            moveDirection.Normalize();
            
            var movement = moveDirection * speed * Time.deltaTime;
            
            if (Input.GetButton("Jump") && isGrounded)
            {
                movement.y = jumpForce * Time.deltaTime;
            }
            else
            {
                movement.y = characterController.velocity.y + Physics.gravity.y * Time.deltaTime;
            }

            characterController.Move(movement);
        }

        private (float, float) GetInput()
        {
            var x = Input.GetAxis("Horizontal");
            var y = Input.GetAxis("Vertical");

            return (x, y);
        }
    }
}

using Code.Game.Scripts.EntitySystem;
using Code.Game.Scripts.Inputs;
using StarterAssets;
using UnityEngine;
using VContainer.Unity;

namespace Code.Game.Scripts.Pawns
{ 
    public class Pawn : Entity, ITickable
    {
        private readonly InputListener inputListener;
        private readonly Camera mainCamera;
        private readonly ThirdPersonController movementComponent;

        private PawnStateMachine pawnStateMachine;

        public HealthComponent Health { get; }
        public ThirdPersonController MovementComponent { get; private set; }
        public PawnStateMachine PawnStateMachine => pawnStateMachine ??= GetComponent<PawnStateMachine>();
        public CharacterController CharacterController { get; private set;}


        public override Vector3 Forward => CharacterController.transform.forward;

        public Vector3 AttackPoint => CharacterController.transform.position;

        public Pawn(
            EntityCatcher entityCatcher,
            GameObject gameObject) : base(gameObject)
        {
            inputListener = gameObject.GetComponent<InputListener>();
            MovementComponent = gameObject.GetComponent<ThirdPersonController>();
            CharacterController = gameObject.GetComponent<CharacterController>();
            mainCamera = Camera.main;

            Health = AddComponent(new HealthComponent(50));
        }


        public void Tick()
        {
            foreach (var tickable in GetComponents<ITickable>())
            {
                tickable.Tick();
            }
        }
    }
}
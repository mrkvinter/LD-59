using Code.Game.Scripts.EntitySystem;
using Game.Core.Contexts;
using RG.DefinitionSystem.Core;
using StarterAssets;

namespace Code.Game.Scripts.Pawns
{
    public class PawnAdapter : EntityAdapter<Pawn>
    {
        private void Awake()
        {
            var controller = GetComponent<ThirdPersonController>();
            controller.enabled = false;
        }

        protected override Pawn CreateEntity()
        {
            var entityCatcher = GameServiceLocator.Resolve<EntityCatcher>();
            var playerCharacter = new Pawn(entityCatcher, gameObject);
            
            return playerCharacter;
        }
        
        protected override void OnEntityInitialized()
        {
            GetComponent<ThirdPersonController>().enabled = true;
        }
    }
}
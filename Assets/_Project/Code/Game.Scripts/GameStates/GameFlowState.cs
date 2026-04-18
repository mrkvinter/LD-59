using Code.Game.Core;
using Code.Game.Scripts.Battle;
using Game.Core;

namespace Code.Game.Scripts.GameStates
{
    public class GameFlowState
    {
        private BattleState battleState;

        public GameFlowState()
        {
            battleState = new BattleState();
        }

        public void Initialize()
        {
            battleState.OnEnter();
            
            G.Resolve<SceneLinks>().RestartButton.onClick
                .AddListener(G.Resolve<IGameDirector>().RestartGame);
        }
    }
}
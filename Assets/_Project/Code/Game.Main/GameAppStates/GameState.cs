using Code.Game.Core;
using Code.Game.Scripts;
using Code.Game.Scripts.GameStates;
using Cysharp.Threading.Tasks;
using Game.Main.Settings;
using Game.Scripts.SaveLoadSystem;
using Game.UI.Base;
using Game.Utilities;
using RG.DefinitionSystem.Core;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace Game.Main.GameAppStates
{
    public class GameState
    {
        private readonly LifetimeScope parentLifetimeScope;
        private readonly GameAppSetting appGameSettings;

        private Scene mainScene;

        public LifetimeScope LifetimeScope { get; private set; }

        public GameState(LifetimeScope parentLifetimeScope)
        {
            this.parentLifetimeScope = parentLifetimeScope;

            appGameSettings = DefManager.GetSingletonDef<GameAppSetting>();
        }

        public async UniTask Enter(GameSaveData saveData = null)
        {
            var sceneMain = appGameSettings.GameScene;
            mainScene = SceneManager.GetSceneByName(sceneMain);
            if (!mainScene.IsValid() || !mainScene.isLoaded)
            {
                await SceneManager.LoadSceneAsync(sceneMain, LoadSceneMode.Additive).ToUniTask();
                mainScene = SceneManager.GetSceneByName(sceneMain);
            }

            SceneManager.SetActiveScene(mainScene);
            var canvasService = parentLifetimeScope.Container.Resolve<CanvasService>();
            var sceneLinks = SceneObjectsUtilities.RecursiveFindObject<SceneLinks>(mainScene.GetRootGameObjects());
            //canvasService.RegisterCanvas(CanvasTarget.Main, sceneLinks.CanvasMain);
            //canvasService.RegisterCanvas(CanvasTarget.GameUI, sceneLinks.CanvasGame);

            LifetimeScope = parentLifetimeScope.CreateChild(builder =>
            {
                builder.RegisterBuildCallback(G.Initialize);
                
                builder.RegisterInstance(sceneLinks).AsSelf();
                builder.Register<GameFlowState>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            });

            await UniTask.DelayFrame(2);
            
            LifetimeScope.Container.Resolve<GameFlowState>().Initialize();
        }

        public async UniTask Exit()
        {
            await SceneManager.UnloadSceneAsync(mainScene).ToUniTask();
        }
    }
}
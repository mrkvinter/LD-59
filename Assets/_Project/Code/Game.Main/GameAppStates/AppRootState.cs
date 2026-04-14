using System;
using Code.Game.Core;
using Cysharp.Threading.Tasks;
using Game.Core;
using Game.Core.Contexts;
using Game.Main.installers;
using Game.Scripts.SaveLoadSystem;
using Game.UI.Base;
using Game.UI.Views;
using KvinterGames;
using RG.DefinitionSystem.UnityAdapter;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Game.Main.GameAppStates
{
    public class AppRootState
    {
        private readonly LoadingScreenView loadingScreen;
        private readonly IGameDirector gameDirector;

        private AppGameState _appGameState;
        private ISaveLoadService saveLoadService;
        
        public AppGameState AppGameState => _appGameState;
        public ISaveLoadService SaveLoadService => saveLoadService;
        public LifetimeScope RootLifetimeScope { get; private set; }

        public AppRootState(IGameDirector gameDirector, LoadingScreenView loadingScreenView)
        {
            this.gameDirector = gameDirector;
            loadingScreen = loadingScreenView;
        }
        
        public async UniTask Enter()
        {
            UniTaskScheduler.UnobservedTaskException += HandleUnobservedTaskException;
            var rootScene = SceneManager.CreateScene("RootScene");
            rootScene.name = "RootScene";
            SceneManager.MoveGameObjectToScene(loadingScreen.transform.root.gameObject, rootScene);
            SceneManager.SetActiveScene(rootScene);
            var objectRegistry = new ObjectRegistry();
            objectRegistry.Initialize();

            // GameAnalytics.Initialize();
            await CreateEventSystem();
            var audioService = await CreateSoundController();
            var database = await Resources.LoadAsync<DefinitionDatabase>("DefinitionDatabase") as DefinitionDatabase;
            RootLifetimeScope = CreateRootLifetimeScope(database, audioService, objectRegistry);

            saveLoadService = new BlankSaveLoadService();
            
            _appGameState = new AppGameState(RootLifetimeScope);
        }

        private void HandleUnobservedTaskException(Exception obj)
        {
            Debug.LogError($"Unobserved exception: {obj}");
            // Handle the exception (e.g., log it, show a message to the user, etc.)
        }

        private LifetimeScope CreateRootLifetimeScope(DefinitionDatabase definitionDatabase, AudioService audioService, ObjectRegistry objectRegistry)
        {
            var lifetimeScope = LifetimeScope.Create(builder =>
            {
                builder.RegisterBuildCallback(RootServiceLocator.Initialize);
                
                builder.RegisterInstance(gameDirector).As<IGameDirector>();
                builder.RegisterInstance(audioService).As<AudioService>();
                builder.RegisterInstance(objectRegistry).As<ObjectRegistry>();
                builder.Register<CanvasService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
                builder.Register<UIService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
                // builder.Register<GameCheatFlags>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
                DefInstaller.Install(builder, definitionDatabase);
            });

            return lifetimeScope;
        }

        private static async UniTask CreateEventSystem()
        {
            var eventSystemPrefab = await Resources.LoadAsync<GameObject>("EventSystem").ToUniTask();
            Object.Instantiate(eventSystemPrefab);
        }

        private static async UniTask<AudioService> CreateSoundController()
        {
            var soundControllerPrefab = await Resources.LoadAsync<GameObject>("Sounds").ToUniTask();
            var o = Object.Instantiate(soundControllerPrefab) as GameObject;
            
            var audioService = o.GetComponent<AudioService>();
            return audioService;
        }
    }
}
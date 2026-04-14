using System;
using Code.Game.Core;
using Code.Game.Core.ExecutorSystem;
using Code.Game.Scripts;
using Code.Game.Scripts.EntitySystem;
using Cysharp.Threading.Tasks;
using Game.Core.Contexts;
using Game.Main.installers;
using Game.Scripts.SaveLoadSystem;
using Game.UI.Base;
using Game.Utilities.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace Game.Main.GameAppStates
{
    public class AppGameState
    {
        private readonly LifetimeScope parentLifetimeScope;

        public LifetimeScope LifetimeScope { get; private set; }
        
        private GameState gameState;
        private ObjectRegistry objectRegistry;

        public AppGameState(LifetimeScope parentLifetimeScope)
        {
            this.parentLifetimeScope = parentLifetimeScope;
        }

        public async UniTask Enter(GameSaveData saveData = null)
        {
            var scene = SceneManager.CreateScene("AppGameScene");
            scene.name = "AppGameScene";
            SceneManager.SetActiveScene(scene);

            LifetimeScope = parentLifetimeScope.CreateChild(builder =>
            {
                builder.RegisterBuildCallback(GameServiceLocator.Initialize);

                builder.Register<GameContext>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
                builder.Register<EntityCatcher>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
                builder.Register<EntityService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

                ExecutorInstaller.Install(builder);
            });

            gameState = new GameState(LifetimeScope);
            
            await gameState.Enter(saveData);
            
            objectRegistry = LifetimeScope.Container.Resolve<ObjectRegistry>();
            foreach (var registryObject in objectRegistry.Enumerate())
            {
                try
                {
                    registryObject.Initialize();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to initialize registry object.", registryObject);
                    Debug.LogException(ex);
                }
            }

            objectRegistry.OnRegistered += InitMonoRegistry;
            
            LifetimeScope.Container.Resolve<ActionExecutorSystem>();
            
            await UniTask.DelayFrame(1);

            if (saveData != null)
            {
                Load(saveData);
            }
        }
        
        public async UniTask Exit()
        {
            objectRegistry.OnRegistered -= InitMonoRegistry;
            await gameState.Exit();
            var scene = SceneManager.GetSceneByName("AppGameScene");

            var uiService = LifetimeScope.Container.Resolve<UIService>();
            uiService.Clear();
            LifetimeScope.Dispose();
            await SceneManager.UnloadSceneAsync(scene).ToUniTask();
        }

        public GameSaveData Save()
        {
            var gameSaveData = new GameSaveData();
            foreach (var persistent in LifetimeScope.Container.ResolveAll<IPersistent>())
            {
                persistent.Save(gameSaveData);
            }
            
            return gameSaveData;
        }

        private void InitMonoRegistry(ObjectRegistry.MonoRegistered monoRegistered)
        {
            UniTask.DelayFrame(1).ContinueWith(monoRegistered.Initialize).Forget();
        }

        private void Load(GameSaveData gameSaveData)
        {
            foreach (var persistent in LifetimeScope.Container.ResolveAll<IPersistent>())
            {
                persistent.Load(gameSaveData);
            }
        }
    }
}
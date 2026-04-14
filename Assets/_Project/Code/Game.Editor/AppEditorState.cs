using Code.Cheats;
using Game.Core.Contexts;
using Game.Main.installers;
using Game.Scripts.SaveLoadSystem;
using RG.DefinitionSystem.UnityAdapter;
using UnityEditor;
using UnityEngine;
using VContainer;

namespace Code.Game.Editor
{
    [InitializeOnLoad]
    public static class AppEditorState
    {
        static AppEditorState()
        {
            var builder = new ContainerBuilder();

            var database = Resources.Load<DefinitionDatabase>("Data/DefinitionDatabase");
            builder.Register<GameCheatFlags>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<DebugSaveLoadService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            DefInstaller.Install(builder, database);

            var resolver = builder.Build();
            EditorServiceLocator.Initialize(resolver);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Code.Game.Core.ExecutorSystem;
using JetBrains.Annotations;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Code.Game.Editor.ValueDrawers
{
    [UsedImplicitly]
    public class ExecutorDrawer<T> : OdinValueDrawer<ExecutorInfo<T>>
        where T : BaseExecutor
    {
        private const string none = "None";

        private bool isInitialized;
        private Dictionary<string, T> executors;
        private Dictionary<string, string> executorNiceNameById;
        private List<string> executorNames;
        private T currentExecutor;
        private PropertyTree argsDrawer;

        protected override void DrawPropertyLayout(GUIContent label)
        {
            Init();

            SirenixEditorGUI.BeginBox();
            DrawExecutor(label);
            SirenixEditorGUI.EndBox();
        }

        private void DrawExecutor(GUIContent label)
        {
            var value = ValueEntry.SmartValue;
            var niceName = executorNiceNameById[value.Id];
            var selectedExecutorIndex = executorNames.IndexOf(niceName);

            var newSelectedExecutorIndex =
                EditorGUILayout.Popup(label, selectedExecutorIndex, executorNames.ToArray());

            if (newSelectedExecutorIndex != selectedExecutorIndex)
            {
                if (newSelectedExecutorIndex == 0)
                {
                    value.Id = none;
                    ValueEntry.SmartValue = value;
                    return;
                }

                var id = executorNiceNameById
                    .FirstOrDefault(e => e.Value == executorNames[newSelectedExecutorIndex]).Key;
                value.Id = id;
                ValueEntry.SmartValue = value;
            }

            if (!executors.TryGetValue(value.Id, out currentExecutor))
            {
                return;
            }

            var argsJson = value.Args;
            var argsType = currentExecutor.GetType();
            var args = JsonUtility.FromJson(argsJson, argsType) ??
                       Activator.CreateInstance(argsType);

            // var oldArgs = JsonUtility.ToJson(args);
            if (argsDrawer?.TargetType != argsType)
                argsDrawer = null;
            
            argsDrawer ??= PropertyTree.Create(args);
            argsDrawer.Draw(false);
            // argsDrawer.ApplyChanges();
            
            value.Args = JsonUtility.ToJson(argsDrawer.WeakTargets.First());
            // if (oldArgs != newValue)
            // {
            //     value.Args = newValue;
            //     ValueEntry.SmartValue = value;
            // }

            argsDrawer.Dispose();

            ValueEntry.SmartValue = value;
        }

        private void Init()
        {
            if (isInitialized)
                return;

            if (string.IsNullOrEmpty(ValueEntry.SmartValue.Id))
            {
                ValueEntry.SmartValue = new ExecutorInfo<T> { Id = none, Args = ValueEntry.SmartValue.Args };
            }

            isInitialized = true;
            var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(e => e.GetTypes()).ToList();
            var types = allTypes
                .Where(e => e.IsSubclassOf(typeof(T)) && !e.IsAbstract && !e.IsGenericType)
                .ToList();

            executors = new Dictionary<string, T>();
            executorNiceNameById = new Dictionary<string, string>();
            executorNiceNameById.Add(none, none);
            executorNames = new List<string> { none };
            foreach (var type in types)
            {
                var executor = (T)Activator.CreateInstance(type);
                executors.Add(executor.Id, executor);
                var niceName = executor.Id.SplitPascalCase();
                executorNames.Add(niceName);
                executorNiceNameById.Add(executor.Id, niceName);
            }
        }
    }
}
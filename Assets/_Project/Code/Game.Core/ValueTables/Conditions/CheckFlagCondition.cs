using System;
using Code.Game.Core.ExecutorSystem.Conditions;
using Game.Core;
using UnityEngine;

namespace Code.Game.Core.Gameplay.ValueTables.Conditions
{
    [Serializable]
    public class CheckFlagCondition : Condition
    {
        [SerializeField] private string flag;
        [SerializeField] private bool value;

        public override bool Execute(IGameContext context) =>
            value ? context.ValueTable.GetFlag(flag) > 0 : context.ValueTable.GetFlag(flag) == 0;

        public override void OnStartTracking(IGameContext context)
        {
            context.ValueTable.OnFlagChanged += RaiseConditionChanged;
        }
        
        public override void OnDisposeTracking(IGameContext context)
        {
            context.ValueTable.OnFlagChanged -= RaiseConditionChanged;
        }
    }
}
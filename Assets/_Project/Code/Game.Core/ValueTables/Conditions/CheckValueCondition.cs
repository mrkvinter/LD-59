using System;
using Code.Game.Core.ExecutorSystem.Conditions;
using Game.Core;
using UnityEngine;

namespace Code.Game.Core.Gameplay.ValueTables.Conditions
{
    [Serializable]
    public class CheckValueCondition : Condition
    {
        [SerializeField] private string valueName;
        [SerializeField] private int value;
        [SerializeField] private ComparisonType comparisonType;

        public override bool Execute(IGameContext context) => comparisonType switch
        {
            ComparisonType.Equals => context.ValueTable.GetFlag(valueName) == value,
            ComparisonType.MoreThan => context.ValueTable.GetFlag(valueName) > value,
            ComparisonType.LessThan => context.ValueTable.GetFlag(valueName) < value,
            ComparisonType.LessOrEqual => context.ValueTable.GetFlag(valueName) <= value,
            ComparisonType.MoreOrEqual => context.ValueTable.GetFlag(valueName) >= value,
            _ => false
        };

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
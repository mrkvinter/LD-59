using System;
using Code.Game.Core.ExecutorSystem.Actions;
using Game.Core;
using UnityEngine;

namespace Code.Game.Core.Gameplay.ValueTables.Actions
{
    [Serializable]
    public class SetFlagGameAction : GameAction
    {
        [SerializeField] private string flag;
        [SerializeField] private bool value;

        public override void Execute(IGameContext context) 
            => context.ValueTable.SetFlag(flag, value ? 1 : 0);
    }
    
    [Serializable]
    public class SetValueGameAction : GameAction
    {
        [SerializeField] private string valueName;
        [SerializeField] private int value;

        public override void Execute(IGameContext context) 
            => context.ValueTable.SetFlag(valueName, value);
    }
    
    [Serializable]
    public class IncrementValueGameAction : GameAction
    {
        [SerializeField] private string valueName;
        [SerializeField] private int value = 1;

        public override void Execute(IGameContext context) 
            => context.ValueTable.SetFlag(valueName, context.ValueTable.GetFlag(valueName) + value);
    }
}
using System;
using System.Collections.Generic;

namespace Code.Game.Core.Gameplay.ValueTables
{
    public class ValueTable
    {
        private readonly Dictionary<string, int> flags = new();

        public IReadOnlyDictionary<string, int> Flags => flags;

        public event Action OnFlagChanged;

        public int GetFlag(string flag) => flags.GetValueOrDefault(flag, 0);

        public void SetFlag(string flag, int value)
        {
            flags[flag] = value;
            OnFlagChanged?.Invoke();
        }

        public bool HasFlag(string flag) => flags.ContainsKey(flag);
    }
}
using System;
using RG.DefinitionSystem.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Code.Game.Scripts.Player
{
    [Serializable]
    public sealed class PlayerDataSingletonDef : SingletonDefinition
    {
        [SuffixLabel("m")] public float InteractViewDistance;
        [SuffixLabel("°")] public float InteractViewAngle;
    }
}
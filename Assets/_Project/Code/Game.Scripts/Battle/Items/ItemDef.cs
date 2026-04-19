using System;
using RG.DefinitionSystem.Core;
using RG.DefinitionSystem.Core.Constants;
using UnityEngine;

namespace Code.Game.Scripts.Battle.Items
{
    [Serializable, WithConstants]
    public class ItemDef : Definition
    {
        public string Name;
        public Quaternion Rotation;
        [TextArea(3, 5)]
        public string Description;
        public GameObject Prefab;
    }
}
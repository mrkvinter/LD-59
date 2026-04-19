using System;
using System.Collections.Generic;
using System.Linq;
using RG.DefinitionSystem.Core;
using RG.DefinitionSystem.Core.Constants;
using UnityEngine;

namespace Code.Game.Scripts.Battle
{
    [Serializable, WithConstants]
    public class SignDef : Definition
    {
        public string Name;
        public Sign Sign;
        public Sprite Sprite;
        public Material Material;
        
        public List<Sign> BeatSigns = new();
        
        public static SignDef Get(Sign sign) => DefManager.GetDefMap<SignDef>().DefinitionsEntries
            .FirstOrDefault(e => e.Sign == sign);
    }
}
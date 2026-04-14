using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RG.DefinitionSystem.Core;
using RG.DefinitionSystem.UnityAdapter;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace RG.DefinitionSystem.Editor
{
    public sealed class DefinitionEntryAttributeProcessor : OdinAttributeProcessor<Definition>
    {
        private BaseScriptableDefinition mock;

        public override void ProcessChildMemberAttributes(InspectorProperty property, MemberInfo memberInfo, List<Attribute> attributes)
        {
            base.ProcessChildMemberAttributes(property, memberInfo, attributes);

            var parentType = property.BaseValueEntry.ParentType;
            if (!typeof(BaseScriptableDefinition).IsAssignableFrom(parentType))
                return;
            
            if (mock == null || mock.GetType() != parentType)
                mock = (BaseScriptableDefinition) Activator.CreateInstance(parentType);

            if (!attributes.Any(e => e is SerializeField))
                return;

            var name = memberInfo.Name;
            if (name.EndsWith("k__BackingField"))
                name = name.Substring(1, name.Length - 17);

            if (mock is IProcessSelfAttributes processSelfAttributes)
                attributes.AddRange(processSelfAttributes.ProcessSelfAttributes(name));
        }
    }
}
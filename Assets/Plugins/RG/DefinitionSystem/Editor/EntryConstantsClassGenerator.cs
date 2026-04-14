using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using RG.DefinitionSystem.Core;
using RG.DefinitionSystem.Core.Constants;
using RG.DefinitionSystem.UnityAdapter;
using UnityEditor;
using UnityEngine;

namespace RG.DefinitionSystem.Editor
{
    public sealed class EntryConstantsClassGenerator
    {
        private readonly BaseScriptableSourceDefinition sourceDefinition;

        public EntryConstantsClassGenerator(BaseScriptableSourceDefinition sourceDefinition)
        {
            this.sourceDefinition = sourceDefinition;
        }

        public void Generate()
        {
            var defType = sourceDefinition.DefinitionType;
            var script = AssetDatabase
                .FindAssets("t:script")
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault(e => Path.GetFileNameWithoutExtension(e) == defType.Name);

            if (script == null)
            {
                Debug.LogError(
                    $"Script file for {defType.Name} not found. Check if it exists and has the same name as the definition type.");
                return;
            }

            var className = EntryConstantsUtility.GetEntryName(sourceDefinition.DefinitionType);
            var path = $"{Path.GetDirectoryName(script)}\\{className}.cs";
            var provider = CodeDomProvider.CreateProvider("CSharp");

            using var file = File.CreateText(path);
            var codeUnit = GetClassConstants(sourceDefinition.DefinitionType, className);
            provider.GenerateCodeFromCompileUnit(codeUnit, file, new CodeGeneratorOptions());
            AssetDatabase.Refresh();
        }

        private CodeCompileUnit GetClassConstants(Type defType, string className)
        {
            var compileUnit = new CodeCompileUnit();

            var codeNamespace = new CodeNamespace(defType.Namespace);
            compileUnit.Namespaces.Add(codeNamespace);

            var entries = DefinitionDatabaseUtility.GetDefinitions(defType);

            var constantsClass = new CodeTypeDeclaration(className);
            constantsClass.IsClass = true;
            constantsClass.Attributes = MemberAttributes.Public | MemberAttributes.Static | MemberAttributes.Final;
            constantsClass.TypeAttributes =
                System.Reflection.TypeAttributes.Public | System.Reflection.TypeAttributes.Sealed;
            codeNamespace.Types.Add(constantsClass);

            var t = typeof(DefRef<>).MakeGenericType(defType);
            foreach (var entry in entries)
            {
                var field = new CodeMemberField(t, IdToName(entry.Id));
                field.Attributes = MemberAttributes.Public | MemberAttributes.Static;
                field.InitExpression = new CodePrimitiveExpression(entry.Id);
                constantsClass.Members.Add(field);
            }

            return compileUnit;

            string IdToName(string id)
            {
                return id.Trim()
                    .Replace(" ", "")
                    .Replace("-", "");
            }
        }
    }
}
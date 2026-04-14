using RG.DefinitionSystem.UnityAdapter;
using UnityEditor;

namespace RG.DefinitionSystem.Editor.Drawers
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BaseScriptableDefinition), true)]
    public sealed class ScriptableDefinitionDrawer : BaseScriptableDefinitionDrawer
    {
    }
}
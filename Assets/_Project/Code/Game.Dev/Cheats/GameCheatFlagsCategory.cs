using System.Linq;
using System.Reflection;
using Code.Prefs;
using UnityEngine;

namespace Code.Cheats
{
    public class GameCheatFlagsCategory : BaseEditorCheatCategory
    {
        private FieldInfo[] fields;

        public override void Draw()
        {
            var gameCheatFlags = Resolve<GameCheatFlags>();
            InitFields();

            foreach (var fieldInfo in fields)
            {
                var field = fieldInfo.GetValue(gameCheatFlags) as BoolPlayerPref;
                if (field == null)
                    continue;

                var fieldName = fieldInfo.Name;
                var fieldValue = field.Value;
#if UNITY_EDITOR
                field.Value = UnityEditor.EditorGUILayout.Toggle(fieldName, fieldValue);
#endif
            }
        }

        [QFSW.QC.Command("game.set-dev-flag")]
        private static void SetDevFlag([DevFlagName] string fieldName, bool value)
        {
            var fields = typeof(GameCheatFlags).GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.FieldType == typeof(BoolPlayerPref)).ToArray();
            var gameCheatFlags = Resolve<GameCheatFlags>();
            var fieldInfo = fields.FirstOrDefault(x => x.Name == fieldName);
            if (fieldInfo == null)
            {
                Debug.LogError($"Field {fieldName} not found");
                return;
            }
            var field = fieldInfo.GetValue(gameCheatFlags) as BoolPlayerPref;
            if (field == null)
            {
                Debug.LogError($"Field {fieldName} is not a BoolPlayerPref");
                return;
            }
            
            field.Value = value;
        }

        private void InitFields()
        {
            fields ??= typeof(GameCheatFlags).GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.FieldType == typeof(BoolPlayerPref)).ToArray();
        }
    }
}
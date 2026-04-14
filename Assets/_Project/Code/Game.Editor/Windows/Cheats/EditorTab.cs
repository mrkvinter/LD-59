using Code.Cheats;
using Code.Prefs;
using UnityEditor;
using UnityEngine;

namespace Code.Game.Editor.Windows.Cheats
{
    public class EditorTab
    {
        private BaseEditorCheatCategory[] editorCheatCategories;
        private BoolPlayerPref[] categoriesFoldouts;

        public void Draw()
        {
            InitEditor();

            if (GUILayout.Button("Open Persistent Path"))
            {
                var path = Application.persistentDataPath;
                Application.OpenURL(path);
            }

            for (var i = 0; i < editorCheatCategories.Length; i++)
            {
                var catName = editorCheatCategories[i].GetType().Name;
                var newState = DrawBigFoldout(categoriesFoldouts[i].Value, catName);
                if (newState != categoriesFoldouts[i].Value)
                {
                    categoriesFoldouts[i].Value = newState;
                }
                if (!categoriesFoldouts[i].Value) continue;

                var contentStyle = new GUIStyle("box")
                {
                    padding = new RectOffset(14, 10, 6, 8),
                    margin = new RectOffset(6, 6, 2, 6)
                };
                using (new EditorGUILayout.VerticalScope(contentStyle))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Space(6);
                        using (new EditorGUILayout.VerticalScope())
                        {
                            editorCheatCategories[i].Draw();
                        }
                    }
                }
            }
        }

        private static bool DrawBigFoldout(bool state, string title)
        {
            var bgStyle = new GUIStyle("box")
            {
                padding = new RectOffset(10, 10, 8, 8),
                margin = new RectOffset(4, 4, 6, 4)
            };

            var headerStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };

            const float arrowSize = 16f;
            var rect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true));
            rect.height = 28f;

            GUILayout.BeginHorizontal(bgStyle);
            GUILayout.Space(0);

            var arrowRect = GUILayoutUtility.GetRect(arrowSize, arrowSize, GUILayout.Width(arrowSize),
                GUILayout.Height(arrowSize));
            var arrow = state ? "▼" : "►";
            var arrowLabelStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
            GUI.Label(arrowRect, arrow, arrowLabelStyle);

            GUILayout.Space(6);
            GUILayout.Label(title, headerStyle, GUILayout.ExpandWidth(true));

            GUILayout.EndHorizontal();

            var clickRect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.MouseDown && clickRect.Contains(Event.current.mousePosition))
            {
                state = !state;
                Event.current.Use();
            }

            return state;
        }

        private void InitEditor()
        {
            if (editorCheatCategories != null) return;

            editorCheatCategories ??= new BaseEditorCheatCategory[]
            {
                new GameCheatFlagsCategory(),
                new CurrentSaveCheatCategory()
            };

            categoriesFoldouts ??= new BoolPlayerPref[editorCheatCategories.Length];
            for (int i = 0; i < editorCheatCategories.Length; i++)
            {
                if (categoriesFoldouts[i] == null)
                {
                    var catName = editorCheatCategories[i].GetType().Name;
                    var key = $"Cheats/EditorTab/Foldout/{catName}";
                    categoriesFoldouts[i] = new BoolPlayerPref(key, false);
                }
            }
        }
    }
}
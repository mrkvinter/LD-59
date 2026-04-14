using System;
using System.Linq;
using Code.Cheats;
using UnityEditor;
using UnityEngine;

namespace Code.Game.Editor.Windows.Cheats
{
    public class CheatsWindow : EditorWindow
    {
        [MenuItem("Game/Cheats Panel")]
        public static void Open()
        {
            var window = GetWindow<CheatsWindow>();
            window.titleContent = new GUIContent("Cheats Panel");
            window.Show();
        }

        private readonly MainCheatsTab mainCheatsTab = new();
        private readonly GameCheatsTab gameCheatsTab = new();
        private readonly EditorTab editorTab = new();
        private CheatCategory currentCategory;

        private Vector2 _scrollPos;
        private void OnGUI()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                DrawTabs();
                
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
                Action drawer = currentCategory switch
                {
                    CheatCategory.Main => mainCheatsTab.Draw,
                    CheatCategory.GameCheats => gameCheatsTab.Draw,
                    CheatCategory.EditorCheats => editorTab.Draw,
                    _ => throw new ArgumentOutOfRangeException()
                };
                
                drawer();
                EditorGUILayout.EndScrollView();
            }
        }
        
        private void DrawTabs()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var categories = Enum.GetValues(typeof(CheatCategory)).Cast<CheatCategory>().ToArray();

                foreach (var category in categories)
                {
                    var style = new GUIStyle(EditorStyles.toolbarButton)
                    {
                        fixedHeight = 30
                    };
                    if (currentCategory == category)
                    {
                        style.fontStyle = FontStyle.Bold;
                        style.normal.textColor = EditorStyles.toolbarButton.normal.textColor;
                    }

                    if (GUILayout.Button(category.ToString(), style))
                    {
                        currentCategory = category;
                    }
                }
            }
        }

        private enum CheatCategory
        {
            Main,
            GameCheats,
            EditorCheats
        }
    }
}
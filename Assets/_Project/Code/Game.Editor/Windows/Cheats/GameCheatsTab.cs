using Code.Cheats;
using UnityEditor;
using UnityEngine;

namespace Code.Game.Editor.Windows.Cheats
{
    public class GameCheatsTab
    {
        private ICheatCategory[] cheatCategories;
        private int? currentCategoryIndex;

        public void Draw()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("You need to be in play mode to use cheats", MessageType.Warning);
                return;
            }

            InitGameCheats();
            if (currentCategoryIndex.HasValue)
            {
                if (GUILayout.Button("<<< Back"))
                {
                    currentCategoryIndex = null;
                    return;
                }

                cheatCategories[currentCategoryIndex.Value].Draw();
            }
            else
            {
                DrawCategories();
            }
        }

        private void DrawCategories()
        {
            for (var i = 0; i < cheatCategories.Length; i++)
            {
                if (GUILayout.Button(cheatCategories[i].GetType().Name))
                {
                    currentCategoryIndex = i;
                }
            }
        }

        private void InitGameCheats()
        {
            cheatCategories ??= new ICheatCategory[]
            {
                new InputInfoCheatCategory()
            };
        }
    }
}

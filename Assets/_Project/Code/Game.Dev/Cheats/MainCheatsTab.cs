using System;
using System.Globalization;
using UnityEngine;

namespace Code.Cheats
{
    public class MainCheatsTab
    {
        private readonly float[] timeScales = { 0.1f, 0.5f, 1f, 2f, 5f, 10f };
        private readonly int[] fpsLimits = { 5, 10, 15, 30, 60, -1 };

        public void Draw()
        {
            var currentTimeScale = Time.timeScale;
            var currentIndex = Array.IndexOf(timeScales, currentTimeScale);
            GUILayout.Label("Time scale: ");
            var timeScaleLabels = Array.ConvertAll(timeScales, t => t.ToString(CultureInfo.InvariantCulture));
            var newIndex = GUILayout.Toolbar(currentIndex, timeScaleLabels, GUILayout.Width(timeScales.Length * 30));
            if (newIndex != currentIndex)
                Time.timeScale = timeScales[newIndex];

            GUILayout.Space(8);

            var currentFps = Application.targetFrameRate;
            var currentFpsIndex = Array.IndexOf(fpsLimits, currentFps);
            GUILayout.Label("FPS limit: ");
            var fpsLabels = Array.ConvertAll(fpsLimits, f => f == -1 ? "∞" : f.ToString());
            var newFpsIndex = GUILayout.Toolbar(currentFpsIndex, fpsLabels, GUILayout.Width(fpsLimits.Length * 30));
            if (newFpsIndex != currentFpsIndex)
            {
                Application.targetFrameRate = fpsLimits[newFpsIndex];
                QualitySettings.vSyncCount = fpsLimits[newFpsIndex] == -1 ? 1 : 0;
            }

            if (GUILayout.Button("Restart Game"))
            {
                // DiceGameContext.Resolve<IGameDirector>().RestartGame();
            }
        }
    }
}
using System.Collections.Generic;
using System.IO;
using Code.Game.Utilities.EditorPrefs;
using Code.Prefs;
using UnityEngine;

namespace Game.Scripts.SaveLoadSystem
{
    public class DebugSaveLoadService
    {
        private string editorPath => $"{Application.persistentDataPath}/Editor";
        public string SavePath => $"{editorPath}/Save";

        public readonly StringEditorPref CurrentSaveNamePref = new("CurrentSaveName");
        public readonly BoolEditorPref StartWithEmptySavePref = new("StartWithEmptySave");

        public List<string> GetAllSaves()
        {
            var saves = new List<string>();
            if (!Directory.Exists(SavePath))
            {
                Directory.CreateDirectory(SavePath);
                return saves;
            }

            var files = Directory.GetFiles(SavePath, "*.sav", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                saves.Add(file);
            }

            return saves;
        }

        public void ReplaceSaveIfNeeded()
        {
            if (StartWithEmptySavePref.Value)
            {
                var emptySaveFilePath = $"{Application.persistentDataPath}/MainSave.sav";
                if (File.Exists(emptySaveFilePath))
                {
                    File.Delete(emptySaveFilePath);
                }
            }

            if (CurrentSaveNamePref.Value != null)
            {
                var saveName = CurrentSaveNamePref.Value;
                if (File.Exists(saveName))
                {
                    var oldSaveFilePath = $"{Application.persistentDataPath}/MainSave.sav";
                    File.Copy(saveName, oldSaveFilePath, true);
                }
            }
        }
    }
}
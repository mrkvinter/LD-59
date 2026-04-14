using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.Scripts.SaveLoadSystem
{
    public class SaveLoadService : ISaveLoadService
    {
        private const int Precision = 100;

        private readonly JsonSerializer serializer;

        public SaveLoadService()
        {
            var serializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            serializer = JsonSerializer.Create(serializerSettings);

            serializer.Converters.Add(new Convertors.Vector2Convertor(Precision));
            serializer.Converters.Add(new Convertors.Vector3Convertor(Precision));
            serializer.Converters.Add(new Convertors.QuaternionConvertor(Precision));
        }

        public bool HasSave(string slotName)
        {
            if (string.IsNullOrEmpty(slotName))
                slotName = "autosave";

            var path = $"{Application.persistentDataPath}/{slotName}.sav";
            return File.Exists(path);
        }

        public void Save(string slotName, GameSaveData gameSaveData)
        {
            if (string.IsNullOrEmpty(slotName))
                slotName = "autosave";

            var path = $"{Application.persistentDataPath}/{slotName}.sav";
            using var stream = File.CreateText(path);
            serializer.Serialize(stream, gameSaveData);
        }

        public GameSaveData Load(string slotName)
        {
            if (string.IsNullOrEmpty(slotName))
                slotName = "autosave";

            var path = $"{Application.persistentDataPath}/{slotName}.sav";

            if (File.Exists(path))
            {
                using var stream = File.OpenText(path);
                return (GameSaveData) serializer.Deserialize(stream, typeof(GameSaveData));
            }

            return null;
        }
    }
}
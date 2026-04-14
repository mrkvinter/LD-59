using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.Scripts.SaveLoadSystem
{
    public class PrefsSaveLoadService : ISaveLoadService
    {
        private const int Precision = 100;
        private const string SaveKey = "GameSaveData";

        private readonly JsonSerializer serializer;

        public PrefsSaveLoadService()
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
            return PlayerPrefs.HasKey(SaveKey);
        }

        public void Save(string slotName, GameSaveData gameSaveData)
        {
            var json = GenerateJson(gameSaveData);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }

        public GameSaveData Load(string slotName)
        {
            if (PlayerPrefs.HasKey(SaveKey))
            {
                var json = PlayerPrefs.GetString(SaveKey);
                return DeserializeJson(json);
            }

            return null;
        }

        private string GenerateJson(GameSaveData gameSaveData)
        {
            using var stringWriter = new StringWriter();
            using var jsonWriter = new JsonTextWriter(stringWriter);
            serializer.Serialize(jsonWriter, gameSaveData);
            return stringWriter.ToString();
        }

        private GameSaveData DeserializeJson(string json)
        {
            using var stringReader = new StringReader(json);
            using var jsonReader = new JsonTextReader(stringReader);
            return serializer.Deserialize<GameSaveData>(jsonReader);
        }
    }
}
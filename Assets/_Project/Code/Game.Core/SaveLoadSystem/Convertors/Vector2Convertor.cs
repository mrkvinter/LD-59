using Newtonsoft.Json;
using UnityEngine;

namespace Game.Scripts.SaveLoadSystem.Convertors
{
    public class Vector2Convertor : JsonConverter<Vector2>
    {
        private readonly int precision;
        
        public Vector2Convertor(int precision = 1000)
        {
            this.precision = precision;
        }
        
        public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
        {
            var x = Mathf.RoundToInt(value.x * precision);
            var y = Mathf.RoundToInt(value.y * precision);
            var serializedString = $"{x}#{y}";
            writer.WriteValue(serializedString);
        }

        public override Vector2 ReadJson(JsonReader reader, System.Type objectType, Vector2 existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var serializedString = (string)reader.Value;
            var split = serializedString.Split('#');
            var x = int.Parse(split[0]);
            var y = int.Parse(split[1]);
            return new Vector2(x / (float)precision, y / (float)precision);
        }
    }
}
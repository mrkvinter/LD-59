using Newtonsoft.Json;
using UnityEngine;

namespace Game.Scripts.SaveLoadSystem.Convertors
{
    public class Vector3Convertor : JsonConverter<Vector3>
    {
        private readonly int precision;
        
        public Vector3Convertor(int precision = 1000)
        {
            this.precision = precision;
        }
        
        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
        {
            var x = Mathf.RoundToInt(value.x * precision);
            var y = Mathf.RoundToInt(value.y * precision);
            var z = Mathf.RoundToInt(value.z * precision);
            var serializedString = $"{x}#{y}#{z}";
            writer.WriteValue(serializedString);
        }

        public override Vector3 ReadJson(JsonReader reader, System.Type objectType, Vector3 existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var serializedString = (string)reader.Value;
            var split = serializedString.Split('#');
            var x = int.Parse(split[0]);
            var y = int.Parse(split[1]);
            var z = int.Parse(split[2]);
            return new Vector3(x / (float)precision, y / (float)precision, z / (float)precision);
        }
    }
}
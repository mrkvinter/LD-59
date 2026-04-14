using Newtonsoft.Json;
using UnityEngine;

namespace Game.Scripts.SaveLoadSystem.Convertors
{
    public class QuaternionConvertor : JsonConverter<Quaternion>
    {
        private readonly int precision;
        
        public QuaternionConvertor(int precision = 1000)
        {
            this.precision = precision;
        }
        
        public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
        {
            var x = Mathf.RoundToInt(value.x * precision);
            var y = Mathf.RoundToInt(value.y * precision);
            var z = Mathf.RoundToInt(value.z * precision);
            var w = Mathf.RoundToInt(value.w * precision);
            var serializedString = $"{x}#{y}#{z}#{w}";
            writer.WriteValue(serializedString);
        }

        public override Quaternion ReadJson(JsonReader reader, System.Type objectType, Quaternion existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var serializedString = (string)reader.Value;
            var split = serializedString.Split('#');
            var x = int.Parse(split[0]);
            var y = int.Parse(split[1]);
            var z = int.Parse(split[2]);
            var w = int.Parse(split[3]);
            return new Quaternion(x / (float)precision, y / (float)precision, z / (float)precision, w / (float)precision);
        }
    }
}
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace lib
{
    public static class JsonExtensions
    {
        private static readonly JsonSerializerSettings Settings;

        static JsonExtensions()
        {
            Settings = new JsonSerializerSettings();
            Settings.Converters.Add(new VJsonConverter());
            Settings.ContractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };
            Settings.NullValueHandling = NullValueHandling.Ignore;

        }

        public static string ToJson<T>(this T obj)
        {
            return JsonConvert.SerializeObject(obj, Settings);
        }

        public static T FromJson<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json, Settings) ?? throw new FormatException("Cant be null");
        }
    }

    public class VJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            V v = (V)(value ?? throw new Exception("V cant be null"));
            writer.WriteStartArray();
            writer.WriteValue(v.X);
            writer.WriteValue(v.Y);
            writer.WriteEndArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var xy = serializer.Deserialize<int[]>(reader) ?? throw  new FormatException("V");
            if (xy.Length != 2) throw new FormatException(xy.StrJoin(" "));
            return new V(xy[0], xy[1]);
        }

        public override bool CanConvert(Type objectType) =>
            objectType == typeof(V);
    }

}

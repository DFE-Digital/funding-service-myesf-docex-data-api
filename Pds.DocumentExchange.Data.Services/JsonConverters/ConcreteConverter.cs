using Newtonsoft.Json;
using System;

namespace Pds.DocumentExchange.Data.Services.JsonConverters
{
    /// <summary>
    /// Generic JSON converter.
    /// </summary>
    /// <typeparam name="T">The generic object.</typeparam>
    public class ConcreteConverter<T> : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType) => true;

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize<T>(reader);
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
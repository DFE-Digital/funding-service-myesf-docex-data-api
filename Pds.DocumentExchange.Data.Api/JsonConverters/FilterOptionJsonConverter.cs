using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pds.DocumentExchange.Data.Api.Enums;
using Pds.DocumentExchange.Data.Api.Models.Filters;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Pds.DocumentExchange.Data.Api.JsonConverters
{
    /// <summary>
    /// The filter option JSON converter.
    /// </summary>
    public class FilterOptionJsonConverter : JsonConverter<IFilterOption>
    {
        /// <inheritdoc/>
        public override IFilterOption ReadJson(JsonReader reader, Type objectType, [AllowNull] IFilterOption existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var filterOptionType = GetFilterOptionTypeFromJsonObject(jsonObject);

            if (filterOptionType.HasValue)
            {
                return CreateFilterOptionFromJsonObject(filterOptionType.Value, jsonObject, serializer);
            }

            throw new JsonSerializationException("The filter option type is invalid.");
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, [AllowNull] IFilterOption value, JsonSerializer serializer)
            => throw new NotImplementedException();

        /// <inheritdoc/>
        public override bool CanWrite
            => false;

        private FilterOptionType? GetFilterOptionTypeFromJsonObject(JObject jsonObject)
        {
            var tokenExists = jsonObject.TryGetValue(nameof(IFilterOption.Type), StringComparison.OrdinalIgnoreCase, out JToken? jToken);

            if (!tokenExists)
            {
                return null;
            }

            return Enum.TryParse(jToken.ToString(), true, out FilterOptionType result)
                ? result
                : default(FilterOptionType?);
        }

        private IFilterOption CreateFilterOptionFromJsonObject(FilterOptionType filterOptionType, JObject jsonObject, JsonSerializer serializer)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .First(p => typeof(IFilterOption).IsAssignableFrom(p) && p.Name == filterOptionType.ToString());

            var filterOption = (IFilterOption)Activator.CreateInstance(type);
            serializer.Populate(jsonObject.CreateReader(), filterOption);

            return filterOption;
        }
    }
}
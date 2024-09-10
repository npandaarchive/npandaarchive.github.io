using System.Buffers;
using System.Globalization;
using System.Numerics;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Collections.Extensions;

namespace HentaiMapGen
{

    public class CastingNumberConverter<T> : JsonConverter<T> where T : INumber<T>
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (reader.TokenType)
            {
                case JsonTokenType.Number when reader.HasValueSequence && reader.ValueSequence.Length < 512:
                {
                    Span<byte> value = stackalloc byte[(int)reader.ValueSequence.Length];
                    reader.ValueSequence.CopyTo(value);
                    return T.Parse(value, CultureInfo.InvariantCulture);
                }
                case JsonTokenType.Number when reader.HasValueSequence:
                    return T.Parse(reader.ValueSequence.ToArray(), CultureInfo.InvariantCulture);
                case JsonTokenType.Number:
                    return T.Parse(reader.ValueSpan, CultureInfo.InvariantCulture);
                case JsonTokenType.String:
                    return T.Parse(reader.GetString(), CultureInfo.InvariantCulture);
                default:
                {
                    using var document = JsonDocument.ParseValue(ref reader);
                    return T.Parse(document.RootElement.ToString(), CultureInfo.InvariantCulture);
                }
            }
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    public class UnixDateTimeConverter : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            return DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64());
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.ToUnixTimeSeconds());
        }
    }

    public class EnumMemberConverter<T, TUnderlying> : JsonConverter<T>
        where T : struct, Enum
        where TUnderlying : IBinaryNumber<TUnderlying>, IMinMaxValue<TUnderlying>
    {
        private static readonly T[] EnumValues = Enum.GetValues<T>();

        // ReSharper disable once StaticMemberInGenericType
        private static readonly string[] EnumMembers = EnumValues.Select(e => GetAttributeOfType<EnumMemberAttribute>(e)!.Value!).ToArray();

        static EnumMemberConverter()
        {
            if (Enum.GetUnderlyingType(typeof(T)) != typeof(TUnderlying))
            {
                throw new InvalidOperationException(
                    $"Type mismatch: {nameof(TUnderlying)} is {typeof(TUnderlying)} but should be {Enum.GetUnderlyingType(typeof(T))}"
                );
            }
        }

        // https://stackoverflow.com/a/9276348
        /// <summary>
        /// Gets an attribute on an enum field value
        /// </summary>
        /// <typeparam name="TAttr">The type of the attribute you want to retrieve</typeparam>
        /// <param name="enumVal">The enum value</param>
        /// <returns>The attribute of type T that exists on the enum value</returns>
        /// <example><![CDATA[string desc = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;]]></example>
        private static TAttr? GetAttributeOfType<TAttr>(T enumVal) where TAttr : Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(TAttr), false);
            return (attributes.Length > 0) ? (TAttr)attributes[0] : null;
        }
        
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            for (var index = 0; index < EnumMembers.Length; index++)
            {
                var enumMember = EnumMembers[index];
                if (string.Equals(str, enumMember))
                {
                    return EnumValues[index];
                }
            }

            return (T)(object)TUnderlying.MaxValue;

            throw new JsonException($"Could not convert enum value {str} to {typeof(T)}");
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(EnumMembers[int.CreateChecked((TUnderlying)(object)value)]);
        }
    }

    public class DictionarySlimConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsConstructedGenericType &&
                   typeToConvert.GetGenericTypeDefinition() == typeof(DictionarySlim<,>);
        }

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return (JsonConverter?)Activator.CreateInstance(typeof(DictionarySlimConverter<,>).MakeGenericType(typeToConvert.GetGenericArguments()));
        }
    }

    public class DictionarySlimConverter<TK, TV> : JsonConverter<DictionarySlim<TK, TV>> where TK : IEquatable<TK>
    {
        public override bool HandleNull => true;

        public override DictionarySlim<TK, TV>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }
            
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException($"JsonTokenType was of type {reader.TokenType}, expected {nameof(JsonTokenType.StartObject)}");
            }

            var keyConverter = (JsonConverter<TK>)options.GetConverter(typeof(TK));
            var valueConverter = (JsonConverter<TV>)options.GetConverter(typeof(TV));

            var dictionary = new DictionarySlim<TK, TV>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return dictionary;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException($"JsonTokenType was of type {reader.TokenType}, expected {nameof(JsonTokenType.PropertyName)}");
                }

                var k = keyConverter.ReadAsPropertyName(ref reader, typeof(TK), options);

                reader.Read();
                
                var v = valueConverter.Read(ref reader, typeof(TV), options);

                dictionary.GetOrAddValueRef(k) = v;
            }

            return dictionary;
        }

        public override void Write(Utf8JsonWriter writer, DictionarySlim<TK, TV>? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }
            
            writer.WriteStartObject();
            
            var keyConverter = (JsonConverter<TK>)options.GetConverter(typeof(TK));
            var valueConverter = (JsonConverter<TV>)options.GetConverter(typeof(TV));
            foreach (var (k, v) in value)
            {
                keyConverter.WriteAsPropertyName(writer, k, options);
                valueConverter.Write(writer, v, options);
            }
            
            writer.WriteEndObject();
        }
    }
    
    
}
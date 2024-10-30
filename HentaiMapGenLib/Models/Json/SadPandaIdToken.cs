using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using MessagePack;

namespace HentaiMapGenLib.Models.Json;

[MessagePackObject]
[JsonConverter(typeof(Converter))]
public readonly partial record struct SadPandaIdToken(
    [property: Key(0), JsonInclude, JsonPropertyName("gid")] uint Gid,
    [property: Key(1), JsonIgnore] ulong Token)
{
    public class Converter : JsonConverter<SadPandaIdToken>
    {
        public override SadPandaIdToken Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException($"JsonTokenType was of type {reader.TokenType}, expected {nameof(JsonTokenType.StartObject)}");
            }

            uint? gid = null;
            ulong? token = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return new SadPandaIdToken(gid!.Value, token!.Value);
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException($"JsonTokenType was of type {reader.TokenType}, expected {nameof(JsonTokenType.PropertyName)}");
                }

                var key = reader.GetString();

                reader.Read();

                switch (key)
                {
                    case "gid":
                        gid = reader.GetUInt32();
                        break;
                    case "token":
                        token = ulong.Parse(reader.GetString()!, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        break;
                    default:
                        throw new JsonException($"Expected 'gid' or 'token' but was '{key}'");
                }
            }

            return new SadPandaIdToken(gid!.Value, token!.Value);
        }

        public override void Write(Utf8JsonWriter writer, SadPandaIdToken value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("gid");
            writer.WriteNumberValue(value.Gid);
            writer.WritePropertyName("token");
            writer.WriteStringValue(value.TokenHex);
            writer.WriteEndObject();
        }
    }

    [JsonInclude, JsonPropertyName("token"), IgnoreMember]
    public string TokenHex => Token.ToString("x10");

    public SadPandaIdToken(long gid, string token)
        : this((uint)gid, ulong.Parse(token, NumberStyles.HexNumber, CultureInfo.InvariantCulture))
    {
    }

    [method: JsonConstructor]
    public SadPandaIdToken(uint gid, string token)
        : this(gid, ulong.Parse(token, NumberStyles.HexNumber, CultureInfo.InvariantCulture))
    {
    }

}

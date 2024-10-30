using System.Text.Json.Serialization;
using HentaiMapGen;

namespace HentaiMapGenLib.Models.Json;

public readonly record struct DeletedGallery(
    [property: JsonPropertyName("Code")] [property: JsonConverter(typeof(CastingNumberConverter<uint>))] uint Id,
    [property: JsonPropertyName("Title")] string TitlePretty,
    [property: JsonPropertyName("FullTitle")] string TitleEnglish,
    [property: JsonPropertyName("Artists")] string[] Artists,
    [property: JsonPropertyName("Groups")] string[] Groups,
    [property: JsonPropertyName("Parodies")] string[] Parodies,
    [property: JsonPropertyName("Characters")] string[] Characters,
    [property: JsonPropertyName("Pages")] [property: JsonConverter(typeof(CastingNumberConverter<uint>))] uint Pages,
    [property: JsonPropertyName("UploadDate")] DateTimeOffset UploadDate
);
using System.Text.Json.Serialization;
using HentaiMapGen;
using Microsoft.Collections.Extensions;

namespace HentaiMapGenLib.Models.Json;
[JsonSourceGenerationOptions(WriteIndented = false, Converters = [typeof(DictionarySlimConverterFactory)], DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(SadPandaIdToken))]
[JsonSerializable(typeof(DeletedGallery))]
[JsonSerializable(typeof(Title))]
[JsonSerializable(typeof(Book))]
[JsonSerializable(typeof(Page))]
[JsonSerializable(typeof(ImageType))]
[JsonSerializable(typeof(Images))]
[JsonSerializable(typeof(Tag))]
[JsonSerializable(typeof(Dictionary<ulong, SadPandaIdToken>), TypeInfoPropertyName = "UlongSadPandaIdTokenDictionary")]
[JsonSerializable(typeof(Dictionary<uint, SadPandaIdToken>), TypeInfoPropertyName = "UintSadPandaIdTokenDictionary")]
[JsonSerializable(typeof(Dictionary<uint, Title>), TypeInfoPropertyName = "UintTitleDictionary")]
[JsonSerializable(typeof(DictionarySlim<ulong, SadPandaIdToken>), TypeInfoPropertyName = "UlongSadPandaIdTokenDictionarySlim")]
[JsonSerializable(typeof(DictionarySlim<uint, SadPandaIdToken>), TypeInfoPropertyName = "UintSadPandaIdTokenDictionarySlim")]
[JsonSerializable(typeof(DictionarySlim<uint, Title>), TypeInfoPropertyName = "UintTitleDictionarySlim")]
[JsonSerializable(typeof(List<uint>), TypeInfoPropertyName = "UintList")]
[JsonSerializable(typeof(DeletedGallery[]))]
[JsonSerializable(typeof(NHentaiClient.SearchResults))]
[JsonSerializable(typeof(NHentaiClient.BookRecommend))]
public partial class NHentaiSerializer : JsonSerializerContext;
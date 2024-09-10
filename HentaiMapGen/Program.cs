// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.Hashing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using DataModel;
using Google.Protobuf;
using HentaiMapGen;
using HentaiMapGen.Proto;
using LinqToDB;
using LinqToDB.Common;
using Microsoft.Collections.Extensions;
using Microsoft.Data.Sqlite;
using ZstdSharp;

Console.WriteLine("Hello, World!");
var jsonOpts = new JsonSerializerOptions
{
    Converters =
    {
        new DictionarySlimConverterFactory(),
    },
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
};

Dictionary<uint, DeletedGallery> deletedGalleries;
await using (var stream = File.OpenRead(@"..\..\..\..\removed_galleries.json"))
{
    deletedGalleries = JsonSerializer.Deserialize<DeletedGallery[]>(stream)!
        .ToDictionary(e => e.Code);
}

Console.WriteLine("Loaded deletedGalleries");

await using var pandaDb = new SadPandaDb(
    new DataOptions<SadPandaDb>(new DataOptions()
        .UseSQLite($@"Data Source={args[0]}")));

#region Create sadPandaNameHashes

DictionarySlim<ulong, SadPandaUrlParts> sadPandaNameHashes;
if (!File.Exists("../../../sadPandaNameHashes.json.zst"))
{
    sadPandaNameHashes = new DictionarySlim<ulong, SadPandaUrlParts>();
    var sw0 = Stopwatch.GetTimestamp();

    foreach (var e in pandaDb.Galleries.Select(e => new { e.Title, e.TitleJpn, e.Gid, e.Token }))
    {
        var urlParts = SadPandaUrlParts.Parse(e.Gid, e.Token!);

        if (e.Title != null)
        {
            sadPandaNameHashes.GetOrAddValueRef(Hash(Normalize(e.Title))) = urlParts;
            sadPandaNameHashes.GetOrAddValueRef(Hash(Normalize(Cleanup(e.Title)))) = urlParts;
        }

        if (e.TitleJpn != null)
        {
            sadPandaNameHashes.GetOrAddValueRef(Hash(Normalize(e.TitleJpn))) = urlParts;
            sadPandaNameHashes.GetOrAddValueRef(Hash(Normalize(Cleanup(e.TitleJpn)))) = urlParts;
        }
    }

    Console.WriteLine($"Created sadPandaNameHashes in {Stopwatch.GetElapsedTime(sw0)}. Total {sadPandaNameHashes.Count} sadpanda name hashes.");

    await using var stream = new CompressionStream(File.Create("../../../sadPandaNameHashes.json.zst"), 19, leaveOpen: false);
    await JsonSerializer.SerializeAsync(stream, sadPandaNameHashes, jsonOpts);
}
else
{
    await using var stream = new DecompressionStream(File.OpenRead("../../../sadPandaNameHashes.json.zst"), leaveOpen: false);
    sadPandaNameHashes = (await JsonSerializer.DeserializeAsync<DictionarySlim<ulong, SadPandaUrlParts>>(stream, jsonOpts))!;

    Console.WriteLine("Loaded sadPandaNameHashes from cache");
}

#endregion

// linq2db shits itself at attempting to parse the manga.db because of sqlite's lack of type checking since the value
// column contains strings typed as BLOB
await using var nhentaiDb = new SqliteConnection($@"Data Source={args[1]}");
await nhentaiDb.OpenAsync();

await using var protobufOutput = new CompressionStream(File.Create("../../../galleries.bin.zst"), 15, leaveOpen: false);

var protobufTotal = TimeSpan.Zero;
var mappingTotal = TimeSpan.Zero;
var newDbTotal = TimeSpan.Zero;

// Mapping of NHentai ID -> SadPanda Gallery ID and Token
var nhentaiMapping = new DictionarySlim<uint, SadPandaUrlParts>();

// Mapping of NHentai ID -> NHentai Title, for when no match was found
var naGalleries = new DictionarySlim<uint, Title>();

// Galleries that returned an error (probably removed by NHentai) and were not matched in removed_galleries.json
var erroredGalleries = new List<uint>();

var sw4 = Stopwatch.GetTimestamp();
await using (var command = nhentaiDb.CreateCommand())
{
    command.CommandText = "SELECT key, value FROM unnamed";

    await using var reader = await command.ExecuteReaderAsync();
    var i = 0;
    while (await reader.ReadAsync())
    {
        var galleryId = uint.Parse(reader.GetString(0));
        var value = reader.GetString(1);

        try
        {
            if (i++ % 1000 == 0) Console.WriteLine($"{i}");

            var nGallery = JsonSerializer.Deserialize<Book>(value.AsSpan().RemovePrefix("json:"));

            var sw1 = Stopwatch.GetTimestamp();
            #region Write protobuf
            var bookList = new NHentai.Types.BookList();
            bookList.Books.Add(nGallery.ToProtobuf(galleryId));
            bookList.WriteTo(protobufOutput);
            #endregion
            protobufTotal += Stopwatch.GetElapsedTime(sw1);

            SadPandaUrlParts pandaId = default;
            DeletedGallery deletedGallery = default;

            var sw2 = Stopwatch.GetTimestamp();
            #region Map Nhentai to sadpanda
            bool TryMatchAnyHash(ReadOnlySpan<string?> candidates, out SadPandaUrlParts parts)
            {
                foreach (var candidate in candidates)
                {
                    if (candidate == null) continue;

                    if (sadPandaNameHashes.TryGetValue(Hash(Normalize(candidate)), out parts)) return true;
                    if (sadPandaNameHashes.TryGetValue(Hash(Normalize(Cleanup(candidate))), out parts)) return true;
                    if (sadPandaNameHashes.TryGetValue(Hash(Normalize(ReplaceRoman(Cleanup(candidate)))), out parts)) return true;
                }

                parts = default;
                return false;
            }

            static bool CheckForAnyMatch(Gallery pGallery, ReadOnlySpan<string?> candidates)
            {
                var normTitle = pGallery.Title != null ? Normalize(pGallery.Title) : null;
                var cleanedNormTitle = pGallery.Title != null ? Normalize(Cleanup(pGallery.Title)) : null;
                var cleanedNormTitleRoman = pGallery.Title != null ? Normalize(ReplaceRoman(Cleanup(pGallery.Title))) : null;

                var normTitleJpn = pGallery.TitleJpn != null ? Normalize(pGallery.TitleJpn) : null;
                var cleanedNormTitleJpn = pGallery.TitleJpn != null ? Normalize(Cleanup(pGallery.TitleJpn)) : null;
                var cleanedNormTitleRomanJpn = pGallery.TitleJpn != null ? Normalize(ReplaceRoman(Cleanup(pGallery.TitleJpn))) : null;
                
                foreach (var candidate in candidates)
                {
                    if (candidate == null) continue;

                    var normCandidate = Normalize(candidate);
                    var cleanedNormCandidate = Normalize(Cleanup(candidate));
                    var cleanedNormRomanCandidate = Normalize(ReplaceRoman(Cleanup(candidate)));

                    if (normTitle != null && normCandidate.SequenceEqual(normTitle)) return true;
                    if (normTitleJpn != null && normCandidate.SequenceEqual(normTitleJpn)) return true;
                    if (cleanedNormTitle != null && cleanedNormCandidate.SequenceEqual(cleanedNormTitle)) return true;
                    if (cleanedNormTitleJpn != null && cleanedNormCandidate.SequenceEqual(cleanedNormTitleJpn)) return true;
                    if (cleanedNormTitleRoman != null && cleanedNormRomanCandidate.SequenceEqual(cleanedNormTitleRoman)) return true;
                    if (cleanedNormTitleRomanJpn != null && cleanedNormRomanCandidate.SequenceEqual(cleanedNormTitleJpn)) return true;
                }

                return false;
            }

            if (nGallery.Error != null)
            {
                if (!deletedGalleries.TryGetValue(galleryId, out deletedGallery))
                {
                    erroredGalleries.Add(galleryId);
                    continue;
                }

                if (TryMatchAnyHash([deletedGallery.FullTitle, deletedGallery.Title], out pandaId))
                {
                    var gid = pandaId.Gid;
                    var pGallery = pandaDb.Galleries.FirstOrDefault(e => e.Gid == gid)!;
                    if (!CheckForAnyMatch(pGallery, [deletedGallery.FullTitle, deletedGallery.Title]))
                    {
                        throw new InvalidOperationException(
                            $"Hash collision! {pGallery.Title} vs {deletedGallery.Title}");
                    }

                    // var token = pandaId.TokenHex;
                    // Console.WriteLine($"{deletedGallery.Title}: https://exhentai.org/g/{gid}/{token}/");

                    nhentaiMapping.GetOrAddValueRef(galleryId) = pandaId;
                }
                else
                {
                    Console.WriteLine($"{deletedGallery.Title}: N/A");
                    naGalleries.GetOrAddValueRef(galleryId) =
                        new Title(deletedGallery.FullTitle, null, deletedGallery.Title);
                }
            }
            else
            {
                if (TryMatchAnyHash([nGallery.Title.English, nGallery.Title.Japanese, nGallery.Title.Pretty], out pandaId))
                {
                    var gid = pandaId.Gid;
                    var pGallery = pandaDb.Galleries.Find(gid)!;
                    if (!CheckForAnyMatch(pGallery,[nGallery.Title.English, nGallery.Title.Japanese, nGallery.Title.Pretty]))
                    {
                        throw new InvalidOperationException(
                            $"Hash collision! {pGallery.Title} vs {nGallery.Title.English}");
                    }

                    // var token = pandaId.TokenHex;
                    // Console.WriteLine($"{nGallery.Title.Pretty}: https://exhentai.org/g/{gid}/{token}/");

                    nhentaiMapping.GetOrAddValueRef(galleryId) = pandaId;
                }
                else
                {
                    Console.WriteLine($"{nGallery.Title.Pretty}: N/A");
                    naGalleries.GetOrAddValueRef(galleryId) = nGallery.Title;
                }
            }
            #endregion
            mappingTotal += Stopwatch.GetElapsedTime(sw2);
        }
        catch (Exception)
        {
            Console.WriteLine($"ERRORED when processing gallery {galleryId}: {value}");
            throw;
        }
    }
}

Console.WriteLine($"Total time: {Stopwatch.GetElapsedTime(sw4)}");
Console.WriteLine($"Protobuf took {protobufTotal}");
Console.WriteLine($"Mapping nhentai->panda took {mappingTotal}");
Console.WriteLine($"Creating new DB schema took {newDbTotal}");
Console.WriteLine($"Found NHentai->Panda mappings: {nhentaiMapping.Count}");
Console.WriteLine($"Not found NHentai->Panda mappings: {naGalleries.Count}");
Console.WriteLine($"Errored (deleted, etc...) NHentai galleries: {erroredGalleries.Count}");

await using (var stream = File.Create("../../../nhentaiMapping.json"))
{
    await JsonSerializer.SerializeAsync(stream, nhentaiMapping, jsonOpts);
}
await using (var stream = File.Create("../../../naGalleries.json"))
{
    await JsonSerializer.SerializeAsync(stream, naGalleries, jsonOpts);
}
await using (var stream = File.Create("../../../erroredGalleries.json"))
{
    await JsonSerializer.SerializeAsync(stream, erroredGalleries, jsonOpts);
}
return;

static ReadOnlySpan<char> Normalize(string str)
    => str.ToLowerInvariant().Normalize(NormalizationForm.FormD).AsSpan().Trim();

static ulong Hash(ReadOnlySpan<char> str)
    => XxHash3.HashToUInt64(MemoryMarshal.Cast<char, byte>(str));

static string Cleanup(string str)
    => PunctuationEtcRegex().Replace(str, "");

static string ReplaceRoman(string str)
    => NumberRegex().Replace(str, static e => ToRoman(e.ValueSpan));

static string ToRoman(ReadOnlySpan<char> str)
{
    var number = long.Parse(str.TrimStart('0') is var v and not [] ? v : ['0']);
    
    if (number >= 4000) return new string(str);
    if (number < 1) return string.Empty;

    Span<char> temp = stackalloc char["MMMCMXCIX".Length];
    var sb = new DefaultInterpolatedStringHandler(0, 0, null, temp);

    while (number >= 1)
    {
        switch (number)
        {
            case >= 1000: sb.AppendLiteral("M"); number -= 1000; break;
            case >= 900: sb.AppendLiteral("CM"); number -= 900; break;
            case >= 500: sb.AppendLiteral("D"); number -= 500; break;
            case >= 400: sb.AppendLiteral("CD"); number -= 400; break;
            case >= 100: sb.AppendLiteral("C"); number -= 100; break;
            case >= 90: sb.AppendLiteral("XC"); number -= 90; break;
            case >= 50: sb.AppendLiteral("L"); number -= 50; break;
            case >= 40: sb.AppendLiteral("XL"); number -= 40; break;
            case >= 10: sb.AppendLiteral("X"); number -= 10; break;
            case >= 9: sb.AppendLiteral("IX"); number -= 9; break;
            case >= 5: sb.AppendLiteral("V"); number -= 5; break;
            case >= 4: sb.AppendLiteral("IV"); number -= 4; break;
            default: sb.AppendLiteral("I"); number -= 1; break;
        }
    }

    return sb.ToStringAndClear();
}

namespace HentaiMapGen
{
    [method: JsonConstructor]
    internal readonly record struct SadPandaUrlParts(uint Gid, ulong Token)
    {
        public string TokenHex => Token.ToString("x10");

        public static SadPandaUrlParts Parse(long gid, string token)
            => new((uint)gid, ulong.Parse(token, NumberStyles.HexNumber, CultureInfo.InvariantCulture));
    }
    
    public readonly record struct DeletedGallery(
        [property: JsonPropertyName("Code")] [property: JsonConverter(typeof(CastingNumberConverter<uint>))] uint Code,
        [property: JsonPropertyName("Title")] string Title,
        [property: JsonPropertyName("FullTitle")] string FullTitle,
        [property: JsonPropertyName("Artists")] string[] Artists,
        [property: JsonPropertyName("Groups")] string[] Groups,
        [property: JsonPropertyName("Parodies")] string[] Parodies,
        [property: JsonPropertyName("Characters")] string[] Characters,
        [property: JsonPropertyName("Pages")] [property: JsonConverter(typeof(CastingNumberConverter<uint>))] uint Pages,
        [property: JsonPropertyName("UploadDate")] DateTimeOffset UploadDate
    );
}

partial class Program
{
    [GeneratedRegex(@"\p{P}\p{S}\p{M}\p{C}\s]", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex PunctuationEtcRegex();

    [GeneratedRegex("[0-9]+")]
    private static partial Regex NumberRegex();
}
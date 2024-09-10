// See https://aka.ms/new-console-template for more information

// Mapping of NHentai ID -> SadPanda Gallery ID and Token

using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf;
using HentaiMapGen;
using HentaiMapGen.Proto;
using Microsoft.Collections.Extensions;
using ZstdSharp;

var jsonOpts = new JsonSerializerOptions
{
    Converters =
    {
        new DictionarySlimConverterFactory(),
    },
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
};

var nhentaiMapping = new DictionarySlim<uint, SadPandaUrlParts>();

// Mapping of NHentai ID -> NHentai Title, for when no match was found
var naGalleries = new DictionarySlim<uint, Title>();

// Galleries that returned an error (probably removed by NHentai) and were not matched in removed_galleries.json
var erroredGalleries = new List<uint>();

Console.WriteLine("Deserializing");
const string root = "../../..";
using (var stream = File.OpenRead($"{root}/../HentaiMapGen/nhentaiMapping.json"))
{
    nhentaiMapping = JsonSerializer.Deserialize<DictionarySlim<uint, SadPandaUrlParts>>(stream, jsonOpts)!;
}
using (var stream = File.OpenRead($"{root}/../HentaiMapGen/naGalleries.json"))
{
    naGalleries = JsonSerializer.Deserialize<DictionarySlim<uint, Title>>(stream, jsonOpts)!;
}
using (var stream = File.OpenRead($"{root}/../HentaiMapGen/erroredGalleries.json"))
{
    erroredGalleries = JsonSerializer.Deserialize<List<uint>>(stream, jsonOpts)!;
}
Console.WriteLine("Deserialized");

{
    Console.WriteLine("Loading BookMaps");
    // BookMap keyed by ID % 1024
    var maps = new DictionarySlim<ushort, List<NHentai.Types.BookOrError>>();

    using (var stream = new DecompressionStream(File.OpenRead($"{root}/../HentaiMapGen/galleries.bin.zst"), leaveOpen: false))
    {
        var bookList = NHentai.Types.BookList.Parser.ParseFrom(stream);
        Console.WriteLine(bookList.Books.Count);
        foreach (var bookOrError in bookList.Books)
        {
            ref var r = ref maps.GetOrAddValueRef((ushort)(bookOrError.Id % 1024));
            if (r == null) r = [bookOrError];
            else r.Add(bookOrError);
        }
    }

    Console.WriteLine("Loaded BookMaps");

    Console.WriteLine("Serializing BookMaps");
    Directory.CreateDirectory($"{root}/data/galleries");
    foreach (var (key, value) in maps)
    {
        Console.WriteLine($"{key}");
        var bookMap = new NHentai.Types.BookMap();
        foreach (var bookOrError in value)
        {
            bookMap.Books.Add(bookOrError.Id, bookOrError);
        }
        using var stream = new CompressionStream(File.Create($"{root}/data/galleries/{key}.bin.zst"), 19, leaveOpen: false);
        bookMap.WriteTo(stream);
    }
    Console.WriteLine("Serialized BookMaps");
}

GC.Collect(2, GCCollectionMode.Aggressive, true, true);

{
    Console.WriteLine("Serializing NHentaiMapping");
    var nhentaiMapping2 = new NHentaiMapping();
    foreach (var (key, value) in nhentaiMapping)
    {
        nhentaiMapping2.Mapping.Add(key, new NHentaiMapping.Types.SadPandaUrlParts()
        {
            Gid = value.Gid,
            Token = value.Token,
        });
    }
    
    using var stream = new CompressionStream(File.Create($"{root}/data/mappings.bin.zst"), 19, leaveOpen: false);
    nhentaiMapping2.WriteTo(stream);
    Console.WriteLine("Serialized NHentaiMapping");
}

{
    Console.WriteLine("Serializing UnmatchedGalleries");
    var naGallery2 = new NHentaiMapping.Types.UnmatchedGalleries();
    foreach (var (key, (english, japanese, pretty)) in naGalleries)
    {
        naGallery2.Mapping.Add(key, new NHentai.Types.Title()
        {
            English = english ?? "",
            Japanese = japanese ?? "",
            Pretty = pretty ?? "",
        });
    }
    
    using var stream = new CompressionStream(File.Create($"{root}/data/unmatched.bin.zst"), 19, leaveOpen: false);
    naGallery2.WriteTo(stream);
    Console.WriteLine("Serialized UnmatchedGalleries");
}

{
    Console.WriteLine("Serializing ErroredGalleries");
    var erroredGalleries2 = new NHentaiMapping.Types.ErroredGalleries();
    erroredGalleries2.Ids.AddRange(erroredGalleries);
    
    using var stream = new CompressionStream(File.Create($"{root}/data/errored.bin.zst"), 19, leaveOpen: false);
    erroredGalleries2.WriteTo(stream);
    Console.WriteLine("Serialized ErroredGalleries");
}
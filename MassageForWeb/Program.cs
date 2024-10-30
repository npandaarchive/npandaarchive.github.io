// See https://aka.ms/new-console-template for more information

// Mapping of NHentai ID -> SadPanda Gallery ID and Token

using System.IO.Pipelines;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf;
using HentaiMapGenLib;
using MemoryPack;
using MemoryPack.Streaming;
using Microsoft.Collections.Extensions;
using ZstdNet;

var nHentaiSerializer = Json.NHentaiSerializer.Default;

var dataFolder = args[0];
var outputFolder = $@"{args[0]}\Output";
var webOutputFolder = $@"{args[1]}";

{
    Console.WriteLine("Loading BookMaps");
    // BookMap keyed by ID % 1024
    var maps = new DictionarySlim<ushort, List<Json.Book>>();

    await using (var fs = File.OpenRead($"{outputFolder}/galleries.mpack"))
    // await using (var stream = new DecompressionStream(fs))
    {
        var books = MemoryPackStreamingSerializer.DeserializeAsync<Json.Book>(fs);

        await foreach (var book in books)
        {
            Console.WriteLine(book.Id);

            ref var r = ref maps.GetOrAddValueRef((ushort)(book!.Id! % 1024));
            if (r == null) r = [book];
            else r.Add(book);
        }
    }

    Console.WriteLine("Loaded BookMaps");

    Console.WriteLine("Serializing BookMaps");
    Directory.CreateDirectory($"{webOutputFolder}/galleries");
    foreach (var (key, value) in maps)
    {
        Console.WriteLine($"{key}");
        var bookMap = new Dictionary<uint, Json.Book>();
        foreach (var bookOrError in value)
        {
            bookMap[bookOrError.Id!.Value] = bookOrError;
        }

        await using var fs = File.Create($"{webOutputFolder}/galleries/{key}.mpack.zst");
        await using var stream = new CompressionStream(File.Create($"{webOutputFolder}/galleries/{key}.mpack.zst"), new CompressionOptions(19));
        MemoryPackSerializer.Serialize(PipeWriter.Create(stream), bookMap);
    }
    Console.WriteLine("Serialized BookMaps");
}

GC.Collect(2, GCCollectionMode.Aggressive, true, true);

{
    Console.WriteLine("Serializing NHentaiMapping");
    
    Dictionary<uint, Json.SadPandaIdToken> nhentaiMapping;
    await using (var stream = File.OpenRead($"{outputFolder}/nhentaiMapping.json"))
    {
        nhentaiMapping = JsonSerializer.Deserialize<Dictionary<uint, Json.SadPandaIdToken>>(stream)!;
    }

    await using (var fileStream = File.Create($"{webOutputFolder}/mappings.mpack.zst"))
    await using (var stream = new CompressionStream(fileStream, new CompressionOptions(19)))
    {
        MemoryPackSerializer.Serialize(PipeWriter.Create(stream), nhentaiMapping);
        Console.WriteLine("Serialized NHentaiMapping");
    }
}

{
    Console.WriteLine("Serializing UnmatchedGalleries");
    
    // Mapping of NHentai ID -> NHentai Title, for when no match was found
    Dictionary<uint, Json.Title> naGalleries;
    await using (var stream = File.OpenRead($"{outputFolder}/naGalleries.json"))
    {
        naGalleries = JsonSerializer.Deserialize<Dictionary<uint, Json.Title>>(stream)!;
    }

    await using (var fileStream = File.Create($"{webOutputFolder}/unmatched.mpack.zst"))
    await using (var stream = new CompressionStream(fileStream, new CompressionOptions(19)))
    {
        MemoryPackSerializer.Serialize(PipeWriter.Create(stream), naGalleries);
        Console.WriteLine("Serialized UnmatchedGalleries");
    }
}

{
    Console.WriteLine("Serializing ErroredGalleries");

    // Galleries that returned an error (probably removed by NHentai) and were not matched in removed_galleries.json
    List<uint> erroredGalleries;
    await using (var stream = File.OpenRead($"{outputFolder}/erroredGalleries.json"))
    {
        erroredGalleries = JsonSerializer.Deserialize<List<uint>>(stream)!;
    }

    await using (var fileStream = File.Create($"{webOutputFolder}/errored.mpack.zst"))
    await using (var stream = new CompressionStream(fileStream, new CompressionOptions(19)))
    {
        MemoryPackSerializer.Serialize(PipeWriter.Create(stream), erroredGalleries);
        Console.WriteLine("Serialized ErroredGalleries");
    }
}
// See https://aka.ms/new-console-template for more information

// Mapping of NHentai ID -> SadPanda Gallery ID and Token

using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Text.Json;
using MessagePack;
using Microsoft.Collections.Extensions;
using ZstdNet;

var nHentaiSerializer = Json.NHentaiSerializer.Default;

var dataFolder = args[0];
var outputFolder = $@"{args[0]}\Output";
var webOutputFolder = $@"{args[1]}";

{
    Console.WriteLine("Loading BookMaps");
    // BookMap keyed by ID % 1024
    var maps = new Dictionary<ushort, List<Json.Book>>();

    await using (var fs = File.OpenRead($"{outputFolder}/galleries.msgpack.zst"))
    // await using (var stream = new DecompressionStream(fs))
    {
        using var books = new MessagePackStreamReader(fs);

        await foreach (var bookArr in books.ReadArrayAsync(default))
        {
            var book = MessagePackSerializer.Deserialize<Json.Book>(bookArr);

            // Console.WriteLine(book.Id);

            ref var r = ref CollectionsMarshal.GetValueRefOrAddDefault(maps, (ushort)(book!.Id! % 1024), out var exists);
            if (!exists) r = [book];
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

        await using var fs = File.Create($"{webOutputFolder}/galleries/{key}.msgpack.zst");
        await using var stream = new CompressionStream(fs, new CompressionOptions(19));
        MessagePackSerializer.Serialize(stream, bookMap);
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

    await using (var fileStream = File.Create($"{webOutputFolder}/mappings.msgpack.zst"))
    await using (var stream = new CompressionStream(fileStream, new CompressionOptions(19)))
    {
        MessagePackSerializer.Serialize(stream, nhentaiMapping);
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

    await using (var fileStream = File.Create($"{webOutputFolder}/unmatched.msgpack.zst"))
    await using (var stream = new CompressionStream(fileStream, new CompressionOptions(19)))
    {
        MessagePackSerializer.Serialize(stream, naGalleries);
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

    await using (var fileStream = File.Create($"{webOutputFolder}/errored.msgpack.zst"))
    await using (var stream = new CompressionStream(fileStream, new CompressionOptions(19)))
    {
        MessagePackSerializer.Serialize(stream, erroredGalleries);
        Console.WriteLine("Serialized ErroredGalleries");
    }
}
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Google.Protobuf.WellKnownTypes;
using HentaiMapGen;
using MessagePack;
using Microsoft.Collections.Extensions;
using Enum = System.Enum;
using Type = System.Type;

namespace HentaiMapGenLib.Models.Json;

[method: JsonConstructor]
[MessagePackObject]
public readonly partial record struct Title(
    [property: Key(0), JsonPropertyName("english")] string? English,
    [property: Key(1), JsonPropertyName("japanese")] string? Japanese,
    [property: Key(2), JsonPropertyName("pretty")] string? Pretty
)
{
    internal Proto.Title ToProtobuf() => new()
    {
        English = English ?? "",
        Japanese = Japanese ?? "",
        Pretty = Pretty ?? "",
    };
}

[method: JsonConstructor]
[MessagePackObject]
public readonly partial record struct Book(
    [property: Key(0), JsonPropertyName("error")] string? Error,
    [property: Key(1), JsonPropertyName("id")] [property: JsonConverter(typeof(CastingNumberConverter<uint>))] uint? Id,
    [property: Key(2), JsonPropertyName("media_id")] [property: JsonConverter(typeof(CastingNumberConverter<uint>))] uint? MediaId,
    [property: Key(3), JsonPropertyName("title")] Title Title,
    [property: Key(4), JsonPropertyName("images")] Images? Images,
    [property: Key(5), JsonPropertyName("scanlator")] string? Scanlator,
    [property: Key(6), JsonPropertyName("upload_date")] [property: JsonConverter(typeof(UnixDateTimeConverter))] DateTime? UploadDate,
    [property: Key(7), JsonPropertyName("tags")] Tag[]? Tags,
    [property: Key(8), JsonPropertyName("num_pages")] uint? NumPages,
    [property: Key(9), JsonPropertyName("num_favorites")] uint? NumFavorites
)
{
    public Proto.BookOrError ToProtobuf(uint idFallback)
    {
        if (Error != null)
        {
            return new Proto.BookOrError
            {
                Id = Id ?? idFallback,
                Error = Error
            };
        }

        var protobuf = new Proto.Book
        {
            MediaId = MediaId!.Value,
            Title = Title.ToProtobuf(),
            Images = Images!.Value.ToProtobuf(),
            Scanlator = Scanlator,
            UploadDate = Timestamp.FromDateTimeOffset(UploadDate!.Value),
            NumPages = NumPages!.Value,
            NumFavorites = NumFavorites!.Value
        };

        if (Tags != null)
        {
            protobuf.Tags.AddRange(Tags.Select(e => e.ToProtobuf()));
        }

        return new Proto.BookOrError
        {
            Id = Id ?? idFallback,
            Book = protobuf
        };
    }
}

[method: JsonConstructor]
[MessagePackObject]
public readonly partial record struct Page(
    [property: Key(0), JsonPropertyName("t")] ImageType Type,
    [property: Key(1), JsonPropertyName("w")] uint Width,
    [property: Key(2), JsonPropertyName("h")] uint Height
)
{
    public Proto.Page ToProtobuf() => new()
    {
        Type = Type switch
        {
            ImageType.Jpg => Proto.PageType.Jpg,
            ImageType.Png => Proto.PageType.Png,
            ImageType.Gif => Proto.PageType.Gif,
            ImageType.Invalid1 => Proto.PageType.Invalid1,
            ImageType.Invalid2 => Proto.PageType.Invalid2,
            ImageType.Invalid3 => Proto.PageType.Invalid3,
            _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, null)
        },
        Width = Width,
        Height = Height,
    };
}

[JsonConverter(typeof(EnumMemberConverter<ImageType>))]
public enum ImageType : byte
{
    [EnumMember(Value = "j")] Jpg,
    [EnumMember(Value = "p")] Png,
    [EnumMember(Value = "g")] Gif,
    [EnumMember(Value = "0")] Invalid1,
    [EnumMember(Value = "1")] Invalid2,
    [EnumMember(Value = "2")] Invalid3
}

public static class ImageTypeHelpers
{
    private static readonly Dictionary<(Int128 UnderlyingValue, Type EnumType), string> Cache = new(); 
    
    public static string GetEnumMemberValue<T>(this T enumVal) where T : unmanaged, Enum, IConvertible
    {
        var underlyingValue = GetUnderlyingValue(enumVal, out var underlyingType);

        ref var enumMemberValue = ref CollectionsMarshal.GetValueRefOrAddDefault(Cache, (underlyingValue, underlyingType), out var exists);

        if (exists)
        {
            return enumMemberValue!;
        }

        var memInfo = typeof(T).GetMember(enumVal.ToString());
        return enumMemberValue = (Attribute.GetCustomAttribute(memInfo[0], typeof(EnumMemberAttribute)) as EnumMemberAttribute)?.Value ?? enumVal.ToString();
    }

    private static Int128 GetUnderlyingValue<T>(T enumVal, out Type underlyingType) where T : unmanaged, Enum, IConvertible
    {
        underlyingType = Enum.GetUnderlyingType(typeof(T));
        Int128 underlyingValue;
        if (underlyingType == typeof(sbyte)) underlyingValue = Unsafe.BitCast<T, sbyte>(enumVal);  
        else if (underlyingType == typeof(byte)) underlyingValue = Unsafe.BitCast<T, byte>(enumVal);  
        else if (underlyingType == typeof(short)) underlyingValue = Unsafe.BitCast<T, short>(enumVal);  
        else if (underlyingType == typeof(ushort)) underlyingValue = Unsafe.BitCast<T, ushort>(enumVal);  
        else if (underlyingType == typeof(int)) underlyingValue = Unsafe.BitCast<T, int>(enumVal);  
        else if (underlyingType == typeof(uint)) underlyingValue = Unsafe.BitCast<T, uint>(enumVal);  
        else if (underlyingType == typeof(long)) underlyingValue = Unsafe.BitCast<T, long>(enumVal);
        else if (underlyingType == typeof(ulong)) underlyingValue = Unsafe.BitCast<T, ulong>(enumVal);
        else throw new InvalidOperationException();

        return underlyingValue;
    }
}

[method: JsonConstructor]
[MessagePackObject]
public readonly partial record struct Images(
    [property: Key(0), JsonPropertyName("pages")] Page[] Pages,
    [property: Key(1), JsonPropertyName("cover")] Page Cover,
    [property: Key(2), JsonPropertyName("thumbnail")] Page Thumbnail
)
{
    public Proto.Images ToProtobuf()
    {
        var protobuf = new Proto.Images
        {
            Cover = Cover.ToProtobuf(),
            Thumbnail = Thumbnail.ToProtobuf()
        };
        protobuf.Pages.AddRange(Pages.Select(e => e.ToProtobuf()));
        return protobuf;
    }
}

[method: JsonConstructor]
[MessagePackObject]
public readonly partial record struct Tag(
    [property: Key(0), JsonPropertyName("id")] uint Id,
    [property: Key(1), JsonPropertyName("type")] string Type,
    [property: Key(2), JsonPropertyName("name")] string Name,
    [property: Key(3), JsonPropertyName("url")] string Url,
    [property: Key(4), JsonPropertyName("count")] uint Count
)
{
    public Proto.Tag ToProtobuf() => new()
    {
        Id = Id,
        Type = Type,
        Name = Name,
        Url = Url,
        Count = Count
    };
}
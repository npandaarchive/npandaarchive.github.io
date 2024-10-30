using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Google.Protobuf.WellKnownTypes;
using HentaiMapGen;
using MemoryPack;
using Microsoft.Collections.Extensions;
using Enum = System.Enum;
using Type = System.Type;

namespace HentaiMapGenLib.Models.Json;

[MemoryPackable]
[method: JsonConstructor]
[GenerateTypeScript]
[method: MemoryPackConstructor]
public partial record Title(
    [property: JsonPropertyName("english")] string? English,
    [property: JsonPropertyName("japanese")] string? Japanese,
    [property: JsonPropertyName("pretty")] string? Pretty
)
{
    internal Proto.Title ToProtobuf() => new()
    {
        English = English ?? "",
        Japanese = Japanese ?? "",
        Pretty = Pretty ?? "",
    };
}

[MemoryPackable]
[method: JsonConstructor]
[GenerateTypeScript]
[method: MemoryPackConstructor]
public partial record Book(
    [property: JsonPropertyName("error")] string? Error,
    [property: JsonPropertyName("id")] [property: JsonConverter(typeof(CastingNumberConverter<uint>))] uint? Id,
    [property: JsonPropertyName("media_id")] [property: JsonConverter(typeof(CastingNumberConverter<uint>))] uint? MediaId,
    [property: JsonPropertyName("title")] Title Title,
    [property: JsonPropertyName("images")] Images? Images,
    [property: JsonPropertyName("scanlator")] string? Scanlator,
    [property: JsonPropertyName("upload_date")] [property: JsonConverter(typeof(UnixDateTimeConverter))] DateTime? UploadDate,
    [property: JsonPropertyName("tags")] Tag[]? Tags,
    [property: JsonPropertyName("num_pages")] uint? NumPages,
    [property: JsonPropertyName("num_favorites")] uint? NumFavorites
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
            Images = Images!.ToProtobuf(),
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

[MemoryPackable]
[method: JsonConstructor]
[GenerateTypeScript]
[method: MemoryPackConstructor]
public partial record Page(
    [property: JsonPropertyName("t")] ImageType Type,
    [property: JsonPropertyName("w")] uint Width,
    [property: JsonPropertyName("h")] uint Height
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

[MemoryPackable]
[method: JsonConstructor]
[GenerateTypeScript]
[method: MemoryPackConstructor]
public partial record Images(
    [property: JsonPropertyName("pages")] Page[] Pages,
    [property: JsonPropertyName("cover")] Page Cover,
    [property: JsonPropertyName("thumbnail")] Page Thumbnail
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

[MemoryPackable]
[method: JsonConstructor]
[GenerateTypeScript]
[method: MemoryPackConstructor]
public partial record Tag(
    [property: JsonPropertyName("id")] uint Id,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("count")] uint Count
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
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Google.Protobuf.WellKnownTypes;
using HentaiMapGen.Proto;

namespace HentaiMapGen
{
    [method: JsonConstructor]
    public readonly record struct Title(
        [property: JsonPropertyName("english")] string? English,
        [property: JsonPropertyName("japanese")] string? Japanese,
        [property: JsonPropertyName("pretty")] string? Pretty
    )
    {
        internal NHentai.Types.Title ToProtobuf() => new()
        {
            English = English ?? "",
            Japanese = Japanese ?? "",
            Pretty = Pretty ?? "",
        };
    }

    [method: JsonConstructor]
    public readonly record struct Book(
        [property: JsonPropertyName("error")] string? Error,
        [property: JsonPropertyName("id")] [property: JsonConverter(typeof(CastingNumberConverter<uint>))] uint? Id,
        [property: JsonPropertyName("media_id")] [property: JsonConverter(typeof(CastingNumberConverter<uint>))] uint MediaId,
        [property: JsonPropertyName("title")] Title Title,
        [property: JsonPropertyName("images")] Images Images,
        [property: JsonPropertyName("scanlator")] string Scanlator,
        [property: JsonPropertyName("upload_date")] [property: JsonConverter(typeof(UnixDateTimeConverter))] DateTimeOffset UploadDate,
        [property: JsonPropertyName("tags")] Tag[] Tags,
        [property: JsonPropertyName("num_pages")] uint NumPages,
        [property: JsonPropertyName("num_favorites")] uint NumFavorites
    )
    {
        internal NHentai.Types.BookOrError ToProtobuf(uint idFallback)
        {
            if (Error != null)
            {
                return new NHentai.Types.BookOrError
                {
                    Id = Id ?? idFallback,
                    Error = Error
                };
            }

            var protobuf = new NHentai.Types.Book
            {
                MediaId = MediaId,
                Title = Title.ToProtobuf(),
                Images = Images.ToProtobuf(),
                Scanlator = Scanlator,
                UploadDate = Timestamp.FromDateTimeOffset(UploadDate),
                NumPages = NumPages,
                NumFavorites = NumFavorites
            };
            protobuf.Tags.AddRange(Tags.Select(e => e.ToProtobuf()));
            return new NHentai.Types.BookOrError
            {
                Id = Id ?? idFallback,
                Book = protobuf
            };
        }
    }

    [method: JsonConstructor]
    public readonly record struct Page(
        [property: JsonPropertyName("t")] [property: JsonConverter(typeof(EnumMemberConverter<ImageType, byte>))] ImageType Type,
        [property: JsonPropertyName("w")] uint Width,
        [property: JsonPropertyName("h")] uint Height
    )
    {
        internal NHentai.Types.Page ToProtobuf() => new()
        {
            Type = Type switch
            {
                ImageType.Jpg => NHentai.Types.PageType.Jpg,
                ImageType.Png => NHentai.Types.PageType.Png,
                ImageType.Gif => NHentai.Types.PageType.Gif,
                ImageType.Invalid1 => NHentai.Types.PageType.Invalid1,
                ImageType.Invalid2 => NHentai.Types.PageType.Invalid2,
                ImageType.Invalid3 => NHentai.Types.PageType.Invalid3,
                _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, null)
            },
            Width = Width,
            Height = Height,
        };
    }

    public enum ImageType : byte
    {
        [EnumMember(Value = "j")] Jpg,
        [EnumMember(Value = "p")] Png,
        [EnumMember(Value = "g")] Gif,
        [EnumMember(Value = "0")] Invalid1,
        [EnumMember(Value = "1")] Invalid2,
        [EnumMember(Value = "2")] Invalid3
    }

    [method: JsonConstructor]
    public readonly record struct Images(
        [property: JsonPropertyName("pages")] Page[] Pages,
        [property: JsonPropertyName("cover")] Page Cover,
        [property: JsonPropertyName("thumbnail")] Page Thumbnail
    )
    {
        internal NHentai.Types.Images ToProtobuf()
        {
            var protobuf = new NHentai.Types.Images
            {
                Cover = Cover.ToProtobuf(),
                Thumbnail = Thumbnail.ToProtobuf()
            };
            protobuf.Pages.AddRange(Pages.Select(e => e.ToProtobuf()));
            return protobuf;
        }
    }

    [method: JsonConstructor]
    public readonly record struct Tag(
        [property: JsonPropertyName("id")] uint Id,
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("url")] string Url,
        [property: JsonPropertyName("count")] uint Count
    )
    {
        internal NHentai.Types.Tag ToProtobuf() => new()
        {
            Id = Id,
            Type = Type,
            Name = Name,
            Url = Url,
            Count = Count
        };
    }
}
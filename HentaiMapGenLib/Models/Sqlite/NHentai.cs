using SQLite;

namespace HentaiMapGenLib.Models.Sqlite;

[Table("books")]
public class Book
{
    [Column("error")] public string? Error { get; set; }
    [Column("id"), PrimaryKey] public uint Id { get; set; }
    [Column("media_id")] public uint? MediaId { get; set; }
    [Column("title_english"), Collation("NOCASE"), Indexed] public string? TitleEnglish { get; set; }
    [Column("title_japanese"), Collation("NOCASE"), Indexed] public string? TitleJapanese { get; set; }
    [Column("title_pretty"), Collation("NOCASE"), Indexed] public string? TitlePretty { get; set; }
    [Column("scanlator")] public string? Scanlator { get; set; }
    [Column("upload_date")] public long? UploadDate { get; set; }
    [Column("num_pages")] public uint? NumPages { get; set; }
    [Column("num_favorites")] public uint? NumFavorites { get; set; }
}

[Table("pages")]
public class Page
{
    public const int COVER = -1;
    public const int THUMBNAIL = -2;
    
    [Column("book_id"), Indexed(Name = "book_and_page_num", Unique = true), Indexed] public uint BookId { get; set; }

    // -1 = cover
    // -2 = thumbnail
    [Column("page_number"), Indexed(Name = "book_and_page_num", Unique = true)] public int PageNumber { get; set; }

    [Column("type"), MaxLength(1)] public string Type { get; set; } = ""; 
    [Column("width")] public uint Width { get; set; }
    [Column("height")] public uint Height { get; set; }
}

[Table("book_tags")]
public class BookTag
{
    [Column("tag_id"), Indexed, Indexed(Name = "book_and_tag", Unique = true)]
    public uint TagId { get; set; }

    [Column("book_id"), Indexed, Indexed(Name = "book_and_tag", Unique = true)]
    public uint BookId { get; set; }
}

[Table("tags")]
public class Tag
{
    [Column("id"), PrimaryKey] public uint Id { get; set; }
    [Column("type")] public string Type { get; set; } = null!;
    [Column("name")] public string Name { get; set; } = null!;
    [Column("url")] public string Url { get; set; } = null!;
    [Column("count")] public uint Count { get; set; }
}
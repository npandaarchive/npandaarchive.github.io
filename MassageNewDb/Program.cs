using System.Runtime.Serialization;
using System.Text.Json;
using HentaiMapGen;
using HentaiMapGenLib.Models;
using HentaiMapGenLib.Models.Json;
using HentaiMapGenLib.Models.Sqlite;
using SQLite;
using Book = HentaiMapGenLib.Models.Sqlite.Book;
using Page = HentaiMapGenLib.Models.Sqlite.Page;
using Tag = HentaiMapGenLib.Models.Sqlite.Tag;
using JsonBook = HentaiMapGenLib.Models.Json.Book;

var oldDb = args[0];
var newDb = args[1];

using var db1 = new SQLiteConnection(oldDb, SQLiteOpenFlags.ReadOnly);
using var db2 = new SQLiteConnection(newDb);

db2.CreateTable<Book>();
db2.CreateTable<Page>();
db2.CreateTable<BookTag>();
db2.CreateTable<Tag>();

// var books = db2.Table<Book>();
// var pages = db2.Table<Page>();
// var bookTags = db2.Table<BookTag>();
// var tags = db2.Table<Tag>();

db2.BeginTransaction();

var i = 0;
foreach (var kv in db1.Table<NHentaiKeyValue>().Deferred())
{
    var k = kv.Key;
    var v = kv.Value;

    Console.WriteLine(k);
    
    var nGallery = JsonSerializer.Deserialize<JsonBook>(v.AsSpan().RemovePrefix("json:"));
    
    if ((i++ % 5000) == 0)
    {
        db2.Commit();
        db2.BeginTransaction();
    }

    if (nGallery.Error is not null)
    {
        db2.InsertOrReplace(new Book
        {
            Error = nGallery.Error,
            Id = uint.Parse(k),
        });
    }
    else
    {
        var nGalleryId = nGallery.Id!.Value;
        
        db2.InsertOrReplace(new Page
        {
            BookId = nGalleryId,
            PageNumber = Page.COVER,
            Type = nGallery.Images!.Cover.Type.GetEnumMemberValue(),
            Width = nGallery.Images!.Cover.Width,
            Height = nGallery.Images!.Cover.Height
        });

        db2.InsertOrReplace(new Page
        {
            BookId = nGalleryId,
            PageNumber = Page.THUMBNAIL,
            Type = nGallery.Images!.Thumbnail.Type.GetEnumMemberValue(),
            Width = nGallery.Images!.Thumbnail.Width,
            Height = nGallery.Images!.Thumbnail.Height
        });

        var index = 0;
        foreach (var (imageType, width, height) in nGallery.Images!.Pages)
        {
            db2.InsertOrReplace(new Page
            {
                BookId = nGalleryId,
                PageNumber = index++,
                Type = imageType.GetEnumMemberValue(),
                Width = width,
                Height = height
            });
        }
        
        foreach (var (tagId, type, name, url, count) in nGallery.Tags!)
        {
            db2.InsertOrReplace(new Tag
            {
                Id = tagId,
                Type = type,
                Name = name,
                Url = url,
                Count = count
            });
            db2.InsertOrReplace(new BookTag
            {
                TagId = tagId,
                BookId = nGalleryId,
            });
        }

        db2.InsertOrReplace(new Book
        {
            Id = nGalleryId,
            MediaId = nGallery.MediaId,
            TitleEnglish = !string.IsNullOrEmpty(nGallery.Title.English) ? nGallery.Title.English : null,
            TitleJapanese = !string.IsNullOrEmpty(nGallery.Title.Japanese) ? nGallery.Title.Japanese : null,
            TitlePretty = !string.IsNullOrEmpty(nGallery.Title.Pretty) ? nGallery.Title.Pretty : null,
            Scanlator = !string.IsNullOrEmpty(nGallery.Scanlator) ? nGallery.Scanlator : null,
            UploadDate = nGallery.UploadDate != null ? new DateTimeOffset(nGallery.UploadDate.Value, TimeSpan.Zero).ToUnixTimeSeconds() : null,
            NumPages = nGallery.NumPages,
            NumFavorites = nGallery.NumFavorites
        });
    }
}
db2.Commit();

db2.CreateCommand("VACUUM;").ExecuteNonQuery();
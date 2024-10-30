// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using HentaiMapGenLib.Models;
using HentaiMapGenLib.Models.Json;
using SQLite;
using Sqlite = HentaiMapGenLib.Models.Sqlite;
using Json = HentaiMapGenLib.Models.Json;

using var client = new NHentaiClient();

var homePageResults = await client.GetHomePageListAsync();
var maxId = homePageResults.Result.Where(e => e.Id != null).Max(e => e.Id!.Value);

var dataFolder = args[0];
var outputFolder = $@"{args[0]}\Output";

using var nhentaiDb = new SQLiteConnection($@"{dataFolder}\manga.db");

using var newNhentaiDb = new SQLiteConnection($@"{dataFolder}\new_manga.db");

nhentaiDb.EnableWriteAheadLogging();
newNhentaiDb.EnableWriteAheadLogging();

// EXTRA synchronous is like FULL with the addition that the directory containing a rollback journal is synced after that journal is unlinked to commit a transaction in DELETE mode. EXTRA provides additional durability if the commit is followed closely by a power loss.
nhentaiDb.ExecuteScalar<string>("PRAGMA synchronous=EXTRA");
newNhentaiDb.ExecuteScalar<string>("PRAGMA synchronous=EXTRA");

using var cl = new HttpClient();
cl.DefaultRequestHeaders.Add("User-Agent", "python-requests/2.32.3");

Console.WriteLine(maxId);

for (uint idx = 1; idx <= maxId; idx++)
{
    var kvEntry = nhentaiDb.Find<NHentaiKeyValue>(idx.ToString());
    var bookEntry = newNhentaiDb.Find<Sqlite.Book>(idx);
    if (kvEntry is not null && bookEntry is not null)
    {
        Console.WriteLine($"Skipping {idx}: {bookEntry.TitleEnglish}");
        continue;
    }

    var bookUrl = client.GetBookDetailsUrl((int)idx);
    var response = await cl.GetAsync(bookUrl);

    if (response.StatusCode != HttpStatusCode.NotFound)
        response.EnsureSuccessStatusCode();

    var bookJson = await response.Content.ReadAsByteArrayAsync();
    var nGallery = JsonSerializer.Deserialize(bookJson, NHentaiSerializer.Default.Book)!;
    if (kvEntry is null)
    {
        nhentaiDb.Insert(new NHentaiKeyValue
        {
            Key = idx.ToString(),
            Value = Encoding.UTF8.GetString(bookJson)
        });
    }

    if (bookEntry is null)
    {
        newNhentaiDb.BeginTransaction();
        
        if (nGallery.Error is not null)
        {
            newNhentaiDb.Insert(new Sqlite.Book
            {
                Error = nGallery.Error,
                Id = idx,
            });
        }
        else
        {
            var nGalleryId = nGallery.Id!.Value;
            
            newNhentaiDb.Insert(new Sqlite.Page
            {
                BookId = nGalleryId,
                PageNumber = Sqlite.Page.COVER,
                Type = nGallery.Images!.Cover.Type.GetEnumMemberValue(),
                Width = nGallery.Images!.Cover.Width,
                Height = nGallery.Images!.Cover.Height
            });

            newNhentaiDb.Insert(new Sqlite.Page
            {
                BookId = nGalleryId,
                PageNumber = Sqlite.Page.THUMBNAIL,
                Type = nGallery.Images!.Thumbnail.Type.GetEnumMemberValue(),
                Width = nGallery.Images!.Thumbnail.Width,
                Height = nGallery.Images!.Thumbnail.Height
            });

            var index = 0;
            foreach (var (imageType, width, height) in nGallery.Images!.Pages)
            {
                newNhentaiDb.Insert(new Sqlite.Page
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
                newNhentaiDb.InsertOrReplace(new Sqlite.Tag
                {
                    Id = tagId,
                    Type = type,
                    Name = name,
                    Url = url,
                    Count = count
                });
                newNhentaiDb.Insert(new Sqlite.BookTag
                {
                    TagId = tagId,
                    BookId = nGalleryId,
                });
            }

            newNhentaiDb.Insert(new Sqlite.Book
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
        
        newNhentaiDb.Commit();
    }

    Console.WriteLine(idx);
}

// Console.WriteLine("VACUUMING");
// var sw = Stopwatch.StartNew();
// await Task.WhenAll(
//     Task.Run(() => nhentaiDb.Execute("VACUUM;")),
//     Task.Run(() => newNhentaiDb.Execute("VACUUM;"))
// );
// Console.WriteLine(sw.Elapsed);
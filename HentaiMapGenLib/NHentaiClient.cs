using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using JetBrains.Annotations;

/// <summary>
/// n-Hentai Client
/// copied from : https://github.com/NHMoeDev/NHentai-android/blob/master/app/src/main/kotlin/moe/feng/nhentai/api/ApiConstants.kt
/// </summary>
[PublicAPI]
public class NHentaiClient : IDisposable
{
    public readonly record struct SearchResults(
        [property: JsonPropertyName("result")] Json.Book[] Result,
        [property: JsonPropertyName("num_pages")] int NumPages,
        [property: JsonPropertyName("per_page")] int PerPage
    );
    
    public enum SortBy : byte
    {
        /// <summary>
        /// Default sorting
        /// </summary>
        Default,

        /// <summary>
        /// Sorting by popular
        /// </summary>
        Popular
    }

    public readonly record struct BookRecommend([property: JsonPropertyName("result")] Json.Book[] Result);
    
    #region Client

    private readonly HttpClient _client;

    public NHentaiClient(string userAgent = "python-requests/2.32.3")
    {
        _client = new HttpClient();

        _client.DefaultRequestHeaders.Add("User-Agent", userAgent);
    }

    #endregion

    #region Urls

    protected string ApiRootUrl { get; } = "https://nhentai.net";

    protected string ImageRootUrl { get; } = "https://i.nhentai.net";

    protected string ThumbnailRootUrl { get; } = "https://t.nhentai.net";

    #endregion

    #region Data urls

    protected virtual string GetHomePageUrl(int? pageNum = null) 
        => $"{ApiRootUrl}/api/galleries/all{(pageNum != null ? $"?page={pageNum}" : "")}";

    protected virtual string GetSearchUrl(string content, int pageNum = 1)
        => $"{ApiRootUrl}/api/galleries/search?query={HttpUtility.UrlEncode(content)}&page={pageNum}";

    protected virtual string GetTagUrl(Json.Tag tag, bool isPopularList = false, int pageNum = 1) 
        => $"{ApiRootUrl}/api/galleries/tagged?tag_id={tag.Id}&page={pageNum}{(isPopularList ? "&sort=popular" : "")}";

    public virtual string GetBookDetailsUrl(int bookId) 
        => $"{ApiRootUrl}/api/gallery/{bookId}";

    protected virtual string GetBookRecommendUrl(int bookId) 
        => $"{ApiRootUrl}/api/gallery/{bookId}/related";

    protected virtual string GetGalleryUrl(uint galleryId) 
        => $"{ImageRootUrl}/galleries/{galleryId}";

    protected virtual string GetThumbGalleryUrl(uint galleryId) 
        => $"{ThumbnailRootUrl}/galleries/{galleryId}";

    #endregion

    #region Picture urls

    public virtual string GetPictureUrl(Json.Book book, int pageNum)
        => GetPictureUrl(book.MediaId!.Value, pageNum, ConvertType(GetImage(book, pageNum).Type));

    public virtual string GetThumbPictureUrl(Json.Book book, int pageNum)
        => GetThumbPictureUrl(book.MediaId!.Value, pageNum, ConvertType(GetImage(book, pageNum).Type));

    public virtual string GetBigCoverUrl(Json.Book book)
        => GetBigCoverUrl(book.MediaId!.Value);

    public virtual string GetOriginPictureUrl(Json.Book book, int pageNum) 
        => GetOriginPictureUrl(book.MediaId!.Value, pageNum);

    public virtual string GetBookThumbUrl(Json.Book book)
        => GetBookThumbUrl(book.MediaId!.Value, ConvertType(book.Images!.Cover.Type));

    protected virtual string GetPictureUrl(uint galleryId, int pageNum, string fileType)
        => $"{GetGalleryUrl(galleryId)}/{pageNum}.{fileType}";

    protected virtual string GetThumbPictureUrl(uint galleryId, int pageNum, string fileType)
        => $"{GetThumbGalleryUrl(galleryId)}/{pageNum}t.{fileType}";

    protected virtual string GetBigCoverUrl(uint galleryId)
        => $"{GetThumbGalleryUrl(galleryId)}/cover.jpg";

    protected virtual string GetOriginPictureUrl(uint galleryId, int pageNum)
        => GetPictureUrl(galleryId, pageNum, "jpg");

    protected virtual string GetBookThumbUrl(uint galleryId, string? fileType = "jpg")
        => $"{GetThumbGalleryUrl(galleryId)}/thumb.{fileType ?? "jpg"}";

    #endregion

    #region Utilities

    public virtual async Task<TOutput> GetData<TOutput>(string rootUrl)
    {
        return (await _client.GetFromJsonAsync<TOutput>(rootUrl, Json.NHentaiSerializer.Default.Options))!;
    }

    public virtual async Task<byte[]> GetByteData(string rootUrl)
    {
        return await _client.GetByteArrayAsync(rootUrl);
    }

    public virtual Json.Page GetImage(Json.Book book, int pageNum)
    {
        return book.Images!.Pages[pageNum - 1];
    }

    protected virtual string ConvertType(Json.ImageType type)
    {
        return type switch
        {
            Json.ImageType.Gif => "gif",
            Json.ImageType.Jpg => "jpg",
            Json.ImageType.Png => "png",
            Json.ImageType.Invalid1 => "1",
            Json.ImageType.Invalid2 => "2",
            Json.ImageType.Invalid3 => "3",
            _ => throw new NotSupportedException($"Format {nameof(type)}  does not support.")
        };
    }

    #endregion

    #region Search

    public virtual async Task<SearchResults> GetHomePageListAsync(int? pageNum = null)
    { 
        var url = GetHomePageUrl(pageNum);
        return await GetData<SearchResults>(url);
    }

	public virtual async Task<SearchResults> GetSearchPageListAsync(string keyword, int pageNum = 1)	
	{ 
        var url = GetSearchUrl(keyword, pageNum);
        return await GetData<SearchResults>(url);
    }

    public virtual async Task<SearchResults> GetTagPageListAsync(Json.Tag tag, SortBy sortBy = SortBy.Default, int pageNum = 1)
    {
        var url = GetTagUrl(tag, sortBy == SortBy.Popular, pageNum);
        return await GetData<SearchResults>(url);
    }

    #endregion

    #region Books

    public virtual async Task<Json.Book> GetBookAsync(int bookId)
    {
        var url = GetBookDetailsUrl(bookId);
        return await GetData<Json.Book>(url);
    }

    public virtual async Task<BookRecommend> GetBookRecommendAsync(int bookId)
    {
        var url = GetBookRecommendUrl(bookId);
        var book = await GetData<Json.Book>(url);
        return new BookRecommend([book]);
    }

    #endregion

    #region Picture

    public virtual async Task<byte[]> GetPictureAsync(Json.Book book, int pageNum)
    {
        var url = GetPictureUrl(book, pageNum);
        return await GetByteData(url);
    }

    public virtual async Task<byte[]> GetThumbPictureAsync(Json.Book book, int pageNum)
    {
        var url = GetThumbPictureUrl(book, pageNum);
        return await GetByteData(url);
    }

    public virtual async Task<byte[]> GetBigCoverPictureAsync(Json.Book book)
    {
        var url = GetBigCoverUrl(book.MediaId!.Value);
        return await GetByteData(url);
    }

    public virtual async Task<byte[]> GetOriginPictureAsync(Json.Book book, int pageNum)
    {
        var url = GetOriginPictureUrl(book.MediaId!.Value, pageNum);
        return await GetByteData(url);
    }

    public virtual async Task<byte[]> GetBookThumbPictureAsync(Json.Book book)
    {
        var url = GetBookThumbUrl(book);
        return await GetByteData(url);
    }

    #endregion

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _client.Dispose();
    }
}
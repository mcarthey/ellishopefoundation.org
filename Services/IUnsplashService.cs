using EllisHope.Models;

namespace EllisHope.Services;

public interface IUnsplashService
{
    Task<UnsplashSearchResult> SearchPhotosAsync(string query, int page = 1, int perPage = 30);
    Task<UnsplashPhoto?> GetPhotoAsync(string id);
    Task<byte[]> DownloadPhotoAsync(string url);
    Task TriggerDownloadAsync(string photoId);
}

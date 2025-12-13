using EllisHope.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace EllisHope.Services;

public class UnsplashService : IUnsplashService
{
    private readonly HttpClient _httpClient;
    private readonly UnsplashSettings _settings;
    private const string BaseUrl = "https://api.unsplash.com";

    public UnsplashService(HttpClient httpClient, IOptions<UnsplashSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;

        _httpClient.BaseAddress = new Uri(BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Client-ID {_settings.AccessKey}");
        _httpClient.DefaultRequestHeaders.Add("Accept-Version", "v1");
    }

    public async Task<UnsplashSearchResult> SearchPhotosAsync(string query, int page = 1, int perPage = 30)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/search/photos?query={Uri.EscapeDataString(query)}&page={page}&per_page={perPage}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };

            return JsonSerializer.Deserialize<UnsplashSearchResult>(content, options) ?? new UnsplashSearchResult();
        }
        catch (Exception ex)
        {
            // Log the error
            Console.WriteLine($"Unsplash API error: {ex.Message}");
            return new UnsplashSearchResult();
        }
    }

    public async Task<UnsplashPhoto?> GetPhotoAsync(string id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/photos/{id}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };

            return JsonSerializer.Deserialize<UnsplashPhoto>(content, options);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unsplash API error: {ex.Message}");
            return null;
        }
    }

    public async Task<byte[]> DownloadPhotoAsync(string url)
    {
        try
        {
            return await _httpClient.GetByteArrayAsync(url);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unsplash download error: {ex.Message}");
            return Array.Empty<byte>();
        }
    }

    public async Task TriggerDownloadAsync(string photoId)
    {
        try
        {
            // Trigger download endpoint as required by Unsplash API guidelines
            await _httpClient.GetAsync($"/photos/{photoId}/download");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unsplash download trigger error: {ex.Message}");
        }
    }
}

namespace EllisHope.Models;

public class UnsplashSettings
{
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
}

public class UnsplashPhoto
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AltDescription { get; set; } = string.Empty;
    public UnsplashUrls Urls { get; set; } = new();
    public UnsplashUser User { get; set; } = new();
    public int Width { get; set; }
    public int Height { get; set; }
    public string Color { get; set; } = string.Empty;
}

public class UnsplashUrls
{
    public string Raw { get; set; } = string.Empty;
    public string Full { get; set; } = string.Empty;
    public string Regular { get; set; } = string.Empty;
    public string Small { get; set; } = string.Empty;
    public string Thumb { get; set; } = string.Empty;
}

public class UnsplashUser
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}

public class UnsplashSearchResult
{
    public int Total { get; set; }
    public int TotalPages { get; set; }
    public List<UnsplashPhoto> Results { get; set; } = new();
}

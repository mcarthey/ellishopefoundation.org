using EllisHope.Data;
using EllisHope.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EllisHope.Tests.Helpers;

/// <summary>
/// Manages test data seeding and cleanup for integration tests
/// Implements IDisposable to ensure proper cleanup
/// </summary>
public class TestDataSeeder : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ApplicationDbContext _context;
    private readonly List<object> _createdEntities = new();
    private bool _disposed;

    public TestDataSeeder(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _context = serviceProvider.GetRequiredService<ApplicationDbContext>();
    }

    /// <summary>
    /// Seeds minimal required data for Member Portal tests
    /// Tracks all created entities for cleanup
    /// </summary>
    public async Task SeedMemberPortalDataAsync()
    {
        // Seed Blog Categories
        if (!_context.BlogCategories.Any())
        {
            var categories = new[]
            {
                new BlogCategory { Name = "News", Slug = "news", Description = "Latest news" },
                new BlogCategory { Name = "Events", Slug = "events", Description = "Event updates" }
            };

            _context.BlogCategories.AddRange(categories);
            await _context.SaveChangesAsync();
            _createdEntities.AddRange(categories);
        }

        // Seed Image Sizes
        if (!_context.ImageSizes.Any())
        {
            var imageSizes = new[]
            {
                new ImageSize { Name = "Test Size", Description = "Test", Width = 100, Height = 100, Category = MediaCategory.Page, IsActive = true }
            };

            _context.ImageSizes.AddRange(imageSizes);
            await _context.SaveChangesAsync();
            _createdEntities.AddRange(imageSizes);
        }

        // Seed Default Pages (Member Dashboard needs pages)
        await SeedDefaultPagesAsync();
        
        // Seed content that Member Portal views expect
        await SeedMemberPortalContentAsync();
    }

    /// <summary>
    /// Seeds content specifically needed by Member Portal views
    /// </summary>
    private async Task SeedMemberPortalContentAsync()
    {
        // Seed sample blog posts (views expect at least one)
        if (!_context.BlogPosts.Any())
        {
            for (int i = 1; i <= 3; i++)
            {
                await SeedBlogPostAsync($"Sample Blog Post {i}");
            }
        }

        // Seed sample events (views expect at least one upcoming event)
        if (!_context.Events.Any())
        {
            for (int i = 1; i <= 3; i++)
            {
                var evt = new Event
                {
                    Title = $"Upcoming Event {i}",
                    Slug = $"upcoming-event-{i}",
                    Description = $"Description for event {i}",
                    Location = "Test Location",
                    EventDate = DateTime.UtcNow.AddDays(7 + i),
                    IsPublished = true,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Events.Add(evt);
                await _context.SaveChangesAsync();
                _createdEntities.Add(evt);
            }
        }

        // Seed sample causes (views may display active causes)
        if (!_context.Causes.Any())
        {
            for (int i = 1; i <= 2; i++)
            {
                var cause = new Cause
                {
                    Title = $"Active Cause {i}",
                    Slug = $"active-cause-{i}",
                    ShortDescription = $"Help us with cause {i}",
                    Description = $"<p>Full description for cause {i}</p>",
                    GoalAmount = 10000m,
                    RaisedAmount = 2500m * i,
                    IsPublished = true,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Causes.Add(cause);
                await _context.SaveChangesAsync();
                _createdEntities.Add(cause);
            }
        }
    }

    /// <summary>
    /// Seeds default pages required by views
    /// </summary>
    private async Task SeedDefaultPagesAsync()
    {
        var requiredPages = new Dictionary<string, string>
        {
            { "Home", "Home Page" },
            { "About", "About Us" },
            { "Contact", "Contact Us" },
            { "MemberDashboard", "Member Dashboard" }
        };

        foreach (var (pageName, title) in requiredPages)
        {
            if (!await _context.Pages.AnyAsync(p => p.PageName == pageName))
            {
                var page = new Page
                {
                    PageName = pageName,
                    Title = title,
                    MetaDescription = $"{title} page",
                    IsPublished = true,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                };

                _context.Pages.Add(page);
                await _context.SaveChangesAsync();
                _createdEntities.Add(page);

                // Add default content sections
                var sections = new[]
                {
                    new ContentSection
                    {
                        PageId = page.Id,
                        SectionKey = "hero-title",
                        ContentType = "Text",
                        Content = title,
                        DisplayOrder = 0
                    },
                    new ContentSection
                    {
                        PageId = page.Id,
                        SectionKey = "main-content",
                        ContentType = "RichText",
                        Content = $"<p>Welcome to {title}</p>",
                        DisplayOrder = 1
                    }
                };

                _context.ContentSections.AddRange(sections);
                await _context.SaveChangesAsync();
                _createdEntities.AddRange(sections);
            }
        }
    }

    /// <summary>
    /// Seeds a test blog post
    /// </summary>
    public async Task<BlogPost> SeedBlogPostAsync(string title = "Test Blog Post")
    {
        var category = await _context.BlogCategories.FirstAsync();
        
        var post = new BlogPost
        {
            Title = title,
            Slug = title.ToLower().Replace(" ", "-"),
            Excerpt = "Test excerpt",
            Content = "<p>Test content</p>",
            AuthorName = "Test Author",
            IsPublished = true,
            PublishedDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow
        };

        _context.BlogPosts.Add(post);
        await _context.SaveChangesAsync();
        _createdEntities.Add(post);
        
        // Link to category
        var blogPostCategory = new BlogPostCategory
        {
            BlogPostId = post.Id,
            CategoryId = category.Id
        };
        _context.BlogPostCategories.Add(blogPostCategory);
        await _context.SaveChangesAsync();
        _createdEntities.Add(blogPostCategory);

        return post;
    }

    /// <summary>
    /// Seeds a test event
    /// </summary>
    public async Task<Event> SeedEventAsync(string title = "Test Event")
    {
        var evt = new Event
        {
            Title = title,
            Slug = title.ToLower().Replace(" ", "-"),
            Description = "Test event description",
            Location = "Test Location",
            EventDate = DateTime.UtcNow.AddDays(7),
            IsPublished = true,
            CreatedDate = DateTime.UtcNow
        };

        _context.Events.Add(evt);
        await _context.SaveChangesAsync();
        _createdEntities.Add(evt);

        return evt;
    }

    /// <summary>
    /// Seeds a test cause
    /// </summary>
    public async Task<Cause> SeedCauseAsync(string title = "Test Cause")
    {
        var cause = new Cause
        {
            Title = title,
            Slug = title.ToLower().Replace(" ", "-"),
            ShortDescription = "Test short description",
            Description = "<p>Test full description</p>",
            GoalAmount = 10000m,
            RaisedAmount = 2500m,
            IsPublished = true,
            CreatedDate = DateTime.UtcNow
        };

        _context.Causes.Add(cause);
        await _context.SaveChangesAsync();
        _createdEntities.Add(cause);

        return cause;
    }

    /// <summary>
    /// Seeds a test media item
    /// </summary>
    public async Task<Media> SeedMediaAsync(string fileName = "test-image.jpg")
    {
        var media = new Media
        {
            FileName = fileName,
            FilePath = $"/uploads/{fileName}",
            FileSize = 1024,
            MimeType = "image/jpeg",
            Source = MediaSource.Local,
            Category = MediaCategory.Page,
            AltText = "Test image",
            UploadedDate = DateTime.UtcNow
        };

        _context.MediaLibrary.Add(media);
        await _context.SaveChangesAsync();
        _createdEntities.Add(media);

        return media;
    }

    /// <summary>
    /// Cleanup: Removes all entities created during the test
    /// Called automatically by Dispose()
    /// </summary>
    public async Task CleanupAsync()
    {
        if (_disposed)
            return;

        // Remove in reverse order (to handle foreign key constraints)
        _createdEntities.Reverse();

        foreach (var entity in _createdEntities)
        {
            try
            {
                // Check if entity is still tracked
                var entry = _context.Entry(entity);
                if (entry.State == EntityState.Detached)
                {
                    // Re-attach if detached
                    _context.Attach(entity);
                }

                _context.Remove(entity);
            }
            catch (Exception)
            {
                // Entity might have already been deleted by cascade
                // or might not exist - safe to ignore
            }
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception)
        {
            // Ignore errors during cleanup
            // In-memory database will be destroyed anyway
        }

        _createdEntities.Clear();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // Cleanup synchronously (Dispose can't be async)
            CleanupAsync().GetAwaiter().GetResult();
        }

        _disposed = true;
    }
}

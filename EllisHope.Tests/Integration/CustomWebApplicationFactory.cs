using EllisHope.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace EllisHope.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory for integration testing
/// Configures in-memory database and test services
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove ALL existing DbContext-related registrations
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            
            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            var dbContextOptionsDescriptor = services.Where(d => d.ServiceType == typeof(DbContextOptions))
                .ToList();
            
            foreach (var descriptor in dbContextOptionsDescriptor)
            {
                services.Remove(descriptor);
            }

            // Remove ApplicationDbContext itself
            var appDbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ApplicationDbContext));
            
            if (appDbContextDescriptor != null)
                services.Remove(appDbContextDescriptor);

            // Add ApplicationDbContext using an in-memory database for testing
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryTestDb");
            });

            // Replace the antiforgery service with a no-op implementation for testing
            var antiforgeryDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IAntiforgery));
            if (antiforgeryDescriptor != null)
            {
                services.Remove(antiforgeryDescriptor);
            }
            services.AddSingleton<IAntiforgery, NoOpAntiforgery>();
        });

        builder.UseEnvironment("Testing");
    }

    public ApplicationDbContext GetDbContext()
    {
        var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Ensure the database is created
        db.Database.EnsureCreated();
        
        return db;
    }

    private void SeedTestData(ApplicationDbContext context)
    {
        // Add test data here if needed for integration tests
        // For example:
        // context.Pages.Add(new Page { PageName = "TestPage", Title = "Test" });
        // context.SaveChanges();
    }
}

/// <summary>
/// No-op antiforgery implementation for testing that always succeeds
/// </summary>
public class NoOpAntiforgery : IAntiforgery
{
    public AntiforgeryTokenSet GetAndStoreTokens(HttpContext httpContext)
    {
        return new AntiforgeryTokenSet("test-request-token", "test-cookie-token", "test-form-field", "test-header");
    }

    public AntiforgeryTokenSet GetTokens(HttpContext httpContext)
    {
        return new AntiforgeryTokenSet("test-request-token", "test-cookie-token", "test-form-field", "test-header");
    }

    public Task<bool> IsRequestValidAsync(HttpContext httpContext)
    {
        // Always return true - skip validation in tests
        return Task.FromResult(true);
    }

    public void SetCookieTokenAndHeader(HttpContext httpContext)
    {
        // No-op
    }

    public Task ValidateRequestAsync(HttpContext httpContext)
    {
        // No-op - always succeed
        return Task.CompletedTask;
    }
}

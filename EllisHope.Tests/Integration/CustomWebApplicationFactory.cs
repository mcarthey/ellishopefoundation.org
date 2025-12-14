using EllisHope.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace EllisHope.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory for integration testing
/// Uses SQLite in-memory database for reliable and fast testing
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IDisposable
{
    private SqliteConnection? _connection;

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

            // Create and open SQLite in-memory connection
            // IMPORTANT: Connection must stay open for in-memory database to persist
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // Add ApplicationDbContext using SQLite in-memory database for testing
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(_connection);
                options.EnableSensitiveDataLogging(); // Helpful for debugging tests
            });

            // Build service provider and ensure database is created with schema
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();
            var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory>>();

            try
            {
                // Ensure database schema is created
                db.Database.EnsureCreated();

                // Optionally seed test data
                SeedTestData(db);

                logger.LogInformation("Test database initialized successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred creating the test database");
                throw;
            }

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

        return db;
    }

    private void SeedTestData(ApplicationDbContext context)
    {
        // Optionally add common test data here that all tests can use
        // For example:
        // context.Causes.Add(new Cause
        // {
        //     Title = "Common Test Cause",
        //     Slug = "common-test-cause",
        //     Description = "A cause available in all tests",
        //     GoalAmount = 1000,
        //     RaisedAmount = 500,
        //     StartDate = DateTime.Now,
        //     IsPublished = true
        // });
        // context.SaveChanges();
    }

    // CRITICAL: Properly dispose of the SQLite connection
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection?.Close();
            _connection?.Dispose();
            _connection = null;
        }
        base.Dispose(disposing);
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

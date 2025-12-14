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
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;
    private bool _databaseInitialized;
    private readonly object _lock = new object();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove ALL DbContext and provider-related services
            var descriptorsToRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType == typeof(ApplicationDbContext))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Create and open SQLite in-memory connection
            // IMPORTANT: Connection must stay open for in-memory database to persist
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // Add ApplicationDbContext using SQLite in-memory database for testing
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(_connection);
                options.EnableSensitiveDataLogging();
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

    protected override HttpClient CreateClient(WebApplicationFactoryClientOptions options)
    {
        // Ensure database is initialized before creating client
        EnsureDatabaseCreated();
        return base.CreateClient(options);
    }

    private void EnsureDatabaseCreated()
    {
        if (_databaseInitialized)
            return;

        lock (_lock)
        {
            if (_databaseInitialized)
                return;

            using var scope = Services.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();
            var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory>>();

            try
            {
                // Ensure database schema is created
                db.Database.EnsureCreated();
                logger.LogInformation("Test database initialized successfully with SQLite");
                _databaseInitialized = true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred creating the test database");
                throw;
            }
        }
    }

    public ApplicationDbContext GetDbContext()
    {
        EnsureDatabaseCreated();
        var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return db;
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

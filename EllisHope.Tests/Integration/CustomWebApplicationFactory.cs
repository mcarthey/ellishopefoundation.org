using EllisHope.Data;
using EllisHope.Models.Domain;
using EllisHope.Tests.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Authentication;
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
            // Remove the existing DbContext registration
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // Remove any DbContextOptions
            var dbContextOptionsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions));
            if (dbContextOptionsDescriptor != null)
            {
                services.Remove(dbContextOptionsDescriptor);
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

            // Add test authentication scheme
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthenticationHelper.TestScheme;
                options.DefaultChallengeScheme = TestAuthenticationHelper.TestScheme;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthenticationHelper.TestScheme, options => { });

            // Replace the antiforgery service with a no-op implementation for testing
            var antiforgeryDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IAntiforgery));
            if (antiforgeryDescriptor != null)
            {
                services.Remove(antiforgeryDescriptor);
            }
            services.AddSingleton<IAntiforgery, NoOpAntiforgery>();
        });

        builder.UseEnvironment("Testing");
        
        // Ensure database is created after all services are configured
        builder.ConfigureServices(services =>
        {
            var sp = services.BuildServiceProvider();
            EnsureDatabaseCreated(sp);
        });
    }

    private void EnsureDatabaseCreated(IServiceProvider serviceProvider)
    {
        if (_databaseInitialized)
            return;

        lock (_lock)
        {
            if (_databaseInitialized)
                return;

            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();
            var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory>>();

            try
            {
                // Ensure database schema is created
                db.Database.EnsureCreated();
                logger.LogInformation("Test database initialized successfully with SQLite");
                
                // Seed minimal required data for tests
                SeedMinimalTestDataAsync(scopedServices).GetAwaiter().GetResult();
                
                _databaseInitialized = true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred creating the test database");
                throw;
            }
        }
    }

    /// <summary>
    /// Seeds minimal required data that all tests need
    /// Individual tests can add more specific data using TestDataSeeder
    /// </summary>
    private async Task SeedMinimalTestDataAsync(IServiceProvider services)
    {
        var seeder = new TestDataSeeder(services);
        try
        {
            await seeder.SeedMemberPortalDataAsync();
        }
        finally
        {
            // Don't dispose here - let tests manage their own cleanup
            // seeder.Dispose();
        }
    }

    public ApplicationDbContext GetDbContext()
    {
        var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return db;
    }

    /// <summary>
    /// Creates an authenticated HTTP client for testing
    /// </summary>
    public HttpClient CreateAuthenticatedClient(string userId)
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        
        // Add test authentication header
        client.DefaultRequestHeaders.Add("X-Test-User-Id", userId);
        
        return client;
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

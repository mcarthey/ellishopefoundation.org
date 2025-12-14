# Integration Tests Failing for Causes Controller

## ✅ RESOLVED

**Resolution Date:** 2025-12-14
**Solution:** Switched from EF Core InMemory provider to SQLite in-memory database

---

## Original Issue Description

8 integration tests for the CausesController were failing with a DbContext initialization error, despite all unit tests passing and the functionality working correctly in the application.

## Environment
- .NET 10.0
- Entity Framework Core 10.0.1
- xUnit 3.1.5

## Error Details
```
Microsoft.EntityFrameworkCore.Internal.DbContextServices.Initialize(IServiceProvider scopedProvider, DbContextOptions contextOptions, DbContext context)
at Microsoft.EntityFrameworkCore.DbContext.get_ContextServices()
at Microsoft.EntityFrameworkCore.DbContext.get_Model()
at Microsoft.EntityFrameworkCore.Internal.InternalDbSet`1.get_EntityType()
```

The error occurred when the integration tests tried to access the `Causes` DbSet through the `CustomWebApplicationFactory`.

## Previously Failing Tests (Now Fixed)
1. ✅ `List_ReturnsSuccess`
2. ✅ `List_WithSearchTerm_ReturnsFilteredResults`
3. ✅ `Grid_ReturnsSuccess`
4. ✅ `Details_WithValidSlug_ReturnsSuccess`
5. ✅ `Details_WithInvalidSlug_ReturnsNotFound`
6. ✅ `PublicCausesEndpoints_AllowAnonymousAccess`
7. ✅ `CauseDetailsRoute_WithSlug_MapsCorrectly`
8. ✅ `FullWorkflow_BrowseCausesPublicly`

## Root Cause

EF Core's InMemory database provider had issues with DbContext initialization for the Causes entity in .NET 10. This was a known limitation of the InMemory provider, which is designed as a simple mock rather than a real relational database.

## Solution Implemented

Switched from `UseInMemoryDatabase()` to `UseSqlite()` with an in-memory connection:

### Changes Made:

**1. Added NuGet Package:**
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.1" />
```

**2. Updated CustomWebApplicationFactory:**
```csharp
private SqliteConnection? _connection;

protected override void ConfigureWebHost(IWebHostBuilder builder)
{
    // Create and open SQLite in-memory connection
    _connection = new SqliteConnection("DataSource=:memory:");
    _connection.Open();

    // Use SQLite instead of InMemory
    services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseSqlite(_connection);
        options.EnableSensitiveDataLogging();
    });

    // ... ensure database is created
}

// Properly dispose connection
protected override void Dispose(bool disposing)
{
    if (disposing)
    {
        _connection?.Close();
        _connection?.Dispose();
    }
    base.Dispose(disposing);
}
```

**3. Unskipped All Tests:**
Removed `Skip` attributes from all 8 previously failing tests.

## Benefits of SQLite In-Memory

✅ **Real relational database** - Not just a mock
✅ **Better EF Core compatibility** - Handles relationships and constraints properly
✅ **Fast** - Runs entirely in memory
✅ **No infrastructure required** - No Docker or external database needed
✅ **Reliable** - Works consistently across all environments
✅ **Better test coverage** - Catches database-specific issues

## Verification

All integration tests now pass:
- ✅ All 23/23 CausesController integration tests (100%)
- ✅ All 45 CauseService unit tests (100%)
- ✅ All 27 CausesController unit tests (100%)
- ✅ All other integration tests continue to pass

## Files Modified

- `EllisHope.Tests/EllisHope.Tests.csproj` - Added SQLite package
- `EllisHope.Tests/Integration/CustomWebApplicationFactory.cs` - Switched to SQLite
- `EllisHope.Tests/Integration/CausesControllerIntegrationTests.cs` - Unskipped tests

## Additional Notes

This solution is recommended for all new projects using EF Core 10+ for integration testing. The InMemory provider should be considered deprecated for integration tests in favor of SQLite in-memory or Testcontainers for production-like testing.

---
**Labels**: bug, testing, integration-tests, entity-framework, resolved
**Milestone**: Testing Infrastructure
**Priority**: ~~Medium~~ → Resolved

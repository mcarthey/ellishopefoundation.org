# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands Reference

### Build & Run
```powershell
# Build the solution
dotnet build EllisHope.sln

# Build main project only
dotnet build EllisHope\EllisHope.csproj

# Run the application (development)
cd EllisHope
dotnet run
# App runs at https://localhost:7049 (or check console output)

# Build SASS assets
cd EllisHope
npm run sass:build
npm run sass:watch  # Watch mode for development
```

### Testing
```powershell
# Run all tests (unit + integration, excludes E2E)
dotnet test EllisHope.sln --filter "TestCategory!=E2E"

# Run specific test project
dotnet test EllisHope.Tests\EllisHope.Tests.csproj

# Run only integration tests
dotnet test --filter "Category=Integration"

# Run a single test by name
dotnet test --filter "DisplayName~TestMethodName"

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Run E2E tests (requires app running at https://localhost:7049)
# Terminal 1: cd EllisHope && dotnet run
# Terminal 2:
dotnet test --filter "Category=E2E"

# Run E2E with visible browser
$env:HEADED = "1"
dotnet test --filter "Category=E2E"

# Run E2E in slow motion (debugging)
$env:SLOWMO = "500"
$env:HEADED = "1"
dotnet test --filter "DisplayName~SpecificTestName"
```

### Database
```powershell
# Create new migration
dotnet ef migrations add MigrationName --project EllisHope

# Apply migrations
dotnet ef database update --project EllisHope

# Remove last migration (if not applied)
dotnet ef migrations remove --project EllisHope

# View pending migrations
dotnet ef migrations list --project EllisHope

# Note: Migrations auto-apply on app startup (except in Testing environment)
# Seeding is idempotent and runs automatically via DbSeeder.SeedAsync()
```

## Architecture Overview

### Application Type
ASP.NET Core 10.0 MVC/Razor Pages web application with an Admin area for authenticated users.

### Key Architectural Patterns

**Service Layer Pattern**: Business logic resides in services (not controllers). Controllers orchestrate services and return views/results. Services are registered in `Program.cs` and injected via DI.

**Repository Pattern via EF Core**: `ApplicationDbContext` serves as the data access layer. Direct DbContext usage in services (no separate repository abstraction).

**Area-based Separation**:
- Public controllers in `/Controllers` (Home, Blog, Events, Contact, etc.)
- Admin controllers in `/Areas/Admin/Controllers` (require authentication/authorization)
- Shared layouts in `/Views/Shared` and `/Areas/Admin/Views/Shared`

**Testing Environment Isolation**: `Program.cs` checks `IsEnvironment("Testing")` to skip database configuration, migrations, and seeding. `CustomWebApplicationFactory` provides SQLite in-memory database for integration tests.

### Project Structure
```
EllisHope/                      # Main ASP.NET Core web application
├── Areas/Admin/                # Admin portal (authenticated)
│   ├── Controllers/            # Account, Applications, Blog, Causes, Dashboard, Media, Pages, Users
│   ├── Models/                 # Admin-specific ViewModels
│   └── Views/                  # Admin-specific Razor views
├── Controllers/                # Public-facing controllers
├── Data/
│   ├── ApplicationDbContext.cs # EF Core DbContext (IdentityDbContext<ApplicationUser>)
│   └── DbSeeder.cs             # Idempotent data seeding (roles, users, test data)
├── Models/
│   ├── Domain/                 # Entity models (ApplicationUser, ClientApplication, BlogPost, Event, Cause, Media, etc.)
│   └── ViewModels/             # View-specific models
├── Services/                   # Business logic layer (all services follow I{Name}Service interface pattern)
├── Views/                      # Public Razor views
└── wwwroot/                    # Static files (CSS, JS, images)

EllisHope.Tests/               # Test project (xUnit, Playwright)
├── Controllers/               # Controller unit tests
├── Integration/               # Integration tests using CustomWebApplicationFactory
│   └── CustomWebApplicationFactory.cs  # SQLite in-memory test database setup
├── E2E/                       # Playwright E2E tests
│   └── README.md              # E2E test execution guide
├── Services/                  # Service unit tests
├── Unit/                      # Other unit tests
└── Helpers/                   # Test utilities (TestDataSeeder, TestAuthHandler, etc.)
```

### Core Domain Models

**ApplicationUser** (extends `IdentityUser`): Custom user model with roles (Admin, BoardMember, Member, Sponsor, Client, Editor), status tracking, profile fields, and sponsor-client relationships.

**ClientApplication**: Multi-step application workflow for program sponsorship. Stateful entity with status progression: Draft → Submitted → UnderReview → Approved/Rejected/MoreInfoRequired. Supports draft saving, board member voting, sponsor assignment, and document attachments.

**ApplicationVote**: Board member voting on applications. One vote per board member per application (enforced by composite unique index). Tracks vote type (Approve/Reject/Abstain), comments, and voting timestamp.

**Blog, Event, Cause**: Content entities with slugs, featured images, categories, and publishing workflow.

**Media**: Centralized media library with usage tracking, image processing (multiple sizes), and Unsplash integration.

### Service Layer

Services implement business logic and are scoped per request. Key services:

- **IClientApplicationService**: Application submission, draft management, status updates, voting, sponsor assignment
- **IUserManagementService**: User CRUD, role management, sponsor-client relationships
- **IEmailService / IEmailTemplateService**: Transactional emails for application workflows
- **IDocumentService / IPdfService**: Document generation and PDF rendering (QuestPDF)
- **IBlogService, IEventService, ICauseService**: Content management
- **IMediaService**: Media upload, processing, usage tracking
- **IPageService**: Dynamic page management with templates

All services follow the pattern: Define interface (I{Name}Service), implement concrete service, register in `Program.cs` with `AddScoped`.

### Authentication & Authorization

**Identity Configuration** (`Program.cs:21-59`):
- Password requirements: 8+ chars, upper, lower, digit, special char
- Lockout: 5 attempts, 15 min lockout
- Cookie paths: Login `/Admin/Account/Login`, Logout `/Admin/Account/Logout`
- SameSite: Lax (production compatibility)

**Roles**: Admin, BoardMember, Member, Sponsor, Client, Editor (seeded in `DbSeeder`)

**Admin Area**: Uses `[Area("Admin")]` and `[Authorize]` attributes. Access control via role-based policies.

**Test Authentication**: `CustomWebApplicationFactory` replaces authentication with `TestAuthHandler` for integration tests. Use `CreateAuthenticatedClient(userId)` for authenticated test requests.

### Database Context

**ApplicationDbContext** (`Data/ApplicationDbContext.cs`):
- Inherits `IdentityDbContext<ApplicationUser>`
- Configures relationships, indexes, decimal precision, cascade behaviors
- Key configurations:
  - ApplicationUser self-referencing (Sponsor → SponsoredClients)
  - ClientApplication relationships (Applicant, AssignedSponsor, DecisionMadeBy, Votes, Comments)
  - Unique indexes on slugs (Blog, Event, Cause)
  - Composite unique index (ApplicationId, VoterId) for ApplicationVote

**Migrations**: Auto-apply on startup via `Program.cs:142-181` (skipped in Testing environment)

**Seeding**: `DbSeeder.SeedAsync()` runs on startup, creating roles, admin user, test users (for E2E tests), and sample data. Idempotent - safe to run multiple times.

### Testing Strategy

**CustomWebApplicationFactory** (`EllisHope.Tests/Integration/CustomWebApplicationFactory.cs`):
- Uses SQLite in-memory database (connection stays open for test duration)
- Sets environment to "Testing" to skip production database setup
- Replaces antiforgery validation with no-op implementation
- Provides `CreateAuthenticatedClient(userId)` for authenticated tests
- Seeds minimal test data via `TestDataSeeder`

**Test Categories**:
- Unit tests: Test individual methods/classes in isolation
- Integration tests (`[Trait("Category", "Integration")]`): Test full request pipeline with in-memory database
- E2E tests (`[Trait("Category", "E2E")]`): Playwright browser tests against running application

**E2E Prerequisites**:
- App must be running at https://localhost:7049
- Test users must exist (seeded automatically or via DbSeeder)
- See `EllisHope.Tests/E2E/README.md` for test accounts and execution guide

### Multi-Step Application Workflow

The ClientApplication system is a core feature with complex state management:

**Application States**: Draft → Submitted → UnderReview → (Approved | Rejected | MoreInfoRequired) → Active (if approved)

**Draft Save**: Applications can be saved as drafts at any step. Draft state persists all filled fields. Users can resume drafts from MyApplications page.

**Submission**: Draft → Submitted (validates all required fields, sets SubmittedDate)

**Review Process**:
1. Board members review submitted applications
2. Board members vote (Approve/Reject/Abstain) via ApplicationVote
3. After all votes, admin moves to Approved/Rejected
4. Approved applications → assign sponsor → status becomes Active
5. Rejected applications can be optionally deleted or archived

**Key Controllers**:
- `MyApplicationsController` (public): Submit, view drafts, view status
- `Admin/ApplicationsController`: Review, vote, approve/reject, assign sponsor

**Important Patterns**:
- Always load related entities: `.Include(a => a.Applicant).Include(a => a.Votes).ThenInclude(v => v.Voter)`
- Draft saves must be partial-validation tolerant
- Status transitions are strict (use service methods, not direct property sets)
- Votes are immutable once cast (edit by delete + re-vote)

### CSS/SCSS Build Process

The project uses SASS for styling:
- Source: `EllisHope/wwwroot/assets/scss/`
- Output: `EllisHope/wwwroot/assets/css/`
- Build: `npm run sass:build` (compiles main.scss and spacing.scss)
- Watch: `npm run sass:watch` (auto-recompile on changes)
- Production: `npm run sass:build:compressed`

### CI/CD Pipeline

GitHub Actions workflow (`.github/workflows/dotnet-ci.yml`):
- Triggers on push to main, develop, claude/** branches and PRs
- Build job: Restore, build, run unit/integration tests with coverage, upload to Codecov
- E2E job: Build, install Playwright, start app, run E2E tests, upload artifacts
- Uses .NET 10 preview SDK
- Excludes E2E tests from unit test run via `--filter "TestCategory!=E2E"`

### Email System

Email configuration via `appsettings.json` → `EmailSettings` section:
- Development: Uses mock SMTP or localhost
- Production: Requires SMTP credentials (set via user secrets or environment variables)
- `IEmailTemplateService`: Renders HTML email templates for application status changes, notifications
- `IEmailService`: Sends emails via SMTP

### Special Notes

**Program.cs Testing Environment Logic**:
- Lines 14-18: Skip DbContext registration if Testing (test factory provides it)
- Lines 142-181: Skip migrations and seeding if Testing

**Antiforgery in Tests**: `CustomWebApplicationFactory` replaces `IAntiforgery` with `NoOpAntiforgery` to bypass CSRF validation in integration tests.

**User Secrets**: Use `dotnet user-secrets set` for local development secrets (connection strings, SMTP credentials). Not committed to repo.

**Static Assets**: Uses `MapStaticAssets()` (ASP.NET Core 10 feature) for optimized static file serving.

**Custom Routes** (`Program.cs:113-133`):
- Blog: `/blog/details/{slug}`
- Event: `/event/details/{slug}`
- Cause: `/causes/details/{slug}`
- Admin area: `/{area:exists}/{controller}/{action}/{id?}`

### Common Development Patterns

**Adding a New Entity**:
1. Create model in `Models/Domain/`
2. Add `DbSet<T>` to `ApplicationDbContext`
3. Configure relationships/indexes in `OnModelCreating`
4. Create migration: `dotnet ef migrations add AddEntityName`
5. Create service interface and implementation
6. Register service in `Program.cs`
7. Create controller and views
8. Write unit and integration tests

**Adding a New Admin Feature**:
1. Add controller in `Areas/Admin/Controllers/`
2. Mark with `[Area("Admin")]` and `[Authorize(Roles = "...")]`
3. Create views in `Areas/Admin/Views/{ControllerName}/`
4. Add navigation link in `Areas/Admin/Views/Shared/_Layout.cshtml`

**Working with Images**:
- Upload via `IMediaService.UploadAsync()`
- Automatic processing: Original + Thumbnail + OptimizedFullSize
- Usage tracking via `MediaUsage` entity
- Supported sources: Upload, Unsplash

### Debugging Tips

**Integration Test Failures**:
- Check `CustomWebApplicationFactory` is being used
- Verify `IsEnvironment("Testing")` logic in `Program.cs`
- Ensure SQLite connection stays open
- Check test data seeding

**E2E Test Failures**:
- Confirm app is running at correct URL
- Use `$env:HEADED = "1"` to watch browser
- Use `$env:SLOWMO = "500"` to slow down actions
- Check test users exist in database
- Review `EllisHope.Tests/E2E/README.md`

**Database Issues**:
- Check connection string in `appsettings.json` or user secrets
- Verify migrations are applied: `dotnet ef migrations list`
- Check seeding logs on app startup
- For tests: Ensure `CustomWebApplicationFactory` is disposing properly

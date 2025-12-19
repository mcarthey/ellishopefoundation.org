# Onboarding — Developers & AI Assistants

This document provides a compact onboarding checklist and pointers to get developers or AI assistants productive quickly.

Goals
- Get local dev environment running
- Run unit & integration tests
- Understand core architecture and where to find key code
- Know how to update documentation and tests

Prerequisites
- .NET 10 SDK
- Visual Studio 2024 (or VS Code)
- SQL Server (LocalDB or full) for local development
- Git

Quick start (developer)
1. Clone the repo
   ```bash
   git clone https://github.com/mcarthey/ellishopefoundation.org
   cd EllisHopeFoundation
   ```
2. Restore & build
   ```bash
   dotnet restore
   dotnet build
   ```
3. Set secrets and connection string (recommended via user-secrets)
   ```bash
   cd EllisHope
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\mssqllocaldb;Database=EllisHopeDev;Trusted_Connection=True;"
   ```
4. Apply migrations & seed data
   ```bash
   dotnet ef database update --project EllisHope
   ```
   - The seeder will create default roles and an admin user.
5. Run the app
   ```bash
   dotnet run --project EllisHope
   ```

Running tests
- Run all tests:
  ```bash
  dotnet test EllisHope.Tests/EllisHope.Tests.csproj
  ```
- Run only integration tests:
  ```bash
  dotnet test --filter "Category=Integration"
  ```
- E2E tests: See `EllisHope.Tests/E2E/README.md` (requires app running for some scenarios)

Key files & where to look
- `Program.cs` — app startup, DI registrations, Identity, DB setup.
- `Data/ApplicationDbContext.cs` — EF Core DbContext and DbSets.
- `Areas/Admin/Controllers/ApplicationsController.cs` — core application workflow.
- `Areas/Admin/Views/Applications/` — admin UI views (Index/Details/RequestInfo/etc.).
- `Services/` — business logic classes.
- `EllisHope.Tests/Integration/CustomWebApplicationFactory.cs` — test host configuration (in-memory DB, antiforgery overrides).

Code review checklist for new features
- Add/Update unit tests for service logic
- Add/Update integration tests for controller behavior
- Add/Update docs in `docs/DEVELOPER-GUIDE.md` and `docs/API_REFERENCE.md` when adding or changing routes
- Keep controllers thin; business logic belongs in services

Documentation & docs process
- Primary authoritative docs are in `docs/DEVELOPER-GUIDE.md` and `docs/ADMIN-GUIDE.md`.
- Archive legacy phase docs in `docs/archive/`.
- Use `scripts/check-md-links.ps1` to validate Markdown links.

Tips for AI assistants
- Start from `docs/PROJECT_REFERENCE.md` to find authoritative locations
- Prefer code and the Developer Guide over legacy docs
- When modifying behavior that affects tests, update `EllisHope.Tests/Integration/CustomWebApplicationFactory.cs` or the E2E README as needed

Contact points
- Repo owner / lead: mcarthey (use GitHub issues or email)
- Support: support@ellishope.org

Welcome — the system is organized by Areas (Admin) and services. When in doubt, run tests and review `CustomWebApplicationFactory` to understand test behavior.

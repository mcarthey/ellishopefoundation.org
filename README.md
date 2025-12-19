# Ellis Hope Foundation Website

This repository contains the Ellis Hope web application and its tests.

Projects
- `EllisHope` — main Razor Pages / MVC web app (ASP.NET Core 10)
- `EllisHope.Tests` — test project (unit, integration, E2E)

Quick start (development)
1. Build:
   ```powershell
   dotnet build EllisHope\EllisHope.csproj
   ```
2. Run the web app (development):
   ```powershell
   cd EllisHope
   dotnet run
   ```
   The app will print the local URL (usually https://localhost:5xxx).

Running tests
- Unit + integration:
  ```powershell
  dotnet test EllisHope.Tests\EllisHope.Tests.csproj
  ```
- Run only integration tests:
  ```powershell
  dotnet test EllisHope.Tests\EllisHope.Tests.csproj --filter "Category=Integration"
  ```
- E2E guidance is in `EllisHope.Tests/E2E/README.md` (headed mode, slowmo, required test accounts).

Key implementation notes
- The app uses ASP.NET Core Identity for authentication and Authorization with role-based access (Admin, BoardMember, Editor, etc.).
- Integration tests use a `CustomWebApplicationFactory` to run the app in a test host. That factory configures an in-memory EF Core database and test-specific service overrides.
- `Program.cs` includes logic to skip applying migrations and seeding when the environment is `Testing` to avoid interfering with the in-memory database used by tests.
- Some integration/E2E tests expect seeded test users; see `EllisHope.Tests/E2E/README.md` for test user accounts and E2E run instructions.

Useful files
- `EllisHope/Program.cs` — app startup, services, and production migration/seeding logic
- `EllisHope/Data/ApplicationDbContext.cs` — EF Core DbContext
- `EllisHope.Tests/Integration/CustomWebApplicationFactory.cs` — test host customization (in-memory DB, antiforgery test behavior)
- `EllisHope.Tests/E2E/README.md` — E2E instructions and test accounts

Documentation
- Developer documentation: `docs/DEVELOPER-GUIDE.md`
- Admin documentation: `docs/ADMIN-GUIDE.md`

Contributing
- Update `docs/DEVELOPER-GUIDE.md` for implementation or setup changes.
- Update `EllisHope.Tests/E2E/README.md` when test accounts or E2E steps change.

If anything in the documentation does not match the current implementation, please open an issue or update the relevant guide in `docs/`.

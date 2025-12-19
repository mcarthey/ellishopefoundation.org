# Project Reference — Ellis Hope Foundation

This consolidated reference documents the *actual* repository layout, key implementation details, and authoritative documentation locations. It is intended to help maintainers and future AI assistants quickly understand and work with the codebase.

Keep this file up to date when the implementation changes. If contradictions exist between this file and other markdowns, trust this file (and the code) and update the other docs.

---

## Primary projects
- `EllisHope` — main web application (ASP.NET Core 10)
  - Uses MVC-style controllers and Razor views (Areas used for Admin). Code is in `EllisHope/`.
- `EllisHope.Tests` — test project (unit, integration, E2E). Code is in `EllisHope.Tests/`.

Paths referenced in tests and docs
- `EllisHope/Program.cs` — app startup (service registrations, Identity, DB setup)
- `EllisHope/Data/ApplicationDbContext.cs` — EF Core DbContext
- `EllisHope/Areas/Admin/Controllers/ApplicationsController.cs` — Admin application workflow controller
- `EllisHope/Areas/Admin/Views/Applications/` — admin application views
- `EllisHope.Tests/Integration/CustomWebApplicationFactory.cs` — test host customization (in-memory DB, antiforgery/test overrides)
- `EllisHope.Tests/E2E/README.md` — E2E instructions and test accounts

---

## How to build, run and test (authoritative)
1. Build:
   ```powershell
   dotnet build EllisHope\EllisHope.csproj
   ```
2. Run in development (app prints local URL):
   ```powershell
   cd EllisHope
   dotnet run
   ```
3. Run tests:
   - All tests: `dotnet test EllisHope.Tests\EllisHope.Tests.csproj`
   - Integration only: `dotnet test --filter "Category=Integration"`
   - E2E tests: see `EllisHope.Tests/E2E/README.md` (requires app running or WebApplicationFactory-backed E2E runner)

Notes:
- Tests use an in-memory EF Core provider via `CustomWebApplicationFactory`. `Program.cs` contains logic to skip applying migrations & seeding when `ASPNETCORE_ENVIRONMENT` == `Testing`.
- Some integration tests expect seeded test users (E2E README lists accounts and passwords used by the test scenarios).

---

## Authentication & test behavior
- App uses ASP.NET Core Identity (see `Program.cs` and `DbSeeder`). Roles: Admin, BoardMember, Editor, Sponsor, Client, Member.
- Integration tests exercise authentication and anti-forgery behavior. The test factory may override antiforgery or provide no-op antiforgery to allow POSTs without tokens.

If you modify authentication, update `EllisHope.Tests/Integration/CustomWebApplicationFactory.cs` and E2E README accordingly.

---

## Authoritative documentation files
- `docs/DEVELOPER-GUIDE.md` — primary technical documentation for developers. Keep in sync with code.
- `docs/ADMIN-GUIDE.md` — primary user-facing admin instructions.
- `EllisHope.Tests/E2E/README.md` — authoritative E2E instructions and test user accounts.
- `README.md` — project overview and quick start (should link to the two guides above).

When you change implementation (APIs, routes, test users, auth behavior), update these files.

---

## Known outdated or duplicate docs
There are many phase-based and test-runner docs under `EllisHope.Tests/` (PHASE* and SUMMARY files) that document past project states. These are useful for history but can be confusing if left as-is.
Suggested actions:
1. Treat `PHASE*` and `PROJECT_COMPLETE_SUMMARY.md` files as archival. Do not rely on them as current implementation docs.
2. Consolidate any still-relevant technical details into `docs/DEVELOPER-GUIDE.md`.
3. Remove or archive outdated markdowns in a dedicated `docs/archive/` folder if cleanup is desired.

Files likely outdated (examples):
- `EllisHope.Tests/PROJECT_COMPLETE_SUMMARY.md`
- `EllisHope.Tests/PHASE6_*`, `PHASE7_*`, `PHASE8_*`, etc.
- `docs/CLEANUP-COMPLETED.md` and similar status reports — keep for history only.

---

## Entry points for a new developer or AI assistant
Start here in this order:
1. `docs/DEVELOPER-GUIDE.md` — read architecture and setup steps
2. `README.md` — quick start and pointers
3. `EllisHope/Program.cs` — see service registrations and how Identity/DB are configured
4. `EllisHope/Data/ApplicationDbContext.cs` — understand entities and relationships
5. `EllisHope/Areas/Admin/Controllers/ApplicationsController.cs` and `EllisHope/Areas/Admin/Views/Applications/` — example of workflow and view patterns
6. `EllisHope.Tests/Integration/CustomWebApplicationFactory.cs` — how tests run the app and any test-specific overrides
7. `EllisHope.Tests/E2E/README.md` — E2E commands and test accounts

---

## Conventions used in the codebase
- Areas: Admin routes live under `/Admin` and controllers are in `Areas/Admin/Controllers`.
- Views: Razor views (.cshtml) under `Views/` and `Areas/Admin/Views/`.
- Services: Business logic in `Services/`, registered via DI and consumed by controllers.
- DB: EF Core 10; migrations live with the `EllisHope` project.
- Tests: xUnit. Integration tests use WebApplicationFactory.

---

## Quick checklist for keeping docs accurate
1. When changing public routes or controller actions, update `docs/DEVELOPER-GUIDE.md` and `docs/ADMIN-GUIDE.md`.
2. When changing seeding or test users, update `EllisHope.Tests/E2E/README.md`.
3. When altering test factory behavior, update `EllisHope.Tests/Integration/CustomWebApplicationFactory.cs` and mention changes in `docs/DEVELOPER-GUIDE.md`.
4. For major feature changes, add a short migration note to the top of `docs/DEVELOPER-GUIDE.md` and `README.md`.

---

## Next recommended steps (I can do these for you)
- Move legacy PHASE docs into `docs/archive/` or mark them as historical.
- Update `EllisHope.Tests/E2E/README.md` test user list if accounts changed.
- Add short `docs/API_REFERENCE.md` listing important routes and expected behaviors for admin workflows.
- Run a docs link check to ensure no broken internal links.

If you want, I can:
- Create `docs/archive/` and move the identified legacy files there,
- Add `docs/API_REFERENCE.md` with admin endpoints and required roles,
- Update `EllisHope.Tests/E2E/README.md` or `docs/DEVELOPER-GUIDE.md` with any missing accurate details.

---

Maintainer: update this file when the code changes. Future AI assistants should treat this file as the single source of truth about where to look next.

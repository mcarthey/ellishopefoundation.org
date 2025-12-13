# Codecov Setup Guide for Ellis Hope Foundation

## Overview
This guide explains how code coverage reporting is configured for the Ellis Hope Foundation .NET project.

## What is Codecov?
Codecov provides detailed code coverage reports and integrates with GitHub to show coverage on pull requests and track coverage trends over time.

## Coverage Tool: Coverlet (Recommended for .NET Core/.NET 5+)

### Why Coverlet?
Your project uses **Coverlet** via the `coverlet.collector` NuGet package, which is the **modern, Microsoft-recommended approach** for .NET Core and newer projects:

- ? **Native Integration**: Works seamlessly with `dotnet test`
- ? **Cross-Platform**: Windows, Linux, macOS support
- ? **.NET 10 Compatible**: Full support for latest .NET versions
- ? **Zero Configuration**: Works out of the box
- ? **Industry Standard**: Generates Cobertura XML format

### Alternative: OpenCover
While Codecov's example repository ([codecov/example-csharp](https://github.com/codecov/example-csharp)) uses OpenCover, that's primarily for .NET Framework compatibility. For your modern .NET 10 Razor Pages project, **Coverlet is the superior choice**.

See `COVERLET-VS-OPENCOVER.md` for a detailed comparison.

## Setup Steps Completed

### 1. ? GitHub Actions Workflow Updated
The CI workflow (`.github/workflows/dotnet-ci.yml`) now:
- Collects code coverage during test execution using `--collect:"XPlat Code Coverage"`
- Organizes coverage files in `./coverage` directory
- Uploads coverage to Codecov using `codecov-action@v5`
- Uses your `CODECOV_TOKEN` secret for authentication

### 2. ? Codecov Configuration Added
A `codecov.yml` file has been created in the root directory with:
- **Coverage targets**: 70-100% range
- **Ignored paths**: Migrations, wwwroot, Razor views, generated files
- **Comment configuration**: Detailed coverage reports on PRs
- **Status checks**: Automatic coverage validation

### 3. ? Test Project Configuration
The test project already has `coverlet.collector` package which:
- Generates coverage data during test execution
- Outputs in Cobertura XML format (compatible with Codecov)
- Works seamlessly with `dotnet test`

## How Coverage Collection Works

### Local Testing with Coverage
```bash
# Run tests and collect coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Coverage files will be in ./coverage/**/coverage.cobertura.xml
```

### CI/CD Process
1. **Tests run**: `dotnet test` executes all 45 tests
2. **Coverage collected**: Coverlet generates `coverage.cobertura.xml`
3. **Results organized**: Files saved to `./coverage` directory
4. **Upload to Codecov**: `codecov-action` uploads the XML files
5. **Codecov processes**: Generates reports and badges

## Codecov Dashboard Features

Once your first build completes, you'll see:

### On GitHub
- ? Coverage status check on PRs
- ? Coverage diff showing changes
- ? File-by-file coverage in PR comments

### On codecov.io
- ?? Coverage trends over time
- ?? Sunburst charts showing project structure
- ?? Detailed file coverage reports
- ?? Coverage goals and targets

## Adding the Coverage Badge to README

After your first successful upload, add this to your `README.md`:

```markdown
[![codecov](https://codecov.io/gh/mcarthey/ellishopefoundation.org/branch/main/graph/badge.svg)](https://codecov.io/gh/mcarthey/ellishopefoundation.org)
```

## Understanding Coverage Reports

### What's Covered
- ? **Services** (`EllisHope/Services/`)
  - BlogService (22 tests)
  - EventService (23 tests)
- ? **Domain Models** (property initialization)
- ? **Business Logic**

### What's Excluded (per codecov.yml)
- ? Database migrations
- ? Static files (wwwroot)
- ? Razor views (.cshtml)
- ? Auto-generated files
- ? Entry points (Program.cs)

## Expected Coverage Levels

Based on your test suite:

| Component | Expected Coverage | Current Tests |
|-----------|------------------|---------------|
| BlogService | ~95% | 22 tests |
| EventService | ~95% | 23 tests |
| Controllers | ~60% | (Add tests in future) |
| Overall Project | ~70-80% | 45 tests |

## Troubleshooting

### Coverage Not Uploading
1. Verify `CODECOV_TOKEN` is set in GitHub Secrets
2. Check Actions logs for upload errors
3. Ensure coverage files are generated: look for `coverage.cobertura.xml` in logs

### Low Coverage Reported
1. Check `codecov.yml` ignore patterns
2. Verify tests are actually running (check test count)
3. Review Codecov dashboard for specific files with low coverage

### Coverage Failing CI
The workflow is configured with `fail_ci_if_error: false`, so:
- ? Failed coverage upload won't fail the build
- ? Tests must still pass for build to succeed
- ?? Check logs if coverage isn't appearing

## Next Steps

### Immediate (After First Push)
1. ? Push changes to trigger workflow
2. ? Verify build passes in GitHub Actions
3. ? Check Codecov dashboard for reports
4. ? Add coverage badge to README

### Future Improvements
- [ ] Add controller tests to increase coverage
- [ ] Add integration tests
- [ ] Set coverage threshold requirements (currently permissive)
- [ ] Configure coverage trends tracking
- [ ] Add coverage requirements for PRs

## Useful Commands

### Generate coverage locally
```bash
# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# View coverage files
ls ./coverage/**/coverage.cobertura.xml
```

### Install coverage report viewer (optional)
```bash
# Install ReportGenerator tool
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML report from coverage
reportgenerator -reports:"./coverage/**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html

# Open report
start coveragereport/index.html
```

## Configuration Files

### Primary Configuration
- **Workflow**: `.github/workflows/dotnet-ci.yml` - CI/CD pipeline
- **Codecov Config**: `codecov.yml` - Coverage reporting rules
- **Test Project**: `EllisHope.Tests/EllisHope.Tests.csproj` - Has coverlet.collector

### Coverage Output
- **Location**: `./coverage/**/coverage.cobertura.xml`
- **Format**: Cobertura XML (industry standard)
- **Generated by**: Coverlet (via XPlat Code Coverage)

## Resources

- [Codecov Documentation](https://docs.codecov.com/docs)
- [Coverlet Documentation](https://github.com/coverlet-coverage/coverlet)
- [Codecov GitHub Action](https://github.com/codecov/codecov-action)
- [Your Codecov Dashboard](https://app.codecov.io/gh/mcarthey/ellishopefoundation.org)

## Support

If you encounter issues:
1. Check the [Codecov Status Page](https://status.codecov.io/)
2. Review [GitHub Actions Logs](https://github.com/mcarthey/ellishopefoundation.org/actions)
3. Consult [Codecov Support](https://codecov.io/support)

---

**Note**: Since you're using .NET 10 (preview), if you encounter any issues with coverage collection, you can use .NET 9 temporarily until .NET 10 is fully supported by all tooling.

# CI/CD Setup Documentation

## Overview
This project uses GitHub Actions for Continuous Integration (CI) to automatically build and test the application on every push and pull request.

## Workflow Configuration

### Location
`.github/workflows/dotnet-ci.yml`

### Triggers
- **Push** to branches: `main`, `develop`, `claude/**`
- **Pull Requests** to: `main`, `develop`

### Build Environment
- **OS**: Ubuntu Latest
- **Framework**: .NET 10 (Preview)

## Workflow Steps

### 1. Checkout Code
Uses `actions/checkout@v4` to clone the repository.

### 2. Setup .NET 10
Installs .NET 10 SDK (preview version) using `actions/setup-dotnet@v4`.

**Important**: .NET 10 requires the `dotnet-quality: 'preview'` flag as it's not yet GA.

### 3. Restore Dependencies
```bash
dotnet restore EllisHope.sln
```
Restores all NuGet packages for both projects:
- `EllisHope` (main web application)
- `EllisHope.Tests` (unit tests)

### 4. Build Solution
```bash
dotnet build EllisHope.sln --configuration Release --no-restore
```
Compiles both projects in Release mode.

### 5. Run Tests
```bash
dotnet test EllisHope.sln --configuration Release --no-build --verbosity normal --logger "trx;LogFileName=test-results.trx" --collect:"XPlat Code Coverage"
```

Features:
- Runs all 45 unit tests
- Generates TRX report for test results
- Collects code coverage data

### 6. Publish Test Results
Uses `dorny/test-reporter@v1` to publish test results as GitHub Check Runs.

### 7. Upload Coverage Reports
Uses `codecov/codecov-action@v4` to upload coverage reports to Codecov (if configured).

## Local Testing

To verify the workflow will succeed before pushing:

```bash
# Restore dependencies
dotnet restore EllisHope.sln

# Build in Release mode
dotnet build EllisHope.sln --configuration Release --no-restore

# Run tests
dotnet test EllisHope.sln --configuration Release --no-build --verbosity normal
```

## Project Structure

```
EllisHopeFoundation/
??? .github/
?   ??? workflows/
?       ??? dotnet-ci.yml          # CI workflow
??? EllisHope/                      # Main web application
?   ??? EllisHope.csproj
??? EllisHope.Tests/                # Unit tests
?   ??? EllisHope.Tests.csproj
??? EllisHope.sln                   # Solution file
```

## Test Coverage

Current test suite includes:
- **45 unit tests** across multiple services
- **BlogService**: 22 tests covering CRUD operations, search, categories
- **EventService**: 23 tests covering CRUD operations, filtering, search

## Required Secrets (Optional)

### For Code Coverage (Codecov)
If you want to enable Codecov integration:

1. Sign up at [codecov.io](https://codecov.io)
2. Add your repository
3. Add the `CODECOV_TOKEN` secret to your GitHub repository:
   - Go to Settings ? Secrets and variables ? Actions
   - Add new repository secret: `CODECOV_TOKEN`
   - Paste the token from Codecov

**Note**: The workflow will continue to work without this token; coverage upload will simply be skipped.

## Troubleshooting

### .NET 10 Not Available
If .NET 10 is not available in GitHub Actions:
1. Check [setup-dotnet action](https://github.com/actions/setup-dotnet) for supported versions
2. Consider downgrading to .NET 9 temporarily
3. Or use the `preview` quality flag as shown in the workflow

### Build Failures
1. Check the build logs in GitHub Actions
2. Ensure all project references are correct
3. Verify NuGet packages are compatible with .NET 10

### Test Failures
1. Review test output in the Actions logs
2. Run tests locally to reproduce
3. Check for environment-specific issues (case-sensitive paths on Linux)

## Status Badge

Add this to your README.md to show build status:

```markdown
![.NET CI](https://github.com/mcarthey/ellishopefoundation.org/workflows/.NET%20CI/badge.svg)
```

## Future Enhancements

- [ ] Add deployment workflow for production
- [ ] Add staging environment deployment
- [ ] Implement automatic database migrations
- [ ] Add security scanning (Dependabot, CodeQL)
- [ ] Add performance testing
- [ ] Add integration tests

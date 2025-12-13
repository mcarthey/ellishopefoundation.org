# CI/CD Setup Documentation

## Overview
This document describes the Continuous Integration and Continuous Deployment (CI/CD) setup for the EllisHope project using GitHub Actions.

## GitHub Actions Workflow

### File Location
`.github/workflows/dotnet-ci.yml`

### Workflow Triggers
The CI workflow runs automatically on:
- **Push events** to branches:
  - `main`
  - `develop`
  - `claude/**` (all Claude Code branches)
- **Pull requests** targeting:
  - `main`
  - `develop`

### Workflow Steps

1. **Checkout Code**
   - Uses `actions/checkout@v4`
   - Clones the repository for testing

2. **Setup .NET**
   - Uses `actions/setup-dotnet@v4`
   - Installs .NET 9.0 SDK

3. **Restore Dependencies**
   - Restores NuGet packages for the main project
   - Command: `dotnet restore EllisHope.csproj`

4. **Build Main Project**
   - Builds in Release configuration
   - Command: `dotnet build EllisHope.csproj --configuration Release --no-restore`

5. **Restore Test Dependencies**
   - Restores NuGet packages for the test project
   - Command: `dotnet restore EllisHope.Tests/EllisHope.Tests.csproj`

6. **Build Test Project**
   - Builds tests in Release configuration
   - Command: `dotnet build EllisHope.Tests/EllisHope.Tests.csproj --configuration Release --no-restore`

7. **Run Tests**
   - Executes all unit tests
   - Generates TRX test result files
   - Command: `dotnet test EllisHope.Tests/EllisHope.Tests.csproj --configuration Release --no-build --verbosity normal --logger "trx;LogFileName=test-results.trx"`

8. **Publish Test Results**
   - Uses `dorny/test-reporter@v1`
   - Publishes test results as GitHub check run
   - Runs even if previous steps fail (`if: always()`)
   - Fails the workflow if tests fail

## Test Execution

### Test Coverage
The CI workflow runs all 52 unit tests:
- **27 BlogService tests** covering:
  - CRUD operations for blog posts
  - Category management
  - Search functionality
  - Slug generation and validation

- **25 EventService tests** covering:
  - CRUD operations for events
  - Upcoming/past event filtering
  - Search functionality
  - Similar events discovery
  - Slug generation and validation

### Test Framework
- **xUnit** v2.9.2
- **In-memory database** for isolation
- **AAA pattern** (Arrange, Act, Assert)

## Local Testing

To run the same tests locally:

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build --configuration Release

# Run tests
dotnet test --configuration Release --verbosity normal
```

## Viewing Test Results

### In GitHub Actions
1. Go to the **Actions** tab in your GitHub repository
2. Click on the workflow run you want to inspect
3. View the **Test Results** section for detailed pass/fail information
4. Check the build logs for any compilation or test failures

### Test Result Files
- Test results are generated in TRX format
- Files are stored in `**/TestResults/` directories
- These directories are ignored by git (configured in `.gitignore`)

## Build Status

You can add a build status badge to your README:

```markdown
![.NET CI](https://github.com/mcarthey/ellishopefoundation.org/workflows/.NET%20CI/badge.svg)
```

## Troubleshooting

### Build Failures
If the build fails:
1. Check the **Build** step logs in GitHub Actions
2. Verify all NuGet packages are restored correctly
3. Ensure .NET 9.0 SDK is compatible with all dependencies

### Test Failures
If tests fail:
1. Check the **Run tests** step logs
2. Review the **Test Results** summary
3. Identify which specific tests are failing
4. Run the failing tests locally to debug

### Common Issues

**Issue**: Tests fail in CI but pass locally
- **Solution**: Ensure you're using the same .NET version (9.0.x)
- **Solution**: Check for timing-dependent tests
- **Solution**: Verify in-memory database behavior is consistent

**Issue**: Build fails due to missing dependencies
- **Solution**: Ensure all PackageReferences are in the .csproj files
- **Solution**: Clear NuGet cache: `dotnet nuget locals all --clear`

**Issue**: Workflow doesn't trigger
- **Solution**: Verify branch names match the trigger patterns
- **Solution**: Check GitHub Actions permissions in repository settings

## Best Practices

1. **Always run tests locally** before pushing
2. **Keep test execution time under 5 minutes** for fast feedback
3. **Fix broken builds immediately** to maintain code quality
4. **Review test results** on every pull request
5. **Update workflow** when adding new test projects or changing .NET versions

## Future Enhancements

Potential improvements to the CI/CD pipeline:

- [ ] Add code coverage reporting (using Coverlet)
- [ ] Add static code analysis (using SonarCloud or similar)
- [ ] Add deployment to staging/production environments
- [ ] Add performance testing
- [ ] Add integration tests with real database
- [ ] Add automated dependency updates (using Dependabot)
- [ ] Add security scanning (using GitHub Advanced Security)

## Related Documentation

- [TEST_SUMMARY.md](TEST_SUMMARY.md) - Comprehensive test documentation
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [.NET Testing Documentation](https://docs.microsoft.com/en-us/dotnet/core/testing/)

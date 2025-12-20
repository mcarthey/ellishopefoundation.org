# ?? GitHub Actions Setup Guide

## Required Secrets

To run the CI/CD pipeline, you need to configure the following secrets in your GitHub repository.

### 1. Codecov Token

**Purpose:** Upload code coverage reports to Codecov

**Steps to Configure:**

1. Go to [codecov.io](https://codecov.io) and sign in with your GitHub account
2. Add the `ellishopefoundation.org` repository
3. Copy the **Repository Upload Token**
4. In GitHub:
   - Go to **Settings** ? **Secrets and variables** ? **Actions**
   - Click **New repository secret**
   - Name: `CODECOV_TOKEN`
   - Value: Paste the token from Codecov
   - Click **Add secret**

### 2. Deployment Secrets (Optional)

If you're deploying to Azure/AWS/other platforms:

#### Azure Web App
- `AZURE_WEBAPP_PUBLISH_PROFILE` - Download from Azure Portal
- `AZURE_WEBAPP_NAME` - Your web app name

#### AWS
- `AWS_ACCESS_KEY_ID`
- `AWS_SECRET_ACCESS_KEY`
- `AWS_REGION`

## Workflow Configuration

### Environment Variables

The workflow uses these environment variables (no secrets needed):

```yaml
DOTNET_VERSION: '10.0.x'
ASPNETCORE_ENVIRONMENT: Development
ASPNETCORE_URLS: https://localhost:7042
```

### Modifying Port

If your app runs on a different port, update:

1. `.github/workflows/ci-cd.yml` - Change `ASPNETCORE_URLS`
2. `EllisHope.Tests/playwright.config.json` - Change `baseURL`
3. `EllisHope.Tests/E2E/ApplicationHappyPathTests.cs` - Change `BaseUrl` const

## What the Workflow Does

### On Every Push/PR:

1. **Build & Test Job:**
   - ? Restores dependencies
   - ? Builds the solution
   - ? Runs unit & integration tests
   - ? Collects code coverage
   - ? Uploads to Codecov

2. **E2E Tests Job:** (runs after build succeeds)
   - ? Installs Playwright browsers
   - ? Starts the application
   - ? Runs Playwright E2E tests
   - ? Uploads screenshots/videos on failure

3. **Deploy Job:** (only on `main` branch)
   - ? Deploys to production (configure as needed)

## Viewing Results

### Test Results
- Click on the **Actions** tab in GitHub
- Select a workflow run
- View **Artifacts** for test results and screenshots

### Code Coverage
- Codecov will comment on PRs with coverage reports
- Badge available at: `https://codecov.io/gh/mcarthey/ellishopefoundation.org/branch/main/graph/badge.svg`

### Playwright Reports
- On test failure, check **Artifacts** ? `playwright-artifacts`
- Contains screenshots and videos of failures

## Badge Setup

Add these to your README.md:

```markdown
[![CI/CD Pipeline](https://github.com/mcarthey/ellishopefoundation.org/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/mcarthey/ellishopefoundation.org/actions/workflows/ci-cd.yml)
[![codecov](https://codecov.io/gh/mcarthey/ellishopefoundation.org/branch/main/graph/badge.svg)](https://codecov.io/gh/mcarthey/ellishopefoundation.org)
```

## Troubleshooting

### Workflow Not Running
- Check `.github/workflows/ci-cd.yml` is committed
- Verify Actions are enabled in Settings ? Actions

### E2E Tests Failing
- Application may not have started in time (increase `sleep` duration)
- Port conflict (check `ASPNETCORE_URLS`)
- Database not initialized (add migration step)

### Codecov Upload Failing
- Verify `CODECOV_TOKEN` secret is set
- Check coverage files are being generated

## Local Testing

Test the workflow locally using:

```bash
# Build
dotnet build --configuration Release

# Unit/Integration Tests
dotnet test --filter "TestCategory!=E2E" --collect:"XPlat Code Coverage"

# Start app
dotnet run --project EllisHope &

# E2E Tests
dotnet test --filter "TestCategory=E2E"
```

## Next Steps

1. ? Set up `CODECOV_TOKEN` secret
2. ? Commit and push `.github/workflows/ci-cd.yml`
3. ? Watch the first workflow run
4. ? Add badges to README.md
5. ? Configure deployment (if needed)

---

**Your CI/CD pipeline is now ready!** ??

Every push will:
- Run all tests automatically
- Generate code coverage reports
- Run Playwright E2E tests
- Deploy on `main` branch (when configured)

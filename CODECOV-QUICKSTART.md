# Codecov Integration - Quick Start

## ? Setup Complete!

Your Ellis Hope Foundation project is now configured for code coverage reporting with Codecov.

## What Was Configured

### 1. GitHub Actions Workflow (`.github/workflows/dotnet-ci.yml`)
? Updated to use `codecov-action@v5` (latest version)  
? Added `--results-directory ./coverage` for organized output  
? Configured to upload `coverage.cobertura.xml` files  
? Uses your `CODECOV_TOKEN` secret  

### 2. Codecov Configuration (`codecov.yml`)
? Coverage range: 70-100%  
? Ignores migrations, static files, views  
? Configured PR comments with coverage diffs  
? Status checks enabled  

### 3. Test Project
? Already has `coverlet.collector` v6.0.4  
? Generates Cobertura XML format  
? Compatible with Codecov  

## Next Steps

### 1. Commit and Push Changes
```bash
git add .github/workflows/dotnet-ci.yml codecov.yml CODECOV-SETUP.md
git commit -m "ci: add Codecov integration for code coverage reporting"
git push origin main
```

### 2. Watch the Build
1. Go to https://github.com/mcarthey/ellishopefoundation.org/actions
2. Click on the latest workflow run
3. Watch for "Upload coverage reports to Codecov" step
4. Verify it completes successfully ?

### 3. Check Codecov Dashboard
1. Go to https://app.codecov.io/gh/mcarthey/ellishopefoundation.org
2. You should see your coverage report within a few minutes
3. Explore the coverage by file/directory

### 4. Add Coverage Badge to README
Once the first upload succeeds, add this badge to your `README.md`:

```markdown
[![codecov](https://codecov.io/gh/mcarthey/ellishopefoundation.org/branch/main/graph/badge.svg?token=YOUR_TOKEN)](https://codecov.io/gh/mcarthey/ellishopefoundation.org)
```

*Note: You can get the exact badge markdown from your Codecov dashboard under Settings ? Badge*

## Verification Checklist

- [x] `CODECOV_TOKEN` added to GitHub repository secrets ?
- [x] Workflow updated to use `codecov-action@v5` ?
- [x] `codecov.yml` created with configuration ?
- [x] Coverage generation tested locally ?
- [ ] Changes committed and pushed
- [ ] GitHub Actions build passes
- [ ] Coverage appears on Codecov dashboard
- [ ] Badge added to README

## Expected Coverage

Based on your 45 unit tests:

| Component | Expected Coverage |
|-----------|------------------|
| BlogService | ~95% (22 tests) |
| EventService | ~95% (23 tests) |
| Overall Project | ~70-80% |

## Troubleshooting

### If coverage doesn't appear on Codecov:

1. **Check GitHub Actions logs**
   - Look for "Upload coverage reports to Codecov" step
   - Check for any error messages

2. **Verify token**
   - Ensure `CODECOV_TOKEN` is correctly set in GitHub Secrets
   - Token should have no extra spaces

3. **Check Codecov status**
   - Visit https://status.codecov.io/
   - Ensure service is operational

4. **Verify coverage files**
   - Check Actions logs for: `Searching for coverage files...`
   - Should find files matching: `./coverage/**/coverage.cobertura.xml`

### If build fails:

The workflow is configured with `fail_ci_if_error: false`, so:
- Failed coverage upload won't fail the build
- Check logs but your tests should still run

## Additional Resources

?? Full documentation: See `CODECOV-SETUP.md`  
?? CI/CD guide: See `CI-CD-SETUP.md`  
?? Codecov dashboard: https://app.codecov.io/gh/mcarthey/ellishopefoundation.org  

---

**Ready to go!** ?? Just commit, push, and watch the coverage reports appear!

# Git Move Script - Preserves file history
# Reorganizes documentation into /docs folder structure

Write-Host "Creating /docs folder structure..." -ForegroundColor Cyan
Write-Host ""

# Create directories
$directories = @("docs", "docs/development", "docs/deployment", "docs/security")
foreach ($dir in $directories) {
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }
}

Write-Host "Moving files with Git (preserves history)..." -ForegroundColor Cyan
Write-Host ""

# Function to move file with git
function Move-WithGit {
    param($source, $destination)
    if (Test-Path $source) {
        git mv $source $destination
        Write-Host "? Moved: $source ? $destination" -ForegroundColor Green
        return $true
    } else {
        Write-Host "? Not found: $source" -ForegroundColor Yellow
        return $false
    }
}

# Development docs
Move-WithGit "CONFIGURATION.md" "docs/development/configuration.md"
Move-WithGit "CI-CD-SETUP.md" "docs/development/ci-cd-setup.md"
Move-WithGit "SECRETS-MANAGEMENT.md" "docs/development/secrets-management.md"
Move-WithGit "TINYMCE-SETUP.md" "docs/development/tinymce-setup.md"

# Deployment docs
Move-WithGit "DEPLOYMENT-GUIDE.md" "docs/deployment/deployment-guide.md"
Move-WithGit "HTTPS-SETUP-GUIDE.md" "docs/deployment/https-setup-guide.md"
Move-WithGit "HTTPS-SETUP-CHECKLIST.md" "docs/deployment/https-setup-checklist.md"
Move-WithGit "POST-SSL-CHECKLIST.md" "docs/deployment/post-ssl-checklist.md"
Move-WithGit "QUICK-REFERENCE.md" "docs/deployment/quick-reference.md"

# Security docs
Move-WithGit "ENCRYPTED-CONFIGURATION.md" "docs/security/encrypted-configuration.md"
Move-WithGit "HTTPS-CONFIGURATION.md" "docs/security/https-configuration.md"

Write-Host ""
Write-Host "? Documentation reorganization complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Review the changes: git status" -ForegroundColor White
Write-Host "  2. Stage new files: git add docs/" -ForegroundColor White
Write-Host "  3. Commit: git commit -m 'docs: Reorganize documentation into /docs folder'" -ForegroundColor White
Write-Host "  4. Push: git push origin main" -ForegroundColor White
Write-Host ""

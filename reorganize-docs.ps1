# Documentation Reorganization Script
# This script moves documentation files to the /docs folder structure

Write-Host "Moving documentation files to /docs folder..." -ForegroundColor Cyan
Write-Host ""

# Create directories if they don't exist
$directories = @(
    "docs",
    "docs/deployment",
    "docs/security",
    "docs/development"
)

foreach ($dir in $directories) {
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        Write-Host "Created: $dir" -ForegroundColor Green
    }
}

# Define file movements
$moves = @{
    # Development docs
    "CONFIGURATION.md" = "docs/development/configuration.md"
    "CI-CD-SETUP.md" = "docs/development/ci-cd-setup.md"
    "SECRETS-MANAGEMENT.md" = "docs/development/secrets-management.md"
    "TINYMCE-SETUP.md" = "docs/development/tinymce-setup.md"
    
    # Deployment docs
    "DEPLOYMENT-GUIDE.md" = "docs/deployment/deployment-guide.md"
    "HTTPS-SETUP-GUIDE.md" = "docs/deployment/https-setup-guide.md"
    "HTTPS-SETUP-CHECKLIST.md" = "docs/deployment/https-setup-checklist.md"
    "POST-SSL-CHECKLIST.md" = "docs/deployment/post-ssl-checklist.md"
    "QUICK-REFERENCE.md" = "docs/deployment/quick-reference.md"
    
    # Security docs
    "ENCRYPTED-CONFIGURATION.md" = "docs/security/encrypted-configuration.md"
    "HTTPS-CONFIGURATION.md" = "docs/security/https-configuration.md"
}

Write-Host ""
Write-Host "Moving files..." -ForegroundColor Cyan
Write-Host ""

foreach ($source in $moves.Keys) {
    $destination = $moves[$source]
    
    if (Test-Path $source) {
        Move-Item -Path $source -Destination $destination -Force
        Write-Host "? Moved: $source ? $destination" -ForegroundColor Green
    } else {
        Write-Host "? Not found: $source" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "Files that should stay in root:" -ForegroundColor Cyan
Write-Host "  - README.md (main project readme)" -ForegroundColor Gray
Write-Host "  - .gitignore" -ForegroundColor Gray
Write-Host "  - LICENSE (if you have one)" -ForegroundColor Gray
Write-Host ""

Write-Host "? Documentation reorganization complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Review the new docs/ structure" -ForegroundColor White
Write-Host "  2. Update any internal links in documentation" -ForegroundColor White
Write-Host "  3. Commit changes to Git" -ForegroundColor White
Write-Host "  4. Push to GitHub" -ForegroundColor White
Write-Host ""

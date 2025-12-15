# Documentation Cleanup Script
# Removes transactional development documentation
# Keeps: ADMIN-GUIDE.md, DEVELOPER-GUIDE.md, README.md

Write-Host "Ellis Hope Foundation - Documentation Cleanup" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Files/Folders to KEEP
$keepFiles = @(
    "docs\ADMIN-GUIDE.md",
    "docs\DEVELOPER-GUIDE.md",
    "README.md"
)

# Get all markdown files
$allDocs = Get-ChildItem -Path "docs" -Filter "*.md" -Recurse

Write-Host "Found $($allDocs.Count) documentation files" -ForegroundColor Yellow
Write-Host ""

# Files to delete
$filesToDelete = @()

foreach ($doc in $allDocs) {
    $relativePath = $doc.FullName.Replace((Get-Location).Path + "\", "")
    
    if ($keepFiles -notcontains $relativePath) {
        $filesToDelete += $doc
    }
}

Write-Host "Files to DELETE ($($filesToDelete.Count)):" -ForegroundColor Red
Write-Host "----------------------------------------" -ForegroundColor Red
foreach ($file in $filesToDelete) {
    $relativePath = $file.FullName.Replace((Get-Location).Path + "\", "")
    Write-Host "  - $relativePath" -ForegroundColor Red
}
Write-Host ""

Write-Host "Files to KEEP ($($keepFiles.Count)):" -ForegroundColor Green
Write-Host "----------------------------------------" -ForegroundColor Green
foreach ($file in $keepFiles) {
    Write-Host "  + $file" -ForegroundColor Green
}
Write-Host ""

# Prompt for confirmation
$response = Read-Host "Delete $($filesToDelete.Count) transactional documentation files? (yes/no)"

if ($response -eq "yes") {
    Write-Host ""
    Write-Host "Deleting files..." -ForegroundColor Yellow
    
    foreach ($file in $filesToDelete) {
        Remove-Item $file.FullName -Force
        Write-Host "  Deleted: $($file.Name)" -ForegroundColor Gray
    }
    
    Write-Host ""
    Write-Host "Cleanup complete!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Remaining documentation:" -ForegroundColor Cyan
    Write-Host "  - README.md (project overview)" -ForegroundColor Cyan
    Write-Host "  - docs/ADMIN-GUIDE.md (for site administrators)" -ForegroundColor Cyan
    Write-Host "  - docs/DEVELOPER-GUIDE.md (for developers)" -ForegroundColor Cyan
    Write-Host ""
    
    # Clean up empty directories
    Write-Host "Removing empty directories..." -ForegroundColor Yellow
    Get-ChildItem -Path "docs" -Directory -Recurse | 
        Where-Object { (Get-ChildItem $_.FullName -Recurse | Measure-Object).Count -eq 0 } |
        Remove-Item -Force
    
    Write-Host "Done! Repository documentation is now clean and organized." -ForegroundColor Green
}
else {
    Write-Host ""
    Write-Host "Cleanup cancelled. No files were deleted." -ForegroundColor Yellow
}

Write-Host ""

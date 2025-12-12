#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Creates a GitHub PR with detailed description and auto-merges it.

.DESCRIPTION
    This script creates a pull request from the current branch to main,
    auto-populates the description from commit messages, merges it,
    and cleans up the branch.

.PARAMETER Title
    The title for the pull request. If not provided, uses the first commit message.

.PARAMETER AutoMerge
    If specified, automatically merges the PR after creation. Default is true.

.EXAMPLE
    .\merge-branch.ps1
    .\merge-branch.ps1 -Title "Add admin dashboard and fix mobile menu"
#>

param(
    [string]$Title = "",
    [bool]$AutoMerge = $true
)

# Ensure we're in a git repository
if (-not (Test-Path .git)) {
    Write-Error "Not in a git repository root directory"
    exit 1
}

# Get current branch name
$currentBranch = git rev-parse --abbrev-ref HEAD
if ($currentBranch -eq "main" -or $currentBranch -eq "master") {
    Write-Error "Cannot create PR from main/master branch. Please switch to a feature branch."
    exit 1
}

Write-Host "Current branch: $currentBranch" -ForegroundColor Cyan

# Check if there are unpushed commits
$unpushedCommits = git log origin/$currentBranch..$currentBranch --oneline 2>$null
if ($unpushedCommits) {
    Write-Host "`nUnpushed commits detected. Pushing to origin..." -ForegroundColor Yellow
    git push -u origin $currentBranch
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to push commits"
        exit 1
    }
}

# Get commit messages for PR body
Write-Host "`nGathering commit messages..." -ForegroundColor Cyan
$commits = git log origin/main..$currentBranch --pretty=format:"- %s%n%b" | Where-Object { $_ -ne "" }

# Generate PR body from commits
$prBody = @"
## Summary

This PR includes the following changes:

$($commits -join "`n")

## Changes Made

$($commits | ForEach-Object {
    if ($_ -match '^- (.+)$') {
        "- $($matches[1])"
    }
} | Select-Object -Unique | Out-String)

---
*Auto-generated PR description from commit messages*
"@

# Use first commit subject as title if not provided
if ([string]::IsNullOrWhiteSpace($Title)) {
    $Title = git log -1 --pretty=format:"%s"
}

Write-Host "`nCreating PR with title: $Title" -ForegroundColor Green
Write-Host "`nPR Description:" -ForegroundColor Cyan
Write-Host $prBody -ForegroundColor Gray

# Create the PR
Write-Host "`nCreating pull request..." -ForegroundColor Cyan

# Save PR body to temp file (gh CLI handles multiline better from file)
$tempFile = [System.IO.Path]::GetTempFileName()
$prBody | Out-File -FilePath $tempFile -Encoding UTF8

try {
    $prUrl = gh pr create --title $Title --body-file $tempFile --base main 2>&1

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to create PR: $prUrl"
        exit 1
    }

    Write-Host "`n✓ PR created successfully!" -ForegroundColor Green
    Write-Host $prUrl -ForegroundColor Blue

    if ($AutoMerge) {
        Write-Host "`nMerging PR..." -ForegroundColor Cyan
        gh pr merge --merge --delete-branch

        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ PR merged and branch deleted!" -ForegroundColor Green

            # Switch to main and pull
            Write-Host "`nSwitching to main and pulling latest changes..." -ForegroundColor Cyan
            git checkout main
            git pull origin main

            # Delete local branch if it still exists
            $branchExists = git branch --list $currentBranch
            if ($branchExists) {
                git branch -d $currentBranch 2>$null
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "✓ Local branch '$currentBranch' deleted" -ForegroundColor Green
                }
            }

            Write-Host "`n✓ All done! You're now on main with the latest changes." -ForegroundColor Green
        } else {
            Write-Error "Failed to merge PR"
            exit 1
        }
    } else {
        Write-Host "`nPR created but not merged (AutoMerge=false)" -ForegroundColor Yellow
        Write-Host "To merge manually, run: gh pr merge --merge --delete-branch" -ForegroundColor Yellow
    }
} finally {
    # Clean up temp file
    Remove-Item $tempFile -ErrorAction SilentlyContinue
}

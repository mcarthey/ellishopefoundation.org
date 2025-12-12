#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Quick merge - Creates and merges PR with one command

.EXAMPLE
    .\quick-merge.ps1
    .\quick-merge.ps1 "Add new feature"
#>

param(
    [string]$Title = ""
)

& "$PSScriptRoot\merge-branch.ps1" -Title $Title -AutoMerge $true

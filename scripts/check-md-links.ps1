param(
    [string[]] $Targets = @('README.md', 'docs', 'EllisHope.Tests')
)

$root = Get-Location
$mds = @()
foreach ($t in $Targets) {
    if (Test-Path $t) {
        $item = Get-Item $t
        if ($item.PSIsContainer) {
            $mds += Get-ChildItem -Path $t -Recurse -Filter *.md -ErrorAction SilentlyContinue | Where-Object { $_.FullName -notmatch '\\node_modules\\' -and $_.FullName -notmatch '\\bin\\' -and $_.FullName -notmatch '\\obj\\' }
        } else {
            $mds += $item
        }
    }
}
$broken = @()

foreach ($md in $mds) {
    $text = Get-Content $md.FullName -Raw
    $pattern = '\[[^\]]+\]\(([^)]+)\)'
    $matches = [regex]::Matches($text, $pattern)
    foreach ($m in $matches) {
        $link = $m.Groups[1].Value.Trim()
        if ($link -match '^(http|https|mailto):') { continue }
        $clean = $link -replace '#.*$','' -replace '\?.*$',''
        if ([string]::IsNullOrWhiteSpace($clean)) { continue }
        if ($clean.StartsWith('/')) {
            $candidate = Join-Path $root $clean.TrimStart('/')
        } else {
            $candidate = Join-Path $md.Directory.FullName $clean
        }
        if (-not (Test-Path $candidate)) {
            $broken += [pscustomobject]@{File=$md.FullName; Link=$link; ResolvedPath=$candidate}
        }
    }
}
if ($broken.Count -eq 0) {
    Write-Host 'No broken relative links found.'
    exit 0
} else {
    $broken | Sort-Object File | Format-Table -AutoSize
    exit 2
}
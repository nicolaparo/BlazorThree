[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $repoRoot

$ProjectPath = "demo/BlazorThree.Demo.BlazorWebAssembly/BlazorThree.Demo.BlazorWebAssembly.csproj"
$Configuration = "Release"
$Framework = "net10.0"
$BaseHref = "/BlazorThree/"
$OutputPath = "artifacts/github-pages/site"
$CleanOutput = $true

$resolvedProjectPath = Join-Path $repoRoot $ProjectPath
if (-not (Test-Path $resolvedProjectPath)) {
    throw "Project not found at '$resolvedProjectPath'."
}

if (-not $BaseHref.StartsWith('/')) {
    $BaseHref = "/$BaseHref"
}
if (-not $BaseHref.EndsWith('/')) {
    $BaseHref = "$BaseHref/"
}

$publishRoot = Join-Path $repoRoot "artifacts/github-pages/publish"
if (Test-Path $publishRoot) {
    Remove-Item $publishRoot -Recurse -Force
}

Write-Host "Publishing WebAssembly demo..." -ForegroundColor Cyan
& dotnet publish $resolvedProjectPath -c $Configuration -f $Framework -o $publishRoot
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

$siteSource = if (Test-Path (Join-Path $publishRoot "wwwroot/index.html")) {
    Join-Path $publishRoot "wwwroot"
} elseif (Test-Path (Join-Path $publishRoot "index.html")) {
    $publishRoot
} else {
    throw "Could not find index.html in publish output."
}

$indexPath = Join-Path $siteSource "index.html"
$indexHtml = Get-Content $indexPath -Raw
$baseReplacement = "<base href=`"$BaseHref`" />"
$indexHtml = [regex]::Replace($indexHtml, '<base\s+href="[^"]*"\s*/>', $baseReplacement)
Set-Content -Path $indexPath -Value $indexHtml -NoNewline

Copy-Item -Path $indexPath -Destination (Join-Path $siteSource "404.html") -Force
Set-Content -Path (Join-Path $siteSource ".nojekyll") -Value "" -NoNewline

$resolvedOutputPath = Join-Path $repoRoot $OutputPath
if (-not (Test-Path $resolvedOutputPath)) {
    New-Item -ItemType Directory -Path $resolvedOutputPath -Force | Out-Null
}

if ($CleanOutput) {
    Get-ChildItem -Path $resolvedOutputPath -Force |
        Where-Object { $_.Name -ne "res" } |
        Remove-Item -Recurse -Force
}

Write-Host "Copying static site to '$resolvedOutputPath'..." -ForegroundColor Cyan
Copy-Item -Path (Join-Path $siteSource "*") -Destination $resolvedOutputPath -Recurse -Force

Write-Host "Done." -ForegroundColor Green
Write-Host "Output folder: $resolvedOutputPath"
Write-Host "Base href set to: $BaseHref"
Write-Host ""
Write-Host "Published with one command: ./scripts/publish-blazor-wasm-ghpages.ps1"

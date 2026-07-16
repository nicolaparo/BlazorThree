[CmdletBinding()]
param(
    [string]$ProjectPath = "demo/BlazorThree.Demo.BlazorWebAssembly/BlazorThree.Demo.BlazorWebAssembly.csproj",
    [string]$TargetRepoPath = "../BlazorThree-demo",
    [string]$Configuration = "Release",
    [string]$Framework = "net10.0",
    [string]$BaseHref = "/BlazorThree-demo/",
    [string]$PublishRootPath = "artifacts/blazorthree-demo-publish",
    [string]$CommitMessage = "Publish BlazorThree demo site",
    [switch]$SkipGitPublish
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $repoRoot

$resolvedProjectPath = Join-Path $repoRoot $ProjectPath
if (-not (Test-Path $resolvedProjectPath)) {
    throw "Project not found at '$resolvedProjectPath'."
}

$resolvedTargetRepoPath = [System.IO.Path]::GetFullPath((Join-Path $repoRoot $TargetRepoPath))
if (-not (Test-Path $resolvedTargetRepoPath)) {
    throw "Target repo folder not found at '$resolvedTargetRepoPath'."
}

if (-not (Test-Path (Join-Path $resolvedTargetRepoPath ".git"))) {
    throw "Target repo folder '$resolvedTargetRepoPath' is not a Git repository."
}

if (-not $BaseHref.StartsWith('/')) {
    $BaseHref = "/$BaseHref"
}
if (-not $BaseHref.EndsWith('/')) {
    $BaseHref = "$BaseHref/"
}

$publishRoot = Join-Path $repoRoot $PublishRootPath
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

Set-Content -Path (Join-Path $resolvedTargetRepoPath ".gitattributes") -Value "* -text" -NoNewline

Write-Host "Copying static site to '$resolvedTargetRepoPath'..." -ForegroundColor Cyan
Get-ChildItem -Path $resolvedTargetRepoPath -Force |
    Where-Object { $_.Name -notin ".git", ".gitattributes", "README.md" } |
    Remove-Item -Recurse -Force

Copy-Item -Path (Join-Path $siteSource "*") -Destination $resolvedTargetRepoPath -Recurse -Force

if ($SkipGitPublish) {
    Write-Host "Done." -ForegroundColor Green
    Write-Host "Target folder: $resolvedTargetRepoPath"
    Write-Host "Base href set to: $BaseHref"
    Write-Host "Git publish skipped." -ForegroundColor Yellow
    exit 0
}

Write-Host "Publishing target repo to GitHub..." -ForegroundColor Cyan
& git -C $resolvedTargetRepoPath add --all
if ($LASTEXITCODE -ne 0) {
    throw "git add failed with exit code $LASTEXITCODE."
}

& git -C $resolvedTargetRepoPath diff --cached --quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "Done." -ForegroundColor Green
    Write-Host "Target folder: $resolvedTargetRepoPath"
    Write-Host "Base href set to: $BaseHref"
    Write-Host "No changes to commit." -ForegroundColor Yellow
    exit 0
}

& git -C $resolvedTargetRepoPath commit -m $CommitMessage
if ($LASTEXITCODE -ne 0) {
    throw "git commit failed with exit code $LASTEXITCODE."
}

& git -C $resolvedTargetRepoPath push
if ($LASTEXITCODE -ne 0) {
    throw "git push failed with exit code $LASTEXITCODE."
}

Write-Host "Done." -ForegroundColor Green
Write-Host "Target folder: $resolvedTargetRepoPath"
Write-Host "Base href set to: $BaseHref"
Write-Host ""
Write-Host "Published with one command: ./scripts/publish-blazor-wasm-ghpages.ps1"

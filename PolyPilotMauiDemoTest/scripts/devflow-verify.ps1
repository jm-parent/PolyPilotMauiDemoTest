param(
    [switch]$Launch,
    [switch]$SkipBuild,
    [string]$Framework = "net10.0-windows10.0.19041.0",
    [string]$Project = "PolyPilotMauiDemoTest.csproj"
)

$ErrorActionPreference = "Stop"

function Write-Step {
    param([string]$Message)
    Write-Host "`n==> $Message" -ForegroundColor Cyan
}

$projectPath = Join-Path (Get-Location) $Project
if (-not (Test-Path $projectPath)) {
    throw "Projet introuvable: $projectPath"
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "La commande 'dotnet' est introuvable dans le PATH."
}

if (-not (Get-Command maui-devflow -ErrorAction SilentlyContinue)) {
    throw "La commande 'maui-devflow' est introuvable dans le PATH."
}

if (-not $SkipBuild) {
    Write-Step "Build Debug ($Framework)"
    dotnet build -t:Build -p:Configuration=Debug -f $Framework -p:WindowsPackageType=None $projectPath
}

if ($Launch) {
    Write-Step "Lancement de l'app (target Run)"
    Start-Process -FilePath "dotnet" -ArgumentList @(
        "build",
        "-t:Run",
        "-p:Configuration=Debug",
        "-f", $Framework,
        "-p:WindowsPackageType=None",
        $projectPath
    ) | Out-Null

    Write-Step "Attente de l'agent MauiDevFlow"
    maui-devflow wait
}
else {
    Write-Step "Mode vérification sans lancement"
    Write-Host "Lance l'application en Debug si tu veux un status connecté." -ForegroundColor Yellow
}

Write-Step "Agents enregistrés"
maui-devflow list

Write-Step "Status de l'agent MAUI"
maui-devflow MAUI status

Write-Host "`nTerminé." -ForegroundColor Green

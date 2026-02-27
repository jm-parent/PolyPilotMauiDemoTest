param(
    [switch]$Launch,
    [switch]$SkipBuild,
    [string]$Framework = "net10.0-windows10.0.19041.0",
    [string]$Project = "PolyPilotMauiDemoTest.csproj",
    [string]$OutputRoot = "artifacts/devflow-smoke"
)

$ErrorActionPreference = "Stop"

function Write-Step {
    param([string]$Message)
    Write-Host "`n==> $Message" -ForegroundColor Cyan
}

function Invoke-Capture {
    param(
        [string]$Title,
        [scriptblock]$Action,
        [string]$OutFile
    )

    Write-Step $Title
    try {
        $result = & $Action 2>&1 | Out-String
        $result | Out-File -FilePath $OutFile -Encoding utf8
        Write-Host "OK -> $OutFile" -ForegroundColor Green
    }
    catch {
        $msg = "ERROR: $($_.Exception.Message)"
        $msg | Out-File -FilePath $OutFile -Encoding utf8
        Write-Host $msg -ForegroundColor Yellow
    }
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

$stamp = Get-Date -Format "yyyyMMdd-HHmmss"
$outDir = Join-Path (Get-Location) (Join-Path $OutputRoot $stamp)
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

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
}

Write-Step "Attente de l'agent"
$port = $null
try {
    $waitOutput = (maui-devflow wait 2>&1 | Out-String).Trim()
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($waitOutput)) {
        throw "Aucun agent détecté. Output: $waitOutput"
    }

    $portMatch = [regex]::Match($waitOutput, "(\d{2,5})")
    if (-not $portMatch.Success) {
        throw "Impossible d'extraire le port depuis: $waitOutput"
    }

    $port = $portMatch.Value
    "Port: $port" | Out-File -FilePath (Join-Path $outDir "port.txt") -Encoding utf8
}
catch {
    $errorMessage = "ERREUR WAIT: $($_.Exception.Message)"
    $errorMessage | Out-File -FilePath (Join-Path $outDir "wait-error.txt") -Encoding utf8
    Write-Host $errorMessage -ForegroundColor Yellow
    Invoke-Capture -Title "Agents list" -Action { maui-devflow list } -OutFile (Join-Path $outDir "list.txt")
    throw "Agent introuvable. Relance avec -Launch ou démarre l'app en Debug. Artefacts: $outDir"
}

Invoke-Capture -Title "Agents list" -Action { maui-devflow list } -OutFile (Join-Path $outDir "list.txt")
Invoke-Capture -Title "MAUI status" -Action { maui-devflow MAUI status --agent-port $port } -OutFile (Join-Path $outDir "status.txt")
Invoke-Capture -Title "Navigate Dashboard" -Action { maui-devflow MAUI navigate --agent-port $port //dashboard } -OutFile (Join-Path $outDir "navigate-dashboard.txt")
Invoke-Capture -Title "Navigate Settings" -Action { maui-devflow MAUI navigate --agent-port $port //settings } -OutFile (Join-Path $outDir "navigate-settings.txt")
Invoke-Capture -Title "Navigate Dashboard (return)" -Action { maui-devflow MAUI navigate --agent-port $port //dashboard } -OutFile (Join-Path $outDir "navigate-dashboard-return.txt")
Invoke-Capture -Title "MAUI tree" -Action { maui-devflow MAUI tree --agent-port $port --depth 6 } -OutFile (Join-Path $outDir "tree.txt")
Invoke-Capture -Title "MAUI query (Buttons)" -Action { maui-devflow MAUI query --agent-port $port --type Button } -OutFile (Join-Path $outDir "buttons.txt")
Invoke-Capture -Title "MAUI logs" -Action { maui-devflow MAUI logs --agent-port $port --limit 200 } -OutFile (Join-Path $outDir "logs.txt")

Write-Step "Capture screenshot"
try {
    maui-devflow MAUI screenshot --agent-port $port --output (Join-Path $outDir "screen.png") | Out-Null
    Write-Host "OK -> $(Join-Path $outDir "screen.png")" -ForegroundColor Green
}
catch {
    $msg = "ERROR screenshot: $($_.Exception.Message)"
    $msg | Out-File -FilePath (Join-Path $outDir "screen-error.txt") -Encoding utf8
    Write-Host $msg -ForegroundColor Yellow
}

Write-Host "`nSmoke test terminé. Artefacts: $outDir" -ForegroundColor Green

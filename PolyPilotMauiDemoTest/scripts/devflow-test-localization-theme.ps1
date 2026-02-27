param(
    [switch]$Launch,
    [switch]$SkipBuild,
    [string]$Framework = "net10.0-windows10.0.19041.0",
    [string]$Project = "PolyPilotMauiDemoTest.csproj",
    [string]$OutputRoot = "artifacts/devflow-localization-theme"
)

$ErrorActionPreference = "Stop"

function Write-Step {
    param([string]$Message)
    Write-Host "`n==> $Message" -ForegroundColor Cyan
}

function Invoke-DevFlow {
    param(
        [string]$Command,
        [switch]$AllowFailure
    )

    $output = Invoke-Expression $Command 2>&1 | Out-String
    $code = $LASTEXITCODE
    if (-not $AllowFailure -and $code -ne 0) {
        throw "Commande en échec ($code): $Command`n$output"
    }
    return $output.Trim()
}

function Save-Text {
    param([string]$Path, [string]$Content)
    $Content | Out-File -FilePath $Path -Encoding utf8
}

function Assert-Contains {
    param(
        [string]$Haystack,
        [string]$Needle,
        [string]$Message
    )

    if ($Haystack -notmatch [regex]::Escape($Needle)) {
        throw "Assertion échouée: $Message`nAttendu: '$Needle'"
    }
    Write-Host "PASS: $Message" -ForegroundColor Green
}

function Get-PickerIds {
    param([string]$Port)

    $queryOutput = Invoke-DevFlow "maui-devflow MAUI query --agent-port $Port --type Picker"
    $lines = $queryOutput -split "`r?`n"
    $ids = @()

    foreach ($line in $lines) {
        if ($line -match "\[([^\]]+)\]\s+Picker") {
            $ids += $matches[1]
        }
    }

    if ($ids.Count -lt 2) {
        throw "Impossible de trouver les 2 Pickers Settings. Sortie:`n$queryOutput"
    }

    return ,$ids
}

$projectPath = Join-Path (Get-Location) $Project
if (-not (Test-Path $projectPath)) {
    throw "Projet introuvable: $projectPath"
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
$waitOutput = Invoke-DevFlow "maui-devflow wait"
$portMatch = [regex]::Match($waitOutput, "(\d{2,5})")
if (-not $portMatch.Success) {
    throw "Port agent introuvable dans: $waitOutput"
}
$port = $portMatch.Value
Save-Text -Path (Join-Path $outDir "port.txt") -Content "Port: $port"

Write-Step "Etat initial"
$status = Invoke-DevFlow "maui-devflow MAUI status --agent-port $port"
Save-Text -Path (Join-Path $outDir "status.txt") -Content $status

Write-Step "Vérification langue EN sur Dashboard"
Invoke-DevFlow "maui-devflow MAUI navigate --agent-port $port //dashboard" -AllowFailure | Out-Null
$helloEn = Invoke-DevFlow "maui-devflow MAUI query --agent-port $port --text \"Hello, World!\""
Save-Text -Path (Join-Path $outDir "hello-en.txt") -Content $helloEn
Assert-Contains -Haystack $helloEn -Needle "Found" -Message "Texte anglais détecté"

Write-Step "Navigation vers Settings"
Invoke-DevFlow "maui-devflow MAUI navigate --agent-port $port //settings" | Out-Null

$pickerIds = Get-PickerIds -Port $port
$themePickerId = $pickerIds[0]
$languagePickerId = $pickerIds[1]
Save-Text -Path (Join-Path $outDir "picker-ids.txt") -Content "ThemePicker=$themePickerId`nLanguagePicker=$languagePickerId"

Write-Step "Test thème: Dark puis Light puis System"
Invoke-DevFlow "maui-devflow MAUI set-property --agent-port $port $themePickerId SelectedIndex 1" | Out-Null
$themeDark = Invoke-DevFlow "maui-devflow MAUI property --agent-port $port $themePickerId SelectedIndex"
Assert-Contains -Haystack $themeDark -Needle "1" -Message "Theme sélectionné sur Dark"

Invoke-DevFlow "maui-devflow MAUI screenshot --agent-port $port --output $(Join-Path $outDir "theme-dark.png")" | Out-Null

Invoke-DevFlow "maui-devflow MAUI set-property --agent-port $port $themePickerId SelectedIndex 0" | Out-Null
$themeLight = Invoke-DevFlow "maui-devflow MAUI property --agent-port $port $themePickerId SelectedIndex"
Assert-Contains -Haystack $themeLight -Needle "0" -Message "Theme sélectionné sur Light"

Invoke-DevFlow "maui-devflow MAUI screenshot --agent-port $port --output $(Join-Path $outDir "theme-light.png")" | Out-Null

Invoke-DevFlow "maui-devflow MAUI set-property --agent-port $port $themePickerId SelectedIndex 2" | Out-Null
$themeSystem = Invoke-DevFlow "maui-devflow MAUI property --agent-port $port $themePickerId SelectedIndex"
Assert-Contains -Haystack $themeSystem -Needle "2" -Message "Theme sélectionné sur System"

Write-Step "Test localisation: bascule en FR"
Invoke-DevFlow "maui-devflow MAUI set-property --agent-port $port $languagePickerId SelectedIndex 1" | Out-Null
Start-Sleep -Milliseconds 600

$settingsFr = Invoke-DevFlow "maui-devflow MAUI query --agent-port $port --text \"Langue\""
Save-Text -Path (Join-Path $outDir "settings-fr.txt") -Content $settingsFr
Assert-Contains -Haystack $settingsFr -Needle "Found" -Message "Labels Settings traduits en FR"

Invoke-DevFlow "maui-devflow MAUI navigate --agent-port $port //dashboard" -AllowFailure | Out-Null
Start-Sleep -Milliseconds 400
$helloFr = Invoke-DevFlow "maui-devflow MAUI query --agent-port $port --text \"Bonjour, Monde !\""
Save-Text -Path (Join-Path $outDir "hello-fr.txt") -Content $helloFr
Assert-Contains -Haystack $helloFr -Needle "Found" -Message "Texte français détecté sur Dashboard"
Invoke-DevFlow "maui-devflow MAUI screenshot --agent-port $port --output $(Join-Path $outDir "dashboard-fr.png")" | Out-Null

Write-Step "Restauration langue EN"
Invoke-DevFlow "maui-devflow MAUI navigate --agent-port $port //settings" | Out-Null
Start-Sleep -Milliseconds 300
Invoke-DevFlow "maui-devflow MAUI set-property --agent-port $port $languagePickerId SelectedIndex 0" | Out-Null
Start-Sleep -Milliseconds 600

Invoke-DevFlow "maui-devflow MAUI navigate --agent-port $port //dashboard" -AllowFailure | Out-Null
$helloEnAgain = Invoke-DevFlow "maui-devflow MAUI query --agent-port $port --text \"Hello, World!\""
Save-Text -Path (Join-Path $outDir "hello-en-restored.txt") -Content $helloEnAgain
Assert-Contains -Haystack $helloEnAgain -Needle "Found" -Message "Retour en anglais confirmé"

Write-Step "Logs finaux"
$logs = Invoke-DevFlow "maui-devflow MAUI logs --agent-port $port --limit 200"
Save-Text -Path (Join-Path $outDir "logs.txt") -Content $logs

Write-Host "`nTest localisation + thème réussi. Artefacts: $outDir" -ForegroundColor Green

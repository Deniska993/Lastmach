$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionPath = Join-Path $projectRoot "ImagePlatformSorter.sln"
$outputPath = Join-Path $projectRoot "BuildOutput"

dotnet restore $solutionPath --ignore-failed-sources
dotnet build $solutionPath -c Release --no-restore "-p:OutDir=$outputPath\"

New-Item -ItemType Directory -Force -Path $outputPath | Out-Null
Copy-Item (Join-Path $projectRoot "config.example.json") (Join-Path $outputPath "config.example.json") -Force

Write-Host "Build ready at: $outputPath"

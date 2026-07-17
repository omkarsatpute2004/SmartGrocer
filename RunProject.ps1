# SmartGrocer Run Script
# This script handles the spaces in the directory path to ensure the project runs correctly.

$ProjectPath = Join-Path $PSScriptRoot "SmartGrocer"
$CsprojPath = Join-Path $ProjectPath "SmartGrocer.csproj"

Write-Host "--- Starting SmartGrocer ---" -ForegroundColor Cyan
Write-Host "Using Project: $CsprojPath"

dotnet run --project "$CsprojPath" --urls "http://localhost:5078"

# SmartGrocer Run Script
# This script handles the spaces in the directory path to ensure the project runs correctly.

$CsprojPath = Join-Path $PSScriptRoot "SmartGrocer.csproj"

Write-Host "--- Starting SmartGrocer ---" -ForegroundColor Cyan
Write-Host "Using Project: $CsprojPath"

dotnet run --project "$CsprojPath" --urls "http://localhost:5078"

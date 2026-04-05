# QSS Platform — Local Development Launcher (Windows PowerShell)
# Starts both the API (port 5000) and Web (port 5001) concurrently

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  QSS Platform - Local Development" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# Check .NET is installed
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "ERROR: .NET SDK not found. Install from https://dot.net" -ForegroundColor Red
    exit 1
}

Write-Host ".NET version: $(dotnet --version)"

# Build the solution first
Write-Host "`nBuilding solution..." -ForegroundColor Yellow
dotnet build QSS.sln -c Debug --nologo -v q
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "`nStarting QSS API on http://localhost:5000 ..." -ForegroundColor Green
$apiProcess = Start-Process -FilePath "dotnet" `
    -ArgumentList "run --no-build --project src/QSS.API/QSS.API.csproj" `
    -WorkingDirectory $PSScriptRoot `
    -PassThru `
    -NoNewWindow

Start-Sleep -Seconds 4

Write-Host "Starting QSS Web on http://localhost:5001 ..." -ForegroundColor Green
$webProcess = Start-Process -FilePath "dotnet" `
    -ArgumentList "run --no-build --project src/QSS.Web/QSS.Web.csproj" `
    -WorkingDirectory $PSScriptRoot `
    -PassThru `
    -NoNewWindow

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  QSS Platform is running!" -ForegroundColor Cyan
Write-Host "  API:     http://localhost:5000/swagger" -ForegroundColor White
Write-Host "  Web UI:  http://localhost:5001" -ForegroundColor White
Write-Host ""
Write-Host "  Default credentials:" -ForegroundColor White
Write-Host "  superadmin@qss.com / Admin@1234!" -ForegroundColor Yellow
Write-Host "  admin@qss.com / Admin@1234!" -ForegroundColor Yellow
Write-Host "  dentist@qss.com / Admin@1234!" -ForegroundColor Yellow
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press Ctrl+C to stop all services"

# Open browser
Start-Sleep -Seconds 2
Start-Process "http://localhost:5001"

$apiProcess.WaitForExit()

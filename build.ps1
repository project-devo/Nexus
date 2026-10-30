# Build and test the Nexus File Manager application
Write-Host "Building Nexus File Manager..." -ForegroundColor Cyan

# Ensure local dotnet is in PATH if installed in user directory
if (!(Get-Command dotnet -ErrorAction SilentlyContinue)) {
    if (Test-Path "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe") {
        $env:PATH = "$env:LOCALAPPDATA\Microsoft\dotnet;" + $env:PATH
    }
}

# Check if dotnet is installed
try {
    $dotnetVersion = dotnet --version
    Write-Host "Found .NET SDK version: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "ERROR: .NET SDK is not installed!" -ForegroundColor Red
    Write-Host "Download from: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
    exit 1
}

# Restore packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Cyan
dotnet restore Nexus.sln

# Build Solution
Write-Host "Building solution..." -ForegroundColor Cyan
dotnet build Nexus.sln -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nBuild failed!" -ForegroundColor Red
    exit 1
}

# Run Tests
Write-Host "Running unit tests..." -ForegroundColor Cyan
dotnet test Nexus.sln -c Release --no-build

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nBuild and tests successful!" -ForegroundColor Green
    Write-Host "Run the app with: dotnet run --project FileManager.csproj" -ForegroundColor Cyan
} else {
    Write-Host "`nTests failed!" -ForegroundColor Red
    exit 1
}

# Build the File Manager application
Write-Host "Building File Manager..." -ForegroundColor Cyan

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
dotnet restore

# Build
Write-Host "Building project..." -ForegroundColor Cyan
dotnet build -c Release

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nBuild successful!" -ForegroundColor Green
    Write-Host "Run the app with: dotnet run" -ForegroundColor Cyan
} else {
    Write-Host "`nBuild failed!" -ForegroundColor Red
    exit 1
}

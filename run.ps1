if (!(Get-Command dotnet -ErrorAction SilentlyContinue)) {
    if (Test-Path "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe") {
        $env:PATH = "$env:LOCALAPPDATA\Microsoft\dotnet;" + $env:PATH
    }
}

Write-Host "Starting Nexus File Manager..." -ForegroundColor Cyan
dotnet run --project FileManager.csproj

@echo off
setlocal
where dotnet >nul 2>nul
if %errorlevel% neq 0 (
    if exist "%LOCALAPPDATA%\Microsoft\dotnet\dotnet.exe" (
        set "PATH=%LOCALAPPDATA%\Microsoft\dotnet;%PATH%"
    )
)

echo Starting Nexus File Manager...
dotnet run --project FileManager.csproj

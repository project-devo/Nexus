@echo off
setlocal
echo ===================================
echo   Building Nexus File Manager
echo ===================================

:: Ensure dotnet is in PATH if installed in local appdata
where dotnet >nul 2>nul
if %errorlevel% neq 0 (
    if exist "%LOCALAPPDATA%\Microsoft\dotnet\dotnet.exe" (
        set "PATH=%LOCALAPPDATA%\Microsoft\dotnet;%PATH%"
    )
)

where dotnet >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERROR] .NET SDK is not found!
    echo Please install .NET 8 SDK or ensure it is in your PATH.
    pause
    exit /b 1
)

echo.
echo [1/3] Restoring NuGet packages...
dotnet restore Nexus.sln
if %errorlevel% neq 0 (
    echo [ERROR] Restore failed!
    exit /b 1
)

echo.
echo [2/3] Building solution (Release)...
dotnet build Nexus.sln -c Release
if %errorlevel% neq 0 (
    echo [ERROR] Build failed!
    exit /b 1
)

echo.
echo [3/3] Running automated unit tests...
dotnet test Nexus.sln -c Release --no-build
if %errorlevel% neq 0 (
    echo [ERROR] Tests failed!
    exit /b 1
)

echo.
echo ===================================
echo   Build and Tests Succeeded!
echo   Run app: dotnet run --project FileManager.csproj
echo ===================================

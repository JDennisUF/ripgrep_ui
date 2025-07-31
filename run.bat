@echo off
echo Starting Ripgrep UI...
echo.
echo Prerequisites:
echo - .NET 8.0 SDK must be installed
echo - ripgrep (rg) must be installed and in PATH
echo.
echo Testing ripgrep installation...
rg --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: ripgrep (rg) is not installed or not in PATH
    echo Please install ripgrep from: https://github.com/BurntSushi/ripgrep/releases
    echo Or use: winget install BurntSushi.ripgrep.MSVC
    pause
    exit /b 1
)
echo ripgrep is installed ✓
echo.
echo Restoring packages...
dotnet restore
if errorlevel 1 (
    echo ERROR: Failed to restore packages
    pause
    exit /b 1
)
echo.
echo Starting application...
echo The browser should open automatically
echo If not, navigate to: https://localhost:5001
echo.
dotnet run
pause
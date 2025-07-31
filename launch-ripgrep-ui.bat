@echo off
echo Starting Ripgrep UI...
echo.
echo Checking for ripgrep...
rg --version >nul 2>&1
if errorlevel 1 (
    echo WARNING: ripgrep (rg.exe) not found in PATH
    echo Please install ripgrep first:
    echo   winget install BurntSushi.ripgrep.MSVC
    echo.
    pause
    exit /b 1
)

echo Starting server...
echo Web UI will be available at: http://localhost:5000
echo Press Ctrl+C to stop the server
echo.

REM Start the application
RipgrepUI.exe --standalone

pause
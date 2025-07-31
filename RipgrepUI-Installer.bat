@echo off
setlocal enabledelayedexpansion

echo ========================================
echo    Ripgrep UI - Windows Installer
echo ========================================
echo.

REM Check if running as administrator
net session >nul 2>&1
if !errorlevel! neq 0 (
    echo This installer should be run as Administrator for best results.
    echo Right-click and select "Run as Administrator"
    echo.
    pause
)

REM Create installation directory
set INSTALL_DIR=C:\Program Files\RipgrepUI
if not exist "%INSTALL_DIR%" (
    echo Creating installation directory: %INSTALL_DIR%
    mkdir "%INSTALL_DIR%"
)

REM Check if ripgrep is installed
echo Checking for ripgrep installation...
rg --version >nul 2>&1
if !errorlevel! neq 0 (
    echo ❌ ripgrep (rg.exe) is not installed or not in PATH
    echo.
    echo Please install ripgrep first:
    echo Option 1: winget install BurntSushi.ripgrep.MSVC
    echo Option 2: Download from https://github.com/BurntSushi/ripgrep/releases
    echo Option 3: choco install ripgrep
    echo.
    pause
    exit /b 1
) else (
    echo ✅ ripgrep is installed
)

REM Check for .NET 8 Runtime
echo Checking for .NET 8 installation...
dotnet --version >nul 2>&1
if !errorlevel! neq 0 (
    echo ❌ .NET runtime not found
    echo Please install .NET 8 Runtime from:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
) else (
    echo ✅ .NET runtime is installed
)

REM Copy files (assuming they're in publish-framework\win-x64\)
if exist "publish-framework\win-x64\RipgrepUI.exe" (
    echo Copying application files...
    xcopy "publish-framework\win-x64\*" "%INSTALL_DIR%\" /E /Y
    
    REM Create desktop shortcut
    echo Creating desktop shortcut...
    set SHORTCUT_PATH=%USERPROFILE%\Desktop\Ripgrep UI.lnk
    powershell -Command "$WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%SHORTCUT_PATH%'); $Shortcut.TargetPath = '%INSTALL_DIR%\RipgrepUI.exe'; $Shortcut.WorkingDirectory = '%INSTALL_DIR%'; $Shortcut.IconLocation = '%INSTALL_DIR%\RipgrepUI.exe'; $Shortcut.Description = 'Ripgrep UI - Fast File Search'; $Shortcut.Save()"
    
    REM Create start menu entry
    echo Creating start menu entry...
    set STARTMENU_PATH=%APPDATA%\Microsoft\Windows\Start Menu\Programs
    if not exist "%STARTMENU_PATH%\Ripgrep UI" mkdir "%STARTMENU_PATH%\Ripgrep UI"
    powershell -Command "$WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%STARTMENU_PATH%\Ripgrep UI\Ripgrep UI.lnk'); $Shortcut.TargetPath = '%INSTALL_DIR%\RipgrepUI.exe'; $Shortcut.WorkingDirectory = '%INSTALL_DIR%'; $Shortcut.IconLocation = '%INSTALL_DIR%\RipgrepUI.exe'; $Shortcut.Description = 'Ripgrep UI - Fast File Search'; $Shortcut.Save()"
    
    echo.
    echo ✅ Installation completed successfully!
    echo.
    echo Installation location: %INSTALL_DIR%
    echo Desktop shortcut: %USERPROFILE%\Desktop\Ripgrep UI.lnk
    echo Start menu: Programs\Ripgrep UI
    echo.
    echo To run Ripgrep UI:
    echo 1. Double-click the desktop shortcut, OR
    echo 2. Run from Start Menu, OR
    echo 3. Navigate to %INSTALL_DIR% and run RipgrepUI.exe
    echo 4. Open your browser to http://localhost:5000
    echo.
    
    choice /M "Would you like to start Ripgrep UI now"
    if !errorlevel! equ 1 (
        echo Starting Ripgrep UI...
        start "" "%INSTALL_DIR%\RipgrepUI.exe"
        timeout /t 3 >nul
        start "" "http://localhost:5000"
    )
    
) else (
    echo ❌ RipgrepUI.exe not found in publish-framework\win-x64\
    echo Please run build-windows-framework.bat first
    pause
    exit /b 1
)

pause
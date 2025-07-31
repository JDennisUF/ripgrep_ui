@echo off
echo Building Ripgrep UI for Windows...
echo.

REM Clean previous builds
if exist "publish" rmdir /s /q publish

REM Build self-contained Windows executable
echo Building self-contained Windows executable...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -o publish\win-x64

if errorlevel 1 (
    echo ERROR: Build failed
    pause
    exit /b 1
)

echo.
echo ✅ Build successful!
echo.
echo Executable location: publish\win-x64\RipgrepUI.exe
echo File size: 
for %%A in ("publish\win-x64\RipgrepUI.exe") do echo %%~zA bytes

echo.
echo To run:
echo 1. Copy the entire publish\win-x64\ folder to target machine
echo 2. Ensure ripgrep (rg.exe) is installed and in PATH
echo 3. Double-click RipgrepUI.exe
echo 4. Navigate to http://localhost:5000 in your browser
echo.

pause
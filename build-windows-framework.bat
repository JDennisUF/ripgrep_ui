@echo off
echo Building Ripgrep UI for Windows (Framework Dependent)...
echo.

REM Clean previous builds
if exist "publish-framework" rmdir /s /q publish-framework

REM Build framework-dependent Windows executable (smaller size, requires .NET 8 runtime)
echo Building framework-dependent Windows executable...
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o publish-framework\win-x64

if errorlevel 1 (
    echo ERROR: Build failed
    pause
    exit /b 1
)

echo.
echo ✅ Build successful!
echo.
echo Executable location: publish-framework\win-x64\RipgrepUI.exe
echo File size: 
for %%A in ("publish-framework\win-x64\RipgrepUI.exe") do echo %%~zA bytes

echo.
echo To run:
echo 1. Ensure .NET 8 Runtime is installed on target machine
echo 2. Copy the entire publish-framework\win-x64\ folder to target machine
echo 3. Ensure ripgrep (rg.exe) is installed and in PATH
echo 4. Double-click RipgrepUI.exe
echo 5. Navigate to http://localhost:5000 in your browser
echo.
echo Download .NET 8 Runtime: https://dotnet.microsoft.com/download/dotnet/8.0

pause
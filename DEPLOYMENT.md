# Ripgrep UI - Windows Deployment Guide

## Compilation Options

### Option 1: Self-Contained Executable (Recommended)
**Pros:** No .NET runtime required on target machine
**Cons:** Larger file size (~70-100MB)

```bash
# Run this command:
build-windows.bat

# Creates: publish\win-x64\RipgrepUI.exe
```

### Option 2: Framework-Dependent Executable  
**Pros:** Smaller file size (~10-20MB)
**Cons:** Requires .NET 8 Runtime on target machine

```bash
# Run this command:
build-windows-framework.bat

# Creates: publish-framework\win-x64\RipgrepUI.exe
```

## Manual Build Commands

```bash
# Self-contained (no runtime required)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -o publish\win-x64

# Framework-dependent (requires .NET 8 runtime)
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o publish-framework\win-x64

# Cross-platform builds
dotnet publish -c Release -r win-x86 --self-contained true -o publish\win-x86     # 32-bit Windows
dotnet publish -c Release -r linux-x64 --self-contained true -o publish\linux   # Linux
dotnet publish -c Release -r osx-x64 --self-contained true -o publish\macos     # macOS
```

## Installation Methods

### Method 1: Simple Copy
1. Build using one of the batch files above
2. Copy the entire `publish\win-x64\` folder to target machine
3. Double-click `RipgrepUI.exe`

### Method 2: Full Installation
1. Build using `build-windows-framework.bat`
2. Run `RipgrepUI-Installer.bat` as Administrator
3. Creates desktop shortcut and start menu entry

## Prerequisites for Target Machine

### Required:
- **ripgrep (rg.exe)** must be installed and in PATH
  ```bash
  # Install options:
  winget install BurntSushi.ripgrep.MSVC
  # OR
  choco install ripgrep
  # OR download from: https://github.com/BurntSushi/ripgrep/releases
  ```

### For Framework-Dependent Build:
- **.NET 8 Runtime** (download from: https://dotnet.microsoft.com/download/dotnet/8.0)

## Running the Application

### Command Line:
```bash
RipgrepUI.exe --standalone
```

### Batch File:
```bash
launch-ripgrep-ui.bat
```

### Direct:
1. Double-click `RipgrepUI.exe`
2. Open browser to `http://localhost:5000`

## Configuration

### Default Settings:
- **Port:** 5000 (HTTP)
- **Interface:** localhost only (secure)
- **Auto-browser:** No (manual navigation required)

### Custom Port:
```bash
RipgrepUI.exe --urls "http://localhost:8080"
```

### Network Access (Use with caution):
```bash
RipgrepUI.exe --urls "http://*:5000"
```

## File Size Comparison

| Build Type | Approximate Size | Dependencies |
|------------|------------------|--------------|
| Self-Contained | 70-100 MB | None |
| Framework-Dependent | 10-20 MB | .NET 8 Runtime |
| Development | 1-5 MB | .NET 8 SDK |

## Distribution

### For End Users:
1. Use **self-contained build** for easiest deployment
2. Include `launch-ripgrep-ui.bat` for easy startup
3. Package with ripgrep installer or instructions

### For Developers:
1. Use **framework-dependent build** 
2. Assume .NET 8 Runtime is available
3. Include source code and build scripts

## Troubleshooting

### "rg command not found":
- Install ripgrep and ensure it's in system PATH
- Test with `rg --version` in command prompt

### "This application requires .NET 8":
- Install .NET 8 Runtime from Microsoft
- Or use self-contained build instead

### Port already in use:
- Use custom port: `RipgrepUI.exe --urls "http://localhost:8080"`
- Or find and stop conflicting service

### Browser doesn't auto-open:
- Manually navigate to `http://localhost:5000`
- Check Windows Firewall settings
- Ensure localhost isn't blocked

## Security Notes

- Application runs on localhost only by default (secure)
- No external network access unless explicitly configured
- File system access limited to search operations only
- No file modification capabilities built-in
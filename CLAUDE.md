# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Ripgrep UI is a Blazor Server application that provides a web-based interface for ripgrep (rg), allowing users to perform fast file searches with a user-friendly GUI. The application can run as both a development web server and be compiled into standalone Windows executables.

## Architecture

### Core Components
- **Blazor Server**: Real-time UI updates using SignalR
- **RipgrepService**: Executes ripgrep via Process.Start, parses JSON output
- **DirectoryBrowserService**: Server-side directory navigation and file system access
- **SearchModel/SearchResult**: Data models for search parameters and results

### Key Design Patterns
- **Service Layer**: Business logic separated into scoped services (RipgrepService, DirectoryBrowserService)
- **Process Execution**: External ripgrep binary execution with JSON output parsing
- **Modal Components**: Reusable DirectoryBrowser modal for file system navigation
- **Real-time Updates**: Blazor Server enables live search progress without page reloads

## Development Commands

### Basic Development
```bash
# Restore dependencies
dotnet restore

# Run development server
dotnet run

# Build project
dotnet build
```

### Windows Executable Builds
```bash
# Self-contained executable (no .NET runtime required)
build-windows.bat
# Creates: publish\win-x64\RipgrepUI.exe (~70-100MB)

# Framework-dependent executable (requires .NET 8 runtime)
build-windows-framework.bat  
# Creates: publish-framework\win-x64\RipgrepUI.exe (~10-20MB)
```

### Manual Publish Commands
```bash
# Self-contained
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -o publish\win-x64

# Framework-dependent
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o publish-framework\win-x64
```

## Prerequisites & Dependencies

### Development Requirements
- .NET 8.0 SDK
- ripgrep (rg) installed and in PATH

### Runtime Dependencies
- ripgrep (rg.exe) must be available in system PATH
- For framework-dependent builds: .NET 8 Runtime on target machine

## Key Implementation Details

### Ripgrep Integration
- Uses `--json` flag for structured output parsing
- Command line construction in `RipgrepService.BuildRipgrepArguments()`
- Returns both search results and the actual command line executed
- Supports all major ripgrep options: case sensitivity, file types, context lines, etc.

### File System Access
- `DirectoryBrowserService` provides server-side directory browsing
- Discovers common development directories automatically
- Handles Windows-specific paths and drive enumeration
- Permission-aware directory traversal with error handling

### Standalone Executable Mode
- Program.cs detects `--standalone` argument to disable HTTPS redirect
- Designed for localhost-only access by default
- Can be configured for network access with `--urls` parameter

### UI Features
- Fixed-width form controls to prevent layout shifts
- Clickable file paths and line numbers with copy-to-clipboard functionality
- Command generation for VS Code (`code "file:line"`) and Notepad++ 
- Responsive design with viewport width constraints to prevent horizontal scrolling

## File Structure Significance

- **Services/**: Contains all business logic and external process interaction
- **Components/**: Reusable UI components like DirectoryBrowser modal
- **Models/**: Data transfer objects for search operations
- **wwwroot/js/**: Client-side JavaScript for file system helpers (limited by browser security)
- **Build scripts**: Multiple deployment options for different distribution scenarios

## Testing Ripgrep Integration

Since the application depends on external ripgrep binary:
- Verify `rg --version` works in development environment  
- Test with various search patterns and file types
- Validate JSON output parsing with edge cases
- Ensure process cleanup and error handling work correctly
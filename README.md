# Ripgrep UI

A Blazor Server application that provides a user-friendly web interface for ripgrep (rg), making it easy to search through files on Windows.

## Features

- **Intuitive Search Interface**: Clean web UI with form inputs for all common ripgrep options
- **Real-time Results**: Fast search results with syntax highlighting
- **File Type Filtering**: Support for filtering by file extensions and patterns
- **Advanced Options**: Case sensitivity, whole word matching, context lines, hidden files
- **Results Display**: Organized results with file names, line numbers, and highlighted matches

## Prerequisites

1. **.NET 8.0 SDK** - Download from [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)
2. **Ripgrep (rg)** - Must be installed and available in your PATH
   - Download from [https://github.com/BurntSushi/ripgrep/releases](https://github.com/BurntSushi/ripgrep/releases)
   - Or install via package manager:
     - Windows:
       - `winget install BurntSushi.ripgrep.MSVC`
       - `choco install ripgrep`
     - Ensure `rg` command is available from command prompt

## Installation & Running

1. **Clone or download this project**
2. **Open command prompt in the project directory**
3. **Restore dependencies:**
   ```bash
   dotnet restore
   ```
4. **Run the application:**
   ```bash
   dotnet run
   ```
5. **Open your browser** and navigate to the URL shown in the console (typically `https://localhost:5001` or `http://localhost:5000`)

## Building Windows Executable

To create a standalone Windows .exe file:

### Self-Contained (Recommended)
```bash
build-windows.bat
```
Creates `publish\win-x64\RipgrepUI.exe` (~70-100MB) - no .NET runtime required on target machine.

### Framework-Dependent
```bash
build-windows-framework.bat
```
Creates `publish-framework\win-x64\RipgrepUI.exe` (~10-20MB) - requires .NET 8 Runtime on target machine.

For detailed deployment instructions, installation options, and troubleshooting, see [DEPLOYMENT.md](DEPLOYMENT.md).

## Usage

1. **Enter Search Pattern**: Type your regex pattern or plain text search
2. **Set Directory**: Specify the directory to search in (defaults to your user profile)
3. **Configure Options**:
   - File Types: Comma-separated patterns like `*.cs,*.js,*.py`
   - Case Sensitive: Toggle case sensitivity
   - Whole Word: Match whole words only
   - Include Hidden: Search hidden files and directories
   - Context Lines: Show surrounding lines for context
   - Max Results: Limit the number of results

4. **Click Search** and view results with syntax highlighting

## Project Structure

```
RipgrepUI/
├── Models/           # Data models (SearchModel, SearchResult)
├── Services/         # Business logic (RipgrepService)
├── Pages/           # Razor pages (Index, History)
├── Shared/          # Layout components (MainLayout, NavMenu)
├── wwwroot/css/     # Static CSS files
└── Program.cs       # Application entry point
```

## Technical Details

- **Framework**: ASP.NET Core Blazor Server
- **UI**: Bootstrap 5 for responsive design
- **Process Execution**: Uses System.Diagnostics.Process to execute ripgrep
- **JSON Parsing**: Parses ripgrep's JSON output for structured results
- **Real-time Updates**: Blazor Server enables real-time UI updates

## Troubleshooting

**"rg command not found" error:**
- Ensure ripgrep is installed and in your PATH
- Try running `rg --version` in command prompt to verify installation

**Search not working:**
- Check that the directory path exists and is accessible
- Ensure you have read permissions for the search directory
- Verify the search pattern is valid regex syntax

## Future Enhancements

- Search history and saved searches
- File preview functionality
- Export search results
- Configurable syntax highlighting themes
- Directory browser dialog
- Performance optimizations for large result sets
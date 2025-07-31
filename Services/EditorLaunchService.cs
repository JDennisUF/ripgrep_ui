using System.Diagnostics;
using RipgrepUI.Models;

namespace RipgrepUI.Services;

public class EditorLaunchService
{
    private readonly ILogger<EditorLaunchService> _logger;
    private readonly EditorSettingsService _settingsService;

    public EditorLaunchService(ILogger<EditorLaunchService> logger, EditorSettingsService settingsService)
    {
        _logger = logger;
        _settingsService = settingsService;
    }

    public bool LaunchPreferredEditor(string filePath, int? lineNumber = null)
    {
        var settings = _settingsService.GetSettings();
        var editorInfo = EditorInfo.SupportedEditors.FirstOrDefault(e => e.Type == settings.PreferredEditor);
        
        if (editorInfo == null)
        {
            _logger.LogWarning("Unknown editor type: {EditorType}", settings.PreferredEditor);
            return false;
        }

        return LaunchEditor(editorInfo, filePath, lineNumber, settings);
    }

    private bool LaunchEditor(EditorInfo editorInfo, string filePath, int? lineNumber, EditorSettings settings)
    {
        try
        {
            if (editorInfo.Type == EditorType.SystemDefault)
            {
                return LaunchSystemDefault(filePath);
            }

            if (editorInfo.Type == EditorType.Custom)
            {
                return LaunchCustomEditor(filePath, lineNumber, settings);
            }

            // Try each executable name for the editor
            foreach (var executable in editorInfo.ExecutableNames)
            {
                if (TryLaunchExecutable(executable, editorInfo, filePath, lineNumber))
                {
                    return true;
                }
            }

            _logger.LogWarning("No working executable found for {EditorType}", editorInfo.Type);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to launch {EditorType} for file: {FilePath}", editorInfo.Type, filePath);
            return false;
        }
    }

    private bool TryLaunchExecutable(string executable, EditorInfo editorInfo, string filePath, int? lineNumber)
    {
        try
        {
            // Validate that the file exists
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("File does not exist: {FilePath}", filePath);
                return false;
            }

            // Get absolute path to ensure proper file opening
            var absolutePath = Path.GetFullPath(filePath);
            
            // Handle Flatpak commands specially
            if (executable.StartsWith("flatpak run"))
            {
                return TryLaunchFlatpakApp(executable, editorInfo, absolutePath, lineNumber);
            }

            string arguments;            
            if (lineNumber.HasValue && editorInfo.SupportsLineNumbers)
            {
                arguments = string.Format(editorInfo.LineArgument, absolutePath, lineNumber.Value);
            }
            else
            {
                arguments = string.Format(editorInfo.FileArgument, absolutePath);
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = executable,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            _logger.LogInformation("Launching {Editor}: {Executable} {Arguments}", editorInfo.DisplayName, executable, arguments);
            _logger.LogInformation("File exists check: {FilePath} -> {Exists}", absolutePath, File.Exists(absolutePath));
            
            using var process = Process.Start(startInfo);
            return process != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to launch {Executable} with file {FilePath}", executable, filePath);
            return false;
        }
    }

    private bool TryLaunchFlatpakApp(string flatpakCommand, EditorInfo editorInfo, string filePath, int? lineNumber)
    {
        try
        {
            // Extract app ID from "flatpak run app.id"
            var parts = flatpakCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                _logger.LogWarning("Invalid Flatpak command format: {Command}", flatpakCommand);
                return false;
            }

            var appId = parts[2];
            var flatpakArgs = new List<string> { "run", appId };

            // Add file arguments
            if (lineNumber.HasValue && editorInfo.SupportsLineNumbers)
            {
                var lineArgs = string.Format(editorInfo.LineArgument, filePath, lineNumber.Value);
                flatpakArgs.AddRange(lineArgs.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            }
            else
            {
                var fileArgs = string.Format(editorInfo.FileArgument, filePath);
                flatpakArgs.AddRange(fileArgs.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "flatpak",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            foreach (var arg in flatpakArgs)
            {
                startInfo.ArgumentList.Add(arg);
            }

            _logger.LogInformation("Launching Flatpak {Editor}: flatpak {Arguments}", editorInfo.DisplayName, string.Join(" ", flatpakArgs));
            
            using var process = Process.Start(startInfo);
            return process != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to launch Flatpak app {Command} with file {FilePath}", flatpakCommand, filePath);
            return false;
        }
    }

    private bool LaunchSystemDefault(string filePath)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            };

            _logger.LogInformation("Launching system default editor for: {FilePath}", filePath);
            
            using var process = Process.Start(startInfo);
            return process != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to launch system default editor for file: {FilePath}", filePath);
            return false;
        }
    }

    private bool LaunchCustomEditor(string filePath, int? lineNumber, EditorSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.CustomEditorPath))
        {
            _logger.LogWarning("Custom editor path not configured");
            return false;
        }

        try
        {
            var arguments = settings.CustomEditorArgs ?? "\"{0}\"";
            arguments = string.Format(arguments, filePath, lineNumber ?? 1);

            var startInfo = new ProcessStartInfo
            {
                FileName = settings.CustomEditorPath,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            _logger.LogInformation("Launching custom editor: {Path} {Arguments}", settings.CustomEditorPath, arguments);
            
            using var process = Process.Start(startInfo);
            return process != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to launch custom editor: {Path}", settings.CustomEditorPath);
            return false;
        }
    }

    public async Task<EditorAvailability> CheckEditorsAsync()
    {
        var availability = new EditorAvailability();
        
        // Check VS Code
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "code",
                Arguments = "--version",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            });
            
            if (process != null)
            {
                await process.WaitForExitAsync();
                availability.VSCodeAvailable = process.ExitCode == 0;
            }
        }
        catch
        {
            availability.VSCodeAvailable = false;
        }

        // Check Notepad++
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "notepad++",
                Arguments = "-?",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            });
            
            if (process != null)
            {
                await process.WaitForExitAsync();
                availability.NotepadPlusPlusAvailable = process.ExitCode == 0;
            }
        }
        catch
        {
            availability.NotepadPlusPlusAvailable = false;
        }

        return availability;
    }
}

public class EditorAvailability
{
    public bool VSCodeAvailable { get; set; }
    public bool NotepadPlusPlusAvailable { get; set; }
}
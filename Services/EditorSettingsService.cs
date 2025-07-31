using System.Text.Json;
using RipgrepUI.Models;

namespace RipgrepUI.Services;

public class EditorSettingsService
{
    private readonly ILogger<EditorSettingsService> _logger;
    private readonly string _settingsPath;
    private EditorSettings _settings;

    public EditorSettingsService(ILogger<EditorSettingsService> logger)
    {
        _logger = logger;
        _settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RipgrepUI", "editor-settings.json");
        _settings = LoadSettings();
    }

    public EditorSettings GetSettings() => _settings;

    public async Task SaveSettingsAsync(EditorSettings settings)
    {
        try
        {
            _settings = settings;
            
            var directory = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_settingsPath, json);
            
            _logger.LogInformation("Editor settings saved to {Path}", _settingsPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save editor settings");
        }
    }

    private EditorSettings LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                var settings = JsonSerializer.Deserialize<EditorSettings>(json);
                if (settings != null)
                {
                    _logger.LogInformation("Editor settings loaded from {Path}", _settingsPath);
                    return settings;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load editor settings, using defaults");
        }

        return new EditorSettings();
    }

    public async Task<Dictionary<EditorType, bool>> CheckEditorAvailabilityAsync()
    {
        var availability = new Dictionary<EditorType, bool>();

        foreach (var editor in EditorInfo.SupportedEditors)
        {
            if (editor.Type == EditorType.SystemDefault)
            {
                availability[editor.Type] = true; // System default is always available
                continue;
            }

            bool isAvailable = false;
            foreach (var executable in editor.ExecutableNames)
            {
                try
                {
                    if (executable.StartsWith("flatpak run"))
                    {
                        // Special handling for Flatpak apps
                        var parts = executable.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 3)
                        {
                            var appId = parts[2];
                            using var process = new System.Diagnostics.Process
                            {
                                StartInfo = new System.Diagnostics.ProcessStartInfo
                                {
                                    FileName = "flatpak",
                                    Arguments = "list --app",
                                    UseShellExecute = false,
                                    CreateNoWindow = true,
                                    RedirectStandardOutput = true,
                                    RedirectStandardError = true
                                }
                            };

                            process.Start();
                            await process.WaitForExitAsync();
                            
                            if (process.ExitCode == 0)
                            {
                                var output = await process.StandardOutput.ReadToEndAsync();
                                if (output.Contains(appId))
                                {
                                    isAvailable = true;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Regular executable check
                        using var process = new System.Diagnostics.Process
                        {
                            StartInfo = new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = executable,
                                Arguments = "--version",
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true
                            }
                        };

                        process.Start();
                        await process.WaitForExitAsync();
                        
                        if (process.ExitCode == 0)
                        {
                            isAvailable = true;
                            break;
                        }
                    }
                }
                catch
                {
                    // Try next executable name
                }
            }

            availability[editor.Type] = isAvailable;
        }

        return availability;
    }
}
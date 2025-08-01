using System.Diagnostics;
using System.Text.Json;
using RipgrepUI.Models;

namespace RipgrepUI.Services;

public class RipgrepService
{
    private readonly ILogger<RipgrepService> _logger;

    public RipgrepService(ILogger<RipgrepService> logger)
    {
        _logger = logger;
    }

    public async Task<(List<SearchResult> Results, string CommandLine)> SearchAsync(SearchModel searchModel)
    {
        var results = new List<SearchResult>();
        
        try
        {
            var arguments = BuildRipgrepArguments(searchModel);
            var commandLine = "rg " + string.Join(" ", arguments.Select(arg => arg.Contains(' ') ? $"\"{arg}\"" : arg));
            _logger.LogInformation("Executing ripgrep with arguments: {Arguments}", string.Join(" ", arguments));

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "rg",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = searchModel.Directory
                }
            };

            foreach (var arg in arguments)
            {
                process.StartInfo.ArgumentList.Add(arg);
            }

            var outputLines = new List<string>();
            var errorOutput = new List<string>();

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    outputLines.Add(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorOutput.Add(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0 && process.ExitCode != 1) // Exit code 1 means no matches found, which is normal
            {
                var errorMessage = string.Join("\n", errorOutput);
                throw new InvalidOperationException($"Ripgrep failed with exit code {process.ExitCode}: {errorMessage}");
            }

            results = ParseRipgrepOutput(outputLines, searchModel);
            
            return (results, commandLine);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing ripgrep search");
            throw;
        }
    }

    private List<string> BuildRipgrepArguments(SearchModel searchModel)
    {
        var args = new List<string>();

        // JSON output for easier parsing
        args.Add("--json");

        // Case sensitivity
        if (!searchModel.CaseSensitive)
        {
            args.Add("--ignore-case");
        }

        // Whole word matching
        if (searchModel.WholeWord)
        {
            args.Add("--word-regexp");
        }

        // Context lines
        if (searchModel.ContextLines > 0)
        {
            args.Add("--context");
            args.Add(searchModel.ContextLines.ToString());
        }

        // Max results (count)
        if (searchModel.MaxResults > 0)
        {
            args.Add("--max-count");
            args.Add(searchModel.MaxResults.ToString());
        }

        // Include hidden files
        if (searchModel.IncludeHidden)
        {
            args.Add("--hidden");
        }

        // File type patterns
        if (!string.IsNullOrWhiteSpace(searchModel.FileTypes))
        {
            var patterns = searchModel.FileTypes.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var pattern in patterns)
            {
                args.Add("--glob");
                args.Add(pattern.Trim());
            }
        }

        // Search pattern
        args.Add(searchModel.Pattern);

        // Search directory (if not current directory)
        if (!string.IsNullOrWhiteSpace(searchModel.Directory) && searchModel.Directory != ".")
        {
            args.Add(searchModel.Directory);
        }

        return args;
    }

    private List<SearchResult> ParseRipgrepOutput(List<string> outputLines, SearchModel searchModel)
    {
        var results = new Dictionary<string, SearchResult>();

        foreach (var line in outputLines)
        {
            try
            {
                var jsonDoc = JsonDocument.Parse(line);
                var root = jsonDoc.RootElement;

                if (root.TryGetProperty("type", out var typeElement))
                {
                    var entryType = typeElement.GetString();
                    if (entryType == "match" || entryType == "context")
                    {
                        if (root.TryGetProperty("data", out var dataElement))
                        {
                            var relativePath = dataElement.GetProperty("path").GetProperty("text").GetString() ?? "";
                            var lineNumber = dataElement.GetProperty("line_number").GetInt32();
                            var lineText = dataElement.GetProperty("lines").GetProperty("text").GetString() ?? "";

                            // Convert relative path to absolute path using the search directory
                            var absolutePath = Path.IsPathRooted(relativePath) 
                                ? relativePath 
                                : Path.Combine(searchModel.Directory, relativePath);
                            
                            // Normalize the path
                            absolutePath = Path.GetFullPath(absolutePath);

                            if (!results.ContainsKey(absolutePath))
                            {
                                results[absolutePath] = new SearchResult { File = absolutePath };
                            }

                            results[absolutePath].Matches.Add(new SearchMatch
                            {
                                LineNumber = lineNumber,
                                Content = lineText.TrimEnd('\n', '\r'),
                                IsMatch = entryType == "match"
                            });
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning("Failed to parse ripgrep JSON output: {Line}. Error: {Error}", line, ex.Message);
                // Continue processing other lines
            }
        }

        return results.Values
            .OrderBy(r => r.File)
            .Take(searchModel.MaxResults)
            .ToList();
    }

    public async Task<bool> IsRipgrepAvailableAsync()
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "rg",
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
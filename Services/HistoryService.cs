using System.Text.Json;
using RipgrepUI.Models;

namespace RipgrepUI.Services;

public class HistoryService
{
    private readonly ILogger<HistoryService> _logger;
    private readonly string _historyPath;
    private readonly List<HistoryEntry> _history = new();
    private const int MaxHistoryEntries = 100;

    public HistoryService(ILogger<HistoryService> logger)
    {
        _logger = logger;
        _historyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RipgrepUI", "search-history.json");
        LoadHistory();
    }

    public async Task AddHistoryEntryAsync(HistoryEntry entry)
    {
        try
        {
            // Add to beginning of list (most recent first)
            _history.Insert(0, entry);

            // Keep only the most recent entries
            if (_history.Count > MaxHistoryEntries)
            {
                _history.RemoveRange(MaxHistoryEntries, _history.Count - MaxHistoryEntries);
            }

            await SaveHistoryAsync();
            _logger.LogInformation("Added search to history: {Pattern} in {Directory}", entry.SearchModel.Pattern, entry.SearchModel.Directory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add history entry");
        }
    }

    public List<HistoryEntry> GetHistory()
    {
        return _history.ToList();
    }

    public HistoryEntry? GetHistoryEntry(string id)
    {
        return _history.FirstOrDefault(h => h.Id == id);
    }

    public async Task ClearHistoryAsync()
    {
        try
        {
            _history.Clear();
            await SaveHistoryAsync();
            _logger.LogInformation("Search history cleared");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear history");
        }
    }

    public async Task RemoveHistoryEntryAsync(string id)
    {
        try
        {
            var entry = _history.FirstOrDefault(h => h.Id == id);
            if (entry != null)
            {
                _history.Remove(entry);
                await SaveHistoryAsync();
                _logger.LogInformation("Removed history entry: {Id}", id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove history entry: {Id}", id);
        }
    }

    private async Task SaveHistoryAsync()
    {
        try
        {
            var directory = Path.GetDirectoryName(_historyPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(_history, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_historyPath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save search history");
        }
    }

    private void LoadHistory()
    {
        try
        {
            if (File.Exists(_historyPath))
            {
                var json = File.ReadAllText(_historyPath);
                var history = JsonSerializer.Deserialize<List<HistoryEntry>>(json);
                if (history != null)
                {
                    _history.AddRange(history);
                    _logger.LogInformation("Loaded {Count} search history entries", _history.Count);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load search history, starting with empty history");
        }
    }
}
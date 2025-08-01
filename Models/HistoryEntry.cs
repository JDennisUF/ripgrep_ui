namespace RipgrepUI.Models;

public class HistoryEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string CommandLine { get; set; } = string.Empty;
    public SearchModel SearchModel { get; set; } = new();
    public List<SearchResult> Results { get; set; } = new();
    public TimeSpan Duration { get; set; }
    public int ResultCount { get; set; }
    public bool HasError { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
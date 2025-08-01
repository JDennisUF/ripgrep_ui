namespace RipgrepUI.Models;

public class SearchResult
{
    public string File { get; set; } = string.Empty;
    public List<SearchMatch> Matches { get; set; } = new();
}

public class SearchMatch
{
    public int LineNumber { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsMatch { get; set; } = true;
}
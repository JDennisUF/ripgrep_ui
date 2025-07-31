using System.ComponentModel.DataAnnotations;

namespace RipgrepUI.Models;

public class SearchModel
{
    [Required(ErrorMessage = "Search pattern is required")]
    public string Pattern { get; set; } = string.Empty;

    [Required(ErrorMessage = "Directory is required")]
    public string Directory { get; set; } = string.Empty;

    public string? FileTypes { get; set; }
    public bool CaseSensitive { get; set; }
    public bool WholeWord { get; set; }
    public bool IncludeHidden { get; set; }
    public int ContextLines { get; set; } = 0;
    public int MaxResults { get; set; } = 1000;
}
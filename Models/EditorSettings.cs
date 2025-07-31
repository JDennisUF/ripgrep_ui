namespace RipgrepUI.Models;

public class EditorSettings
{
    public EditorType PreferredEditor { get; set; } = EditorType.VSCode;
    public string? CustomEditorPath { get; set; }
    public string? CustomEditorArgs { get; set; }
}

public enum EditorType
{
    VSCode,
    NotepadPlusPlus,
    NotepadNext,
    Notepad,
    SystemDefault,
    Custom
}

public class EditorInfo
{
    public EditorType Type { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string[] ExecutableNames { get; set; } = Array.Empty<string>();
    public string FileArgument { get; set; } = "\"{0}\"";
    public string LineArgument { get; set; } = "\"{0}:{1}\"";
    public bool SupportsLineNumbers { get; set; } = true;

    public static readonly EditorInfo[] SupportedEditors = new[]
    {
        new EditorInfo
        {
            Type = EditorType.VSCode,
            DisplayName = "Visual Studio Code",
            ExecutableNames = new[] { "code", "code.exe" },
            FileArgument = "\"{0}\"",
            LineArgument = "--goto \"{0}:{1}\"",
            SupportsLineNumbers = true
        },
        new EditorInfo
        {
            Type = EditorType.NotepadPlusPlus,
            DisplayName = "Notepad++",
            ExecutableNames = new[] { "notepad++", "notepad++.exe" },
            FileArgument = "\"{0}\"",
            LineArgument = "-n{1} \"{0}\"",
            SupportsLineNumbers = true
        },
        new EditorInfo
        {
            Type = EditorType.NotepadNext,
            DisplayName = "Notepad Next",
            ExecutableNames = new[] { 
                "notepadnext", "notepadnext.exe", "NotepadNext", "NotepadNext.exe",
                "flatpak run io.github.flathub.notepadnext",
                "flatpak run com.github.dail8859.NotepadNext"
            },
            FileArgument = "\"{0}\"",
            LineArgument = "--line {1} \"{0}\"",
            SupportsLineNumbers = true
        },
        new EditorInfo
        {
            Type = EditorType.Notepad,
            DisplayName = "Notepad",
            ExecutableNames = new[] { "notepad", "notepad.exe" },
            FileArgument = "\"{0}\"",
            LineArgument = "\"{0}\"",
            SupportsLineNumbers = false
        },
        new EditorInfo
        {
            Type = EditorType.SystemDefault,
            DisplayName = "System Default",
            ExecutableNames = Array.Empty<string>(),
            FileArgument = "\"{0}\"",
            LineArgument = "\"{0}\"",
            SupportsLineNumbers = false
        }
    };
}
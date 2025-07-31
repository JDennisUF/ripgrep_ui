namespace RipgrepUI.Services;

public class DirectoryBrowserService
{
    private readonly ILogger<DirectoryBrowserService> _logger;

    public DirectoryBrowserService(ILogger<DirectoryBrowserService> logger)
    {
        _logger = logger;
    }

    public List<string> GetCommonDirectories()
    {
        var commonDirs = new List<string>();

        try
        {
            // User profile directory
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (!string.IsNullOrEmpty(userProfile) && Directory.Exists(userProfile))
                commonDirs.Add(userProfile);

            // Documents folder
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (!string.IsNullOrEmpty(documents) && Directory.Exists(documents))
                commonDirs.Add(documents);

            // Desktop folder
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (!string.IsNullOrEmpty(desktop) && Directory.Exists(desktop))
                commonDirs.Add(desktop);

            // Common development directories
            var commonDevDirs = new[]
            {
                @"C:\Projects",
                @"C:\Code",
                @"C:\Source",
                @"C:\Dev",
                @"C:\Users\" + Environment.UserName + @"\source\repos",
                @"C:\Users\" + Environment.UserName + @"\Projects",
                @"D:\Projects",
                @"D:\Code"
            };

            foreach (var dir in commonDevDirs)
            {
                if (Directory.Exists(dir))
                    commonDirs.Add(dir);
            }

            // Drive roots
            var drives = DriveInfo.GetDrives()
                .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                .Select(d => d.RootDirectory.FullName)
                .ToList();

            commonDirs.AddRange(drives);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting common directories");
        }

        return commonDirs.Distinct().OrderBy(d => d).ToList();
    }

    public List<DirectoryInfo> GetSubDirectories(string path)
    {
        var directories = new List<DirectoryInfo>();

        try
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                return directories;

            var dirInfo = new DirectoryInfo(path);
            directories = dirInfo.GetDirectories()
                .Where(d => !d.Attributes.HasFlag(FileAttributes.Hidden) || 
                           !d.Attributes.HasFlag(FileAttributes.System))
                .OrderBy(d => d.Name)
                .Take(100) // Limit to prevent performance issues
                .ToList();
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogWarning("Access denied to directory: {Path}", path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subdirectories for: {Path}", path);
        }

        return directories;
    }

    public bool IsValidDirectory(string path)
    {
        try
        {
            return !string.IsNullOrWhiteSpace(path) && Directory.Exists(path);
        }
        catch
        {
            return false;
        }
    }

    public string? GetParentDirectory(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            var dirInfo = new DirectoryInfo(path);
            return dirInfo.Parent?.FullName;
        }
        catch
        {
            return null;
        }
    }
}
namespace Nexus.Models;

public class ScanLocation
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public int FileCount { get; set; }
    public bool IsSystemLocation { get; set; }

    public static ScanLocation CreateSystemLocation(string name, string path)
    {
        return new ScanLocation
        {
            Name = name,
            Path = path,
            IsEnabled = true,
            IsSystemLocation = true
        };
    }

    public static ScanLocation CreateCustomLocation(string path)
    {
        var folderName = System.IO.Path.GetFileName(path.TrimEnd(System.IO.Path.DirectorySeparatorChar));
        return new ScanLocation
        {
            Name = string.IsNullOrEmpty(folderName) ? path : folderName,
            Path = path,
            IsEnabled = true,
            IsSystemLocation = false
        };
    }
}

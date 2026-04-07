using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Models;

namespace Nexus.Services;

public class FileScannerService
{
    private static readonly string[] ExcludedDirectories = {
        "$RECYCLE.BIN", "System Volume Information", "Windows", "Program Files",
        "Program Files (x86)", "ProgramData", "PerfLogs"
    };

    public async Task<ObservableCollection<FileItem>> ScanLocationsAsync(
        IEnumerable<ScanLocation> locations,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var allFiles = new ObservableCollection<FileItem>();
        var enabledLocations = locations.Where(l => l.IsEnabled).ToList();
        int totalLocations = enabledLocations.Count;
        int completedLocations = 0;

        foreach (var location in enabledLocations)
        {
            if (cancellationToken.IsCancellationRequested) break;

            if (Directory.Exists(location.Path))
            {
                var files = await Task.Run(() => ScanDirectory(location.Path, location.Name, cancellationToken), cancellationToken);
                foreach (var file in files)
                {
                    allFiles.Add(file);
                }
                location.FileCount = files.Count;
            }

            completedLocations++;
            progress?.Report((completedLocations * 100) / totalLocations);
        }

        return allFiles;
    }

    public async Task<ObservableCollection<FileItem>> ScanSingleFolderAsync(
        string folderPath,
        string locationName,
        CancellationToken cancellationToken = default)
    {
        var files = await Task.Run(() => ScanDirectory(folderPath, locationName, cancellationToken), cancellationToken);
        var collection = new ObservableCollection<FileItem>();
        foreach (var file in files) collection.Add(file);
        return collection;
    }

    private List<FileItem> ScanDirectory(string path, string locationName, CancellationToken cancellationToken)
    {
        var files = new List<FileItem>();

        try
        {
            var dirInfo = new DirectoryInfo(path);

            var directories = GetSafeDirectories(dirInfo);
            foreach (var dir in directories)
            {
                if (cancellationToken.IsCancellationRequested) break;
                if (ExcludedDirectories.Contains(dir.Name, StringComparer.OrdinalIgnoreCase)) continue;

                files.Add(FileItem.FromDirectoryInfo(dir, locationName));

                try
                {
                    files.AddRange(ScanDirectory(dir.FullName, locationName, cancellationToken));
                }
                catch (UnauthorizedAccessException) { }
                catch (IOException) { }
            }

            var fileInfos = GetSafeFiles(dirInfo);
            foreach (var file in fileInfos)
            {
                if (cancellationToken.IsCancellationRequested) break;
                files.Add(FileItem.FromFileInfo(file, locationName));
            }
        }
        catch (UnauthorizedAccessException) { }
        catch (IOException) { }
        catch (Exception) { }

        return files;
    }

    private IEnumerable<DirectoryInfo> GetSafeDirectories(DirectoryInfo dir)
    {
        try
        {
            return dir.GetDirectories();
        }
        catch
        {
            return Array.Empty<DirectoryInfo>();
        }
    }

    private IEnumerable<FileInfo> GetSafeFiles(DirectoryInfo dir)
    {
        try
        {
            return dir.GetFiles();
        }
        catch
        {
            return Array.Empty<FileInfo>();
        }
    }

    public static List<ScanLocation> GetDefaultSystemLocations()
    {
        var locations = new List<ScanLocation>();
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        AddIfValid(locations, "Desktop", Environment.SpecialFolder.DesktopDirectory);
        AddIfValid(locations, "Documents", Environment.SpecialFolder.MyDocuments);
        AddIfValid(locations, "Downloads", Path.Combine(userProfile, "Downloads"));
        AddIfValid(locations, "Pictures", Environment.SpecialFolder.MyPictures);
        AddIfValid(locations, "Music", Environment.SpecialFolder.MyMusic);
        AddIfValid(locations, "Videos", Environment.SpecialFolder.MyVideos);

        return locations;
    }

    private static void AddIfValid(List<ScanLocation> locations, string name, Environment.SpecialFolder folder)
    {
        var path = Environment.GetFolderPath(folder);
        if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
        {
            locations.Add(ScanLocation.CreateSystemLocation(name, path));
        }
    }

    private static void AddIfValid(List<ScanLocation> locations, string name, string path)
    {
        if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
        {
            locations.Add(ScanLocation.CreateSystemLocation(name, path));
        }
    }
}

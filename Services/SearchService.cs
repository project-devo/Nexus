using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Models;

namespace Nexus.Services;

public class SearchService
{
    public async Task<ObservableCollection<FileItem>> SearchAsync(
        ObservableCollection<FileItem> allFiles,
        string searchTerm,
        string? extensionFilter = null,
        FileSizeFilter sizeFilter = FileSizeFilter.Any,
        DateFilter dateFilter = DateFilter.Any,
        bool searchContent = false,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var results = new List<FileItem>();

            foreach (var file in allFiles)
            {
                if (cancellationToken.IsCancellationRequested) break;

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    bool nameMatch = file.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
                    bool contentMatch = false;

                    if (searchContent && !file.IsDirectory && IsTextFile(file.Extension))
                    {
                        contentMatch = SearchFileContent(file.FullName, searchTerm);
                    }

                    if (!nameMatch && !contentMatch) continue;
                }

                if (!string.IsNullOrEmpty(extensionFilter) && extensionFilter != "All")
                {
                    if (!file.Extension.Equals(extensionFilter, StringComparison.OrdinalIgnoreCase)) continue;
                }

                if (!MatchesSizeFilter(file.Size, sizeFilter)) continue;
                if (!MatchesDateFilter(file.DateModified, dateFilter)) continue;

                results.Add(file);
            }

            var collection = new ObservableCollection<FileItem>();
            foreach (var r in results) collection.Add(r);
            return collection;
        }, cancellationToken);
    }

    public ObservableCollection<FileItem> FilterAndSort(
        ObservableCollection<FileItem> files,
        string? searchTerm = null,
        string? extensionFilter = null,
        FileSizeFilter sizeFilter = FileSizeFilter.Any,
        DateFilter dateFilter = DateFilter.Any,
        SortField sortField = SortField.Name,
        SortDirection sortDirection = SortDirection.Ascending)
    {
        var query = files.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLowerInvariant();
            query = query.Where(f => f.Name.ToLowerInvariant().Contains(term));
        }

        if (!string.IsNullOrEmpty(extensionFilter) && extensionFilter != "All")
        {
            query = query.Where(f => f.Extension.Equals(extensionFilter, StringComparison.OrdinalIgnoreCase));
        }

        query = sizeFilter switch
        {
            FileSizeFilter.Tiny => query.Where(f => f.Size < 1024),
            FileSizeFilter.Small => query.Where(f => f.Size >= 1024 && f.Size < 1024 * 100),
            FileSizeFilter.Medium => query.Where(f => f.Size >= 1024 * 100 && f.Size < 1024 * 1024),
            FileSizeFilter.Large => query.Where(f => f.Size >= 1024 * 1024 && f.Size < 1024 * 1024 * 100),
            FileSizeFilter.Huge => query.Where(f => f.Size >= 1024 * 1024 * 100),
            _ => query
        };

        query = dateFilter switch
        {
            DateFilter.Today => query.Where(f => f.DateModified.Date == DateTime.Today),
            DateFilter.ThisWeek => query.Where(f => f.DateModified >= DateTime.Today.AddDays(-7)),
            DateFilter.ThisMonth => query.Where(f => f.DateModified >= DateTime.Today.AddMonths(-1)),
            DateFilter.ThisYear => query.Where(f => f.DateModified >= DateTime.Today.AddYears(-1)),
            DateFilter.Older => query.Where(f => f.DateModified < DateTime.Today.AddYears(-1)),
            _ => query
        };

        query = sortField switch
        {
            SortField.Name => sortDirection == SortDirection.Ascending
                ? query.OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
                : query.OrderByDescending(f => f.Name, StringComparer.OrdinalIgnoreCase),
            SortField.Size => sortDirection == SortDirection.Ascending
                ? query.OrderBy(f => f.Size)
                : query.OrderByDescending(f => f.Size),
            SortField.DateModified => sortDirection == SortDirection.Ascending
                ? query.OrderBy(f => f.DateModified)
                : query.OrderByDescending(f => f.DateModified),
            SortField.DateCreated => sortDirection == SortDirection.Ascending
                ? query.OrderBy(f => f.DateCreated)
                : query.OrderByDescending(f => f.DateCreated),
            SortField.Extension => sortDirection == SortDirection.Ascending
                ? query.OrderBy(f => f.Extension)
                : query.OrderByDescending(f => f.Extension),
            _ => query
        };

        var result = new ObservableCollection<FileItem>();
        foreach (var item in query) result.Add(item);
        return result;
    }

    public List<string> GetUniqueExtensions(ObservableCollection<FileItem> files)
    {
        return files
            .Where(f => !f.IsDirectory && !string.IsNullOrEmpty(f.Extension))
            .Select(f => f.Extension.ToUpperInvariant().TrimStart('.'))
            .Distinct()
            .OrderBy(e => e)
            .ToList();
    }

    private bool MatchesSizeFilter(long size, FileSizeFilter filter)
    {
        return filter switch
        {
            FileSizeFilter.Tiny => size < 1024,
            FileSizeFilter.Small => size >= 1024 && size < 1024 * 100,
            FileSizeFilter.Medium => size >= 1024 * 100 && size < 1024 * 1024,
            FileSizeFilter.Large => size >= 1024 * 1024 && size < 1024 * 1024 * 100,
            FileSizeFilter.Huge => size >= 1024 * 1024 * 100,
            _ => true
        };
    }

    private bool MatchesDateFilter(DateTime date, DateFilter filter)
    {
        return filter switch
        {
            DateFilter.Today => date.Date == DateTime.Today,
            DateFilter.ThisWeek => date >= DateTime.Today.AddDays(-7),
            DateFilter.ThisMonth => date >= DateTime.Today.AddMonths(-1),
            DateFilter.ThisYear => date >= DateTime.Today.AddYears(-1),
            DateFilter.Older => date < DateTime.Today.AddYears(-1),
            _ => true
        };
    }

    private bool IsTextFile(string extension)
    {
        var textExtensions = new[]
        {
            ".txt", ".log", ".md", ".csv", ".json", ".xml", ".html", ".css",
            ".js", ".ts", ".cs", ".py", ".java", ".cpp", ".c", ".h", ".sql",
            ".yaml", ".yml", ".ini", ".cfg", ".conf", ".bat", ".ps1", ".sh"
        };
        return textExtensions.Contains(extension.ToLowerInvariant());
    }

    private bool SearchFileContent(string filePath, string searchTerm)
    {
        try
        {
            using var reader = new StreamReader(filePath);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }
        catch { }
        return false;
    }
}

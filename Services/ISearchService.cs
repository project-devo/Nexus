using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Models;

namespace Nexus.Services;

public interface ISearchService
{
    Task<ObservableCollection<FileItem>> SearchAsync(
        ObservableCollection<FileItem> allFiles,
        string searchTerm,
        string? extensionFilter = null,
        FileSizeFilter sizeFilter = FileSizeFilter.Any,
        DateFilter dateFilter = DateFilter.Any,
        bool searchContent = false,
        CancellationToken cancellationToken = default);

    ObservableCollection<FileItem> FilterAndSort(
        ObservableCollection<FileItem> files,
        string? searchTerm = null,
        string? extensionFilter = null,
        FileSizeFilter sizeFilter = FileSizeFilter.Any,
        DateFilter dateFilter = DateFilter.Any,
        SortField sortField = SortField.Name,
        SortDirection sortDirection = SortDirection.Ascending);

    List<string> GetUniqueExtensions(ObservableCollection<FileItem> files);
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Models;

namespace Nexus.Services;

public interface IFileScannerService
{
    Task<ObservableCollection<FileItem>> ScanLocationsAsync(
        IEnumerable<ScanLocation> locations,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default);

    Task<ObservableCollection<FileItem>> ScanSingleFolderAsync(
        string folderPath,
        string locationName,
        CancellationToken cancellationToken = default);
}

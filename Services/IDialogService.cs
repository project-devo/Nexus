using System.Collections.Generic;
using Nexus.Models;

namespace Nexus.Services;

public interface IDialogService
{
    string? ShowFolderBrowser(string description, string? initialPath = null);
    bool ShowConfirmation(string title, string message);
    void ShowError(string title, string message);
    void ShowInfo(string title, string message);
    bool? ShowBatchRenameDialog(IEnumerable<FileItem> files);
}

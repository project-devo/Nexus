using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Nexus.Models;
using Nexus.ViewModels;
using Nexus.Views;

namespace Nexus.Services;

public class DialogService : IDialogService
{
    private readonly IFileOperationService _fileOperationService;

    public DialogService(IFileOperationService fileOperationService)
    {
        _fileOperationService = fileOperationService;
    }

    public string? ShowFolderBrowser(string description, string? initialPath = null)
    {
        using var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = description,
            SelectedPath = initialPath ?? string.Empty
        };

        var result = dialog.ShowDialog();
        return result == System.Windows.Forms.DialogResult.OK ? dialog.SelectedPath : null;
    }

    public bool ShowConfirmation(string title, string message)
    {
        var result = MessageBox.Show(
            message,
            title,
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        return result == MessageBoxResult.Yes;
    }

    public void ShowError(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public void ShowInfo(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public bool? ShowBatchRenameDialog(IEnumerable<FileItem> files)
    {
        var fileList = files.ToList();
        if (!fileList.Any()) return false;

        var viewModel = new BatchRenameViewModel(_fileOperationService);
        viewModel.LoadFiles(fileList);

        var window = new BatchRenameWindow(viewModel)
        {
            Owner = Application.Current?.MainWindow
        };

        return window.ShowDialog();
    }
}

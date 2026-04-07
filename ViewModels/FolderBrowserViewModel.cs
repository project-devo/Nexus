using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nexus.Models;
using Nexus.Services;

namespace Nexus.ViewModels;

public partial class FolderBrowserViewModel : ObservableObject
{
    private readonly FileOperationService _fileOperationService;

    [ObservableProperty]
    private string _currentPath = string.Empty;

    [ObservableProperty]
    private ObservableCollection<FileItem> _folders = new();

    [ObservableProperty]
    private FileItem? _selectedFolder;

    [ObservableProperty]
    private string _pathHistory = string.Empty;

    public FolderBrowserViewModel()
    {
        _fileOperationService = new FileOperationService();
        NavigateToPath(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
    }

    [RelayCommand]
    private void NavigateToPath(string path)
    {
        if (!Directory.Exists(path)) return;

        CurrentPath = path;
        Folders.Clear();

        try
        {
            var dirInfo = new DirectoryInfo(path);

            if (dirInfo.Parent != null)
            {
                Folders.Add(new FileItem
                {
                    Name = "..",
                    FullName = dirInfo.Parent.FullName,
                    IsDirectory = true,
                    DisplayName = "⬆ Parent Folder"
                });
            }

            var directories = dirInfo.GetDirectories();
            foreach (var dir in directories)
            {
                Folders.Add(FileItem.FromDirectoryInfo(dir, "Browser"));
            }
        }
        catch (UnauthorizedAccessException) { }
        catch (Exception) { }
    }

    [RelayCommand]
    private async Task SelectFolderAsync()
    {
        if (SelectedFolder == null) return;

        if (SelectedFolder.Name == "..")
        {
            NavigateToPath(SelectedFolder.FullName);
            return;
        }

        using var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Select destination for folder operation",
            SelectedPath = SelectedFolder.FullName
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            var result = MessageBox.Show(
                $"Copy '{SelectedFolder.Name}' to '{dialog.SelectedPath}'?",
                "Confirm Copy",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await _fileOperationService.CopyFilesAsync(new[] { SelectedFolder }, dialog.SelectedPath);
            }
        }
    }
}

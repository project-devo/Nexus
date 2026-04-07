using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nexus.Models;
using Nexus.Services;

namespace Nexus.ViewModels;

public partial class BatchRenameViewModel : ObservableObject
{
    private readonly FileOperationService _fileOperationService;

    [ObservableProperty]
    private ObservableCollection<FileItem> _selectedFiles = new();

    [ObservableProperty]
    private string _pattern = string.Empty;

    [ObservableProperty]
    private bool _includeExtension;

    [ObservableProperty]
    private string _previewText = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public BatchRenameViewModel()
    {
        _fileOperationService = new FileOperationService();
    }

    public void LoadFiles(IEnumerable<FileItem> files)
    {
        SelectedFiles.Clear();
        foreach (var file in files)
        {
            SelectedFiles.Add(file);
        }
        UpdatePreview();
    }

    partial void OnPatternChanged(string value) => UpdatePreview();

    private void UpdatePreview()
    {
        if (SelectedFiles.Count == 0)
        {
            PreviewText = "No files selected";
            return;
        }

        var preview = new System.Text.StringBuilder();
        preview.AppendLine("Preview:");
        
        int index = 1;
        foreach (var file in SelectedFiles.Take(10))
        {
            string extension = System.IO.Path.GetExtension(file.Name);
            string newName = Pattern.Replace("#", index.ToString("D3"));
            
            if (!IncludeExtension && !file.IsDirectory)
            {
                newName += extension;
            }
            
            preview.AppendLine($"  {file.Name} → {newName}");
            index++;
        }

        if (SelectedFiles.Count > 10)
        {
            preview.AppendLine($"  ... and {SelectedFiles.Count - 10} more");
        }

        PreviewText = preview.ToString();
    }

    [RelayCommand]
    private async Task ExecuteRenameAsync()
    {
        if (SelectedFiles.Count == 0 || string.IsNullOrEmpty(Pattern))
        {
            StatusMessage = "Please select files and enter a pattern";
            return;
        }

        IsBusy = true;
        StatusMessage = "Renaming files...";

        var success = await _fileOperationService.BatchRenameAsync(
            SelectedFiles, Pattern, IncludeExtension);

        StatusMessage = success ? "Rename completed successfully!" : "Rename completed with some errors";
        IsBusy = false;

        if (success)
        {
            CloseCommand?.Execute(null);
        }
    }

    [RelayCommand]
    private void Close()
    {
        // Close dialog
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nexus.Models;
using Nexus.Services;

namespace Nexus.ViewModels;

public record RenamePreviewItem(string OriginalName, string NewName);

public partial class BatchRenameViewModel : ObservableObject
{
    private readonly IFileOperationService _fileOperationService;

    [ObservableProperty]
    private ObservableCollection<FileItem> _selectedFiles = new();

    [ObservableProperty]
    private ObservableCollection<RenamePreviewItem> _previewItems = new();

    [ObservableProperty]
    private string _pattern = "File_###";

    [ObservableProperty]
    private bool _includeExtension;

    [ObservableProperty]
    private string _previewText = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public event Action<bool?>? RequestClose;

    public BatchRenameViewModel() : this(new FileOperationService())
    {
    }

    public BatchRenameViewModel(IFileOperationService fileOperationService)
    {
        _fileOperationService = fileOperationService;
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
    partial void OnIncludeExtensionChanged(bool value) => UpdatePreview();

    public void UpdatePreview()
    {
        PreviewItems.Clear();

        if (SelectedFiles.Count == 0)
        {
            PreviewText = "No files selected";
            return;
        }

        if (string.IsNullOrWhiteSpace(Pattern))
        {
            PreviewText = "Enter a naming pattern (e.g. Document_###)";
            return;
        }

        int hashCount = 0;
        int firstHash = Pattern.IndexOf('#');
        if (firstHash >= 0)
        {
            int endHash = firstHash;
            while (endHash < Pattern.Length && Pattern[endHash] == '#')
            {
                endHash++;
            }
            hashCount = endHash - firstHash;
        }

        string hashPlaceholder = hashCount > 0 ? new string('#', hashCount) : "#";
        string formatString = hashCount > 1 ? $"D{hashCount}" : "D";

        int index = 1;
        foreach (var file in SelectedFiles)
        {
            string extension = System.IO.Path.GetExtension(file.Name);
            string formattedIndex = index.ToString(formatString);
            string newBaseName = Pattern.Contains('#')
                ? Pattern.Replace(hashPlaceholder, formattedIndex)
                : $"{Pattern}_{formattedIndex}";

            string newName = newBaseName;
            if (!IncludeExtension && !file.IsDirectory)
            {
                newName += extension;
            }

            PreviewItems.Add(new RenamePreviewItem(file.Name, newName));
            index++;
        }

        PreviewText = $"Ready to rename {SelectedFiles.Count} items.";
    }

    [RelayCommand]
    private async Task ExecuteRenameAsync()
    {
        if (SelectedFiles.Count == 0 || string.IsNullOrWhiteSpace(Pattern))
        {
            StatusMessage = "Please select files and enter a pattern";
            return;
        }

        IsBusy = true;
        StatusMessage = "Renaming files...";

        var success = await _fileOperationService.BatchRenameAsync(
            SelectedFiles, Pattern, IncludeExtension);

        IsBusy = false;
        StatusMessage = success ? "Rename completed successfully!" : "Rename completed with some errors";

        if (success)
        {
            RequestClose?.Invoke(true);
        }
    }

    [RelayCommand]
    private void Close()
    {
        RequestClose?.Invoke(false);
    }
}

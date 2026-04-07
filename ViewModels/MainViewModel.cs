using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nexus.Models;
using Nexus.Services;

namespace Nexus.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly FileScannerService _scannerService;
    private readonly SearchService _searchService;
    private readonly FileOperationService _fileOperationService;

    [ObservableProperty]
    private bool _isDarkMode = true;

    [ObservableProperty]
    private string _themeIcon = "🌙";

    [ObservableProperty]
    private ObservableCollection<ScanLocation> _scanLocations = new();

    [ObservableProperty]
    private ObservableCollection<FileItem> _allFiles = new();

    [ObservableProperty]
    private ObservableCollection<FileItem> _displayedFiles = new();

    [ObservableProperty]
    private ObservableCollection<FileItem> _selectedFiles = new();

    [ObservableProperty]
    private string _searchTerm = string.Empty;

    [ObservableProperty]
    private string _selectedExtension = "All";

    [ObservableProperty]
    private ObservableCollection<string> _availableExtensions = new() { "All" };

    [ObservableProperty]
    private FileSizeFilter _selectedSizeFilter = FileSizeFilter.Any;

    [ObservableProperty]
    private DateFilter _selectedDateFilter = DateFilter.Any;

    [ObservableProperty]
    private SortField _sortField = SortField.Name;

    [ObservableProperty]
    private SortDirection _sortDirection = SortDirection.Ascending;

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private int _scanProgress;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private bool _searchContentEnabled;

    public MainViewModel()
    {
        _scannerService = new FileScannerService();
        _searchService = new SearchService();
        _fileOperationService = new FileOperationService();

        ScanLocations = new ObservableCollection<ScanLocation>(
            FileScannerService.GetDefaultSystemLocations());
    }

    [RelayCommand]
    private async Task ScanLocationsAsync()
    {
        if (IsScanning) return;

        IsScanning = true;
        ScanProgress = 0;
        StatusMessage = "Scanning locations...";
        AllFiles.Clear();
        DisplayedFiles.Clear();

        var progress = new Progress<int>(value => ScanProgress = value);
        var cts = new CancellationToken();

        try
        {
            var files = await _scannerService.ScanLocationsAsync(ScanLocations, progress, cts);
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                AllFiles = files;
                ApplyFilters();
                UpdateAvailableExtensions();
                StatusMessage = $"Found {AllFiles.Count} items";
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsScanning = false;
        }
    }

    [RelayCommand]
    private void AddCustomLocation(string path)
    {
        if (!string.IsNullOrEmpty(path) && System.IO.Directory.Exists(path))
        {
            var existing = ScanLocations.FirstOrDefault(l => l.Path == path);
            if (existing == null)
            {
                ScanLocations.Add(ScanLocation.CreateCustomLocation(path));
            }
        }
    }

    [RelayCommand]
    private void RemoveLocation(ScanLocation location)
    {
        if (location != null && !location.IsSystemLocation)
        {
            ScanLocations.Remove(location);
        }
    }

    [RelayCommand]
    private void ToggleLocation(ScanLocation location)
    {
        if (location != null)
        {
            location.IsEnabled = !location.IsEnabled;
        }
    }

    partial void OnSearchTermChanged(string value) => ApplyFilters();
    partial void OnSelectedExtensionChanged(string value) => ApplyFilters();
    partial void OnSelectedSizeFilterChanged(FileSizeFilter value) => ApplyFilters();
    partial void OnSelectedDateFilterChanged(DateFilter value) => ApplyFilters();
    partial void OnSortFieldChanged(SortField value) => ApplyFilters();
    partial void OnSortDirectionChanged(SortDirection value) => ApplyFilters();

    [RelayCommand]
    private void ToggleSortDirection()
    {
        SortDirection = SortDirection == SortDirection.Ascending 
            ? SortDirection.Descending 
            : SortDirection.Ascending;
    }

    private void ApplyFilters()
    {
        var filtered = _searchService.FilterAndSort(
            AllFiles,
            SearchTerm,
            SelectedExtension == "All" ? null : SelectedExtension,
            SelectedSizeFilter,
            SelectedDateFilter,
            SortField,
            SortDirection);

        Application.Current.Dispatcher.Invoke(() =>
        {
            DisplayedFiles = filtered;
            UpdateSelectedFiles();
        });
    }

    private void UpdateAvailableExtensions()
    {
        var extensions = _searchService.GetUniqueExtensions(AllFiles);
        Application.Current.Dispatcher.Invoke(() =>
        {
            AvailableExtensions.Clear();
            AvailableExtensions.Add("All");
            foreach (var ext in extensions)
            {
                AvailableExtensions.Add(ext);
            }
        });
    }

    private void UpdateSelectedFiles()
    {
        SelectedFiles.Clear();
        foreach (var file in DisplayedFiles.Where(f => f.IsSelected))
        {
            SelectedFiles.Add(file);
        }
    }

    [RelayCommand]
    private void SelectAllFiles()
    {
        foreach (var file in DisplayedFiles)
        {
            file.IsSelected = true;
        }
    }

    [RelayCommand]
    private void ClearSelection()
    {
        foreach (var file in DisplayedFiles)
        {
            file.IsSelected = false;
        }
    }

    [RelayCommand]
    private async Task CopyFilesAsync()
    {
        var files = DisplayedFiles.Where(f => f.IsSelected).ToList();
        if (!files.Any()) return;

        using var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Select destination folder"
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            StatusMessage = "Copying files...";
            var success = await _fileOperationService.CopyFilesAsync(files, dialog.SelectedPath);
            StatusMessage = success ? "Copy completed" : "Copy completed with errors";
        }
    }

    [RelayCommand]
    private async Task MoveFilesAsync()
    {
        var files = DisplayedFiles.Where(f => f.IsSelected).ToList();
        if (!files.Any()) return;

        using var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Select destination folder"
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            StatusMessage = "Moving files...";
            var success = await _fileOperationService.MoveFilesAsync(files, dialog.SelectedPath);
            StatusMessage = success ? "Move completed" : "Move completed with errors";
            await ScanLocationsAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteFilesAsync()
    {
        var files = DisplayedFiles.Where(f => f.IsSelected).ToList();
        if (!files.Any()) return;

        var result = MessageBox.Show(
            $"Delete {files.Count} selected item(s)?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            StatusMessage = "Deleting files...";
            var success = await _fileOperationService.DeleteFilesAsync(files);
            StatusMessage = success ? "Delete completed" : "Delete completed with errors";
            await ScanLocationsAsync();
        }
    }

    [RelayCommand]
    private void OpenFile(FileItem? file)
    {
        if (file != null)
        {
            _fileOperationService.OpenFile(file);
        }
    }

    [RelayCommand]
    private void OpenContainingFolder(FileItem? file)
    {
        if (file != null)
        {
            _fileOperationService.OpenContainingFolder(file);
        }
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        IsDarkMode = !IsDarkMode;
        ThemeIcon = IsDarkMode ? "🌙" : "☀️";
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        var app = Application.Current;
        
        if (IsDarkMode)
        {
            app.Resources["BackgroundColor"] = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#0D1117");
            app.Resources["PanelColor"] = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#161B22");
            app.Resources["CardColor"] = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#21262D");
            app.Resources["BorderColor"] = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#30363D");
            app.Resources["PrimaryTextColor"] = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F0F6FC");
            app.Resources["SecondaryTextColor"] = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#8B949E");
            app.Resources["AccentColor"] = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#58A6FF");
            
            app.Resources["BackgroundBrush"] = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)app.Resources["BackgroundColor"]);
            app.Resources["PanelBrush"] = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)app.Resources["PanelColor"]);
            app.Resources["CardBrush"] = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)app.Resources["CardColor"]);
            app.Resources["BorderBrush"] = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)app.Resources["BorderColor"]);
            app.Resources["PrimaryTextBrush"] = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)app.Resources["PrimaryTextColor"]);
            app.Resources["SecondaryTextBrush"] = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)app.Resources["SecondaryTextColor"]);
            app.Resources["AccentBrush"] = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)app.Resources["AccentColor"]);
        }
        else
        {
            app.Resources["BackgroundColor"] = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF");
            app.Resources["PanelColor"] = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F6F8FA");
            app.Resources["CardColor"] = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF");
            app.Resources["BorderColor"] = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#D0D7DE");
            app.Resources["PrimaryTextColor"] = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#1F2328");
            app.Resources["SecondaryTextColor"] = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#656D76");
            app.Resources["AccentColor"] = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#0969DA");
            
            app.Resources["BackgroundBrush"] = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)app.Resources["BackgroundColor"]);
            app.Resources["PanelBrush"] = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)app.Resources["PanelColor"]);
            app.Resources["CardBrush"] = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)app.Resources["CardColor"]);
            app.Resources["BorderBrush"] = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)app.Resources["BorderColor"]);
            app.Resources["PrimaryTextBrush"] = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)app.Resources["PrimaryTextColor"]);
            app.Resources["SecondaryTextBrush"] = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)app.Resources["SecondaryTextColor"]);
            app.Resources["AccentBrush"] = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)app.Resources["AccentColor"]);
        }
    }
}

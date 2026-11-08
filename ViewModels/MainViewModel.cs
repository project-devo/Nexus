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
    private readonly IFileScannerService _scannerService;
    private readonly ISearchService _searchService;
    private readonly IFileOperationService _fileOperationService;
    private readonly IDialogService _dialogService;

    private CancellationTokenSource? _scanCts;
    private CancellationTokenSource? _searchDebounceCts;

    [ObservableProperty]
    private bool _isDarkMode = true;

    [ObservableProperty]
    private string _themeIcon = "\uE708";

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

    public MainViewModel() : this(
        new FileScannerService(),
        new SearchService(),
        new FileOperationService(),
        new DialogService(new FileOperationService()))
    {
    }

    public MainViewModel(
        IFileScannerService scannerService,
        ISearchService searchService,
        IFileOperationService fileOperationService,
        IDialogService dialogService)
    {
        _scannerService = scannerService;
        _searchService = searchService;
        _fileOperationService = fileOperationService;
        _dialogService = dialogService;

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
        SelectedFiles.Clear();

        var progress = new Progress<int>(value => ScanProgress = value);
        _scanCts?.Dispose();
        _scanCts = new CancellationTokenSource();

        try
        {
            var files = await _scannerService.ScanLocationsAsync(ScanLocations, progress, _scanCts.Token);

            if (_scanCts.IsCancellationRequested)
            {
                StatusMessage = "Scan cancelled";
                return;
            }

            void UpdateUi()
            {
                AllFiles = files;
                ApplyFilters();
                UpdateAvailableExtensions();
                StatusMessage = $"Found {AllFiles.Count} items";
            }

            if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(UpdateUi);
            }
            else
            {
                UpdateUi();
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Scan cancelled";
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
    private void CancelScan()
    {
        if (IsScanning && _scanCts != null)
        {
            _scanCts.Cancel();
            StatusMessage = "Cancelling scan...";
        }
    }

    [RelayCommand]
    private void PickAndAddCustomFolder()
    {
        var path = _dialogService.ShowFolderBrowser("Select folder to add to Locations");
        if (!string.IsNullOrEmpty(path))
        {
            AddCustomLocation(path);
        }
    }

    [RelayCommand]
    private void AddCustomLocation(string path)
    {
        if (!string.IsNullOrEmpty(path) && System.IO.Directory.Exists(path))
        {
            var existing = ScanLocations.FirstOrDefault(l => string.Equals(l.Path, path, StringComparison.OrdinalIgnoreCase));
            if (existing == null)
            {
                ScanLocations.Add(ScanLocation.CreateCustomLocation(path));
                StatusMessage = $"Added folder: {System.IO.Path.GetFileName(path)}";
            }
            else
            {
                existing.IsEnabled = true;
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

    partial void OnSearchTermChanged(string value) => DebounceApplyFilters();
    partial void OnSelectedExtensionChanged(string value) => ApplyFilters();
    partial void OnSelectedSizeFilterChanged(FileSizeFilter value) => ApplyFilters();
    partial void OnSelectedDateFilterChanged(DateFilter value) => ApplyFilters();
    partial void OnSortFieldChanged(SortField value) => ApplyFilters();
    partial void OnSortDirectionChanged(SortDirection value) => ApplyFilters();

    private void DebounceApplyFilters()
    {
        _searchDebounceCts?.Cancel();
        _searchDebounceCts?.Dispose();
        _searchDebounceCts = new CancellationTokenSource();
        var token = _searchDebounceCts.Token;

        Task.Delay(200, token).ContinueWith(t =>
        {
            if (!t.IsCanceled)
            {
                void Run() => ApplyFilters();
                if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
                {
                    Application.Current.Dispatcher.Invoke(Run);
                }
                else
                {
                    Run();
                }
            }
        }, TaskScheduler.Default);
    }

    [RelayCommand]
    private void ToggleSortDirection()
    {
        SortDirection = SortDirection == SortDirection.Ascending
            ? SortDirection.Descending
            : SortDirection.Ascending;
    }

    public void ApplyFilters()
    {
        var filtered = _searchService.FilterAndSort(
            AllFiles,
            SearchTerm,
            SelectedExtension == "All" ? null : SelectedExtension,
            SelectedSizeFilter,
            SelectedDateFilter,
            SortField,
            SortDirection);

        void Update()
        {
            DisplayedFiles = filtered;
            UpdateSelectedFiles();
        }

        if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(Update);
        }
        else
        {
            Update();
        }
    }

    private void UpdateAvailableExtensions()
    {
        var extensions = _searchService.GetUniqueExtensions(AllFiles);
        void Update()
        {
            // Clear() makes the bound ComboBox push null into SelectedExtension; restore it afterwards.
            var current = SelectedExtension;
            AvailableExtensions.Clear();
            AvailableExtensions.Add("All");
            foreach (var ext in extensions)
            {
                AvailableExtensions.Add(ext);
            }
            SelectedExtension = AvailableExtensions.Contains(current) ? current : "All";
        }

        if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(Update);
        }
        else
        {
            Update();
        }
    }

    public void UpdateSelectedFiles()
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
        UpdateSelectedFiles();
    }

    [RelayCommand]
    private void ClearSelection()
    {
        foreach (var file in DisplayedFiles)
        {
            file.IsSelected = false;
        }
        SelectedFiles.Clear();
    }

    [RelayCommand]
    private async Task OpenBatchRenameAsync()
    {
        var selected = DisplayedFiles.Where(f => f.IsSelected).ToList();
        if (!selected.Any())
        {
            _dialogService.ShowInfo("Batch Rename", "Please select at least one file to rename.");
            return;
        }

        var result = _dialogService.ShowBatchRenameDialog(selected);
        if (result == true)
        {
            StatusMessage = "Batch rename completed.";
            await ScanLocationsAsync();
        }
    }

    [RelayCommand]
    private async Task CopyFilesAsync()
    {
        var files = DisplayedFiles.Where(f => f.IsSelected).ToList();
        if (!files.Any()) return;

        var destination = _dialogService.ShowFolderBrowser("Select destination folder for copy");
        if (!string.IsNullOrEmpty(destination))
        {
            StatusMessage = "Copying files...";
            var success = await _fileOperationService.CopyFilesAsync(files, destination);
            StatusMessage = success ? "Copy completed successfully." : "Copy completed with errors.";
        }
    }

    [RelayCommand]
    private async Task MoveFilesAsync()
    {
        var files = DisplayedFiles.Where(f => f.IsSelected).ToList();
        if (!files.Any()) return;

        var destination = _dialogService.ShowFolderBrowser("Select destination folder for move");
        if (!string.IsNullOrEmpty(destination))
        {
            StatusMessage = "Moving files...";
            var success = await _fileOperationService.MoveFilesAsync(files, destination);
            StatusMessage = success ? "Move completed successfully." : "Move completed with errors.";
            await ScanLocationsAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteFilesAsync()
    {
        var files = DisplayedFiles.Where(f => f.IsSelected).ToList();
        if (!files.Any()) return;

        var confirmed = _dialogService.ShowConfirmation(
            "Confirm Delete",
            $"Send {files.Count} selected item(s) to the Recycle Bin?");

        if (confirmed)
        {
            StatusMessage = "Deleting files...";
            var success = await _fileOperationService.DeleteFilesAsync(files, useRecycleBin: true);
            StatusMessage = success ? "Delete completed successfully." : "Delete completed with errors.";
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
        ThemeIcon = IsDarkMode ? "\uE708" : "\uE706";
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        var app = Application.Current;
        if (app == null) return;

        if (IsDarkMode)
        {
            SetColor(app, "BackgroundColor", "#202020");
            SetColor(app, "PanelColor", "#2B2B2B");
            SetColor(app, "CardColor", "#323232");
            SetColor(app, "CardHoverColor", "#3F3F3F");
            SetColor(app, "SurfaceSubtleColor", "#3A3A3A");
            SetColor(app, "BorderColor", "#4A4A4A");
            SetColor(app, "PrimaryTextColor", "#FFFFFF");
            SetColor(app, "SecondaryTextColor", "#B0B0B0");
            SetColor(app, "AccentColor", "#60CDFF");
            SetColor(app, "AccentHoverColor", "#4CC2FF");
            SetColor(app, "AccentPressedColor", "#39B5F5");
            SetColor(app, "AccentTextColor", "#000000");
            SetColor(app, "SelectionColor", "#264F78");
            SetColor(app, "DangerColor", "#E81123");
            SetColor(app, "DangerHoverColor", "#C50F1F");
        }
        else
        {
            SetColor(app, "BackgroundColor", "#F3F3F3");
            SetColor(app, "PanelColor", "#FFFFFF");
            SetColor(app, "CardColor", "#FBFBFB");
            SetColor(app, "CardHoverColor", "#EDEDED");
            SetColor(app, "SurfaceSubtleColor", "#EFEFEF");
            SetColor(app, "BorderColor", "#D4D4D4");
            SetColor(app, "PrimaryTextColor", "#1A1A1A");
            SetColor(app, "SecondaryTextColor", "#555555");
            SetColor(app, "AccentColor", "#0067C0");
            SetColor(app, "AccentHoverColor", "#1879CD");
            SetColor(app, "AccentPressedColor", "#005A9E");
            SetColor(app, "AccentTextColor", "#FFFFFF");
            SetColor(app, "SelectionColor", "#CCE4F7");
            SetColor(app, "DangerColor", "#C42B1C");
            SetColor(app, "DangerHoverColor", "#A4262C");
        }

        App.IsDarkTheme = IsDarkMode;
        foreach (Window window in app.Windows)
            App.ApplyTitleBarTheme(window);
    }

    private static void SetColor(Application app, string colorKey, string hex)
    {
        var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex);
        app.Resources[colorKey] = color;
        string brushKey = colorKey.EndsWith("Color", StringComparison.Ordinal)
            ? colorKey.Substring(0, colorKey.Length - 5) + "Brush"
            : colorKey + "Brush";

        var brush = new System.Windows.Media.SolidColorBrush(color);
        brush.Freeze();
        app.Resources[brushKey] = brush;
    }
}

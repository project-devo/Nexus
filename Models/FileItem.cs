using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace Nexus.Models;

public class FileItem : INotifyPropertyChanged
{
    private bool _isSelected;
    private string _displayName = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public long Size { get; set; }
    public string FormattedSize => FormatSize(Size);
    public DateTime DateModified { get; set; }
    public DateTime DateCreated { get; set; }
    public bool IsDirectory { get; set; }
    public string ParentPath { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;

    public string DisplayName
    {
        get => _displayName;
        set
        {
            _displayName = value;
            OnPropertyChanged();
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            OnPropertyChanged();
        }
    }

    public string IconKey => IsDirectory ? "Folder" : GetFileIcon(Extension);

    public static FileItem FromFileInfo(FileInfo fileInfo, string locationName)
    {
        return new FileItem
        {
            Name = fileInfo.Name,
            FullName = fileInfo.FullName,
            Extension = fileInfo.Extension.ToLowerInvariant(),
            Size = fileInfo.Length,
            DateModified = fileInfo.LastWriteTime,
            DateCreated = fileInfo.CreationTime,
            IsDirectory = false,
            ParentPath = fileInfo.Directory?.FullName ?? string.Empty,
            LocationName = locationName,
            DisplayName = fileInfo.Name
        };
    }

    public static FileItem FromDirectoryInfo(DirectoryInfo directoryInfo, string locationName)
    {
        return new FileItem
        {
            Name = directoryInfo.Name,
            FullName = directoryInfo.FullName,
            Extension = string.Empty,
            Size = 0,
            DateModified = directoryInfo.LastWriteTime,
            DateCreated = directoryInfo.CreationTime,
            IsDirectory = true,
            ParentPath = directoryInfo.Parent?.FullName ?? string.Empty,
            LocationName = locationName,
            DisplayName = directoryInfo.Name
        };
    }

    private static string GetFileIcon(string extension)
    {
        return extension switch
        {
            ".txt" or ".log" or ".md" or ".csv" => "TextFile",
            ".doc" or ".docx" => "WordFile",
            ".xls" or ".xlsx" => "ExcelFile",
            ".pdf" => "PdfFile",
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".svg" => "ImageFile",
            ".mp3" or ".wav" or ".flac" or ".aac" or ".ogg" => "AudioFile",
            ".mp4" or ".avi" or ".mkv" or ".mov" or ".wmv" => "VideoFile",
            ".zip" or ".rar" or ".7z" or ".tar" or ".gz" => "ArchiveFile",
            ".exe" or ".msi" or ".bat" or ".cmd" => "ExecutableFile",
            ".cs" or ".py" or ".js" or ".ts" or ".html" or ".css" or ".json" or ".xml" => "CodeFile",
            _ => "GenericFile"
        };
    }

    private static string FormatSize(long bytes)
    {
        if (bytes == 0) return "0 B";
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return $"{size:0.##} {sizes[order]}";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

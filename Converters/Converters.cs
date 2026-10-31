using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Nexus.Converters;

public class BoolToVisibilityConverter : MarkupExtension, IValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }
        return false;
    }
}

public class InverseBoolConverter : MarkupExtension, IValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return false;
    }
}

public class FileSizeToTextConverter : MarkupExtension, IValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is long size)
        {
            if (size == 0) return "0 B";
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double s = size;
            while (s >= 1024 && order < sizes.Length - 1)
            {
                order++;
                s /= 1024;
            }
            return $"{s:0.##} {sizes[order]}";
        }
        return "0 B";
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToInverseVisibilityConverter : MarkupExtension, IValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class FileIconGlyphConverter : MarkupExtension, IValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is string iconKey)
        {
            return iconKey switch
            {
                "Folder" => "\uE8B7",
                "ImageFile" => "\uEB9F",
                "VideoFile" => "\uE8B2",
                "AudioFile" => "\uE8D6",
                "CodeFile" => "\uE943",
                "ArchiveFile" => "\uF012",
                "ExecutableFile" => "\uE756",
                "PdfFile" or "WordFile" or "ExcelFile" or "TextFile" => "\uE8A5",
                _ => "\uE7C3"
            };
        }
        return "\uE7C3";
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class FileIconBrushConverter : MarkupExtension, IValueConverter
{
    private static readonly System.Windows.Media.SolidColorBrush FolderBrush = new((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F59E0B"));
    private static readonly System.Windows.Media.SolidColorBrush ImageBrush = new((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#8B5CF6"));
    private static readonly System.Windows.Media.SolidColorBrush VideoBrush = new((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#EC4899"));
    private static readonly System.Windows.Media.SolidColorBrush AudioBrush = new((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#10B981"));
    private static readonly System.Windows.Media.SolidColorBrush CodeBrush = new((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#06B6D4"));
    private static readonly System.Windows.Media.SolidColorBrush ArchiveBrush = new((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#EAB308"));
    private static readonly System.Windows.Media.SolidColorBrush ExeBrush = new((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#EF4444"));
    private static readonly System.Windows.Media.SolidColorBrush DocBrush = new((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#3B82F6"));
    private static readonly System.Windows.Media.SolidColorBrush DefaultBrush = new((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#94A3B8"));

    static FileIconBrushConverter()
    {
        FolderBrush.Freeze();
        ImageBrush.Freeze();
        VideoBrush.Freeze();
        AudioBrush.Freeze();
        CodeBrush.Freeze();
        ArchiveBrush.Freeze();
        ExeBrush.Freeze();
        DocBrush.Freeze();
        DefaultBrush.Freeze();
    }

    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is string iconKey)
        {
            return iconKey switch
            {
                "Folder" => FolderBrush,
                "ImageFile" => ImageBrush,
                "VideoFile" => VideoBrush,
                "AudioFile" => AudioBrush,
                "CodeFile" => CodeBrush,
                "ArchiveFile" => ArchiveBrush,
                "ExecutableFile" => ExeBrush,
                "PdfFile" or "WordFile" or "ExcelFile" or "TextFile" => DocBrush,
                _ => DefaultBrush
            };
        }
        return DefaultBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ZeroCountToVisibilityConverter : MarkupExtension, IValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is int count)
        {
            return count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class SortDirectionToGlyphConverter : MarkupExtension, IValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is Nexus.Models.SortDirection dir)
        {
            return dir == Nexus.Models.SortDirection.Ascending ? "\uE74A" : "\uE74B";
        }
        return "\uE74A";
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

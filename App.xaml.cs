using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Services;
using Nexus.ViewModels;

namespace Nexus;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public App()
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            LogException("AppDomain.UnhandledException", e.ExceptionObject as Exception);
        };

        DispatcherUnhandledException += (s, e) =>
        {
            LogException("DispatcherUnhandledException", e.Exception);
            e.Handled = false;
        };

        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        // Match the OS title bar to the app theme for every window as it loads.
        EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent,
            new RoutedEventHandler((s, _) => ApplyTitleBarTheme((Window)s)));
    }

    public static bool IsDarkTheme { get; set; } = true;

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    public static void ApplyTitleBarTheme(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero) return;
        int dark = IsDarkTheme ? 1 : 0;
        // 20 = DWMWA_USE_IMMERSIVE_DARK_MODE (Win10 20H1+ / Win11); silently ignored on older Windows.
        DwmSetWindowAttribute(hwnd, 20, ref dark, sizeof(int));
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IFileScannerService, FileScannerService>();
        services.AddSingleton<ISearchService, SearchService>();
        services.AddSingleton<IFileOperationService, FileOperationService>();
        services.AddSingleton<IDialogService, DialogService>();

        services.AddTransient<MainViewModel>();
        services.AddTransient<BatchRenameViewModel>();
    }

    private static void LogException(string source, Exception? ex)
    {
        try
        {
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log");
            string message = $"[{DateTime.Now}] {source}:\n{ex?.ToString()}\n-------------------------\n";
            File.AppendAllText(logPath, message);
            MessageBox.Show(ex?.Message ?? "Unknown error", $"Nexus Error ({source})", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch { }
    }
}

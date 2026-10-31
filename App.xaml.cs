using System;
using System.IO;
using System.Windows;
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

using System;
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
}

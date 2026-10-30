using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Models;
using Nexus.ViewModels;

namespace Nexus.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        if (App.Services != null)
        {
            DataContext = App.Services.GetRequiredService<MainViewModel>();
        }
    }

    private void OnRowDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is DataGridRow row && row.Item is FileItem fileItem && DataContext is MainViewModel vm)
        {
            vm.OpenFileCommand.Execute(fileItem);
        }
    }
}

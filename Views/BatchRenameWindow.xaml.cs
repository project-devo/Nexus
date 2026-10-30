using System.Windows;
using Nexus.ViewModels;

namespace Nexus.Views;

public partial class BatchRenameWindow : Window
{
    public BatchRenameWindow()
    {
        InitializeComponent();
    }

    public BatchRenameWindow(BatchRenameViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.RequestClose += (result) =>
        {
            DialogResult = result;
            Close();
        };
    }
}

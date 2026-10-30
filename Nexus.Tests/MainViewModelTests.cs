using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Nexus.Models;
using Nexus.Services;
using Nexus.ViewModels;
using Xunit;

namespace Nexus.Tests;

public class MainViewModelTests
{
    private readonly Mock<IFileScannerService> _mockScanner = new();
    private readonly Mock<ISearchService> _mockSearch = new();
    private readonly Mock<IFileOperationService> _mockFileOp = new();
    private readonly Mock<IDialogService> _mockDialog = new();

    private MainViewModel CreateViewModel()
    {
        return new MainViewModel(
            _mockScanner.Object,
            _mockSearch.Object,
            _mockFileOp.Object,
            _mockDialog.Object);
    }

    [Fact]
    public void SelectAllFiles_SetsAllFilesSelectedAndUpdatesSelectedCollection()
    {
        // Arrange
        var vm = CreateViewModel();
        var files = new ObservableCollection<FileItem>
        {
            new() { Name = "file1.txt", IsSelected = false },
            new() { Name = "file2.txt", IsSelected = false },
        };
        vm.DisplayedFiles = files;

        // Act
        vm.SelectAllFilesCommand.Execute(null);

        // Assert
        vm.DisplayedFiles.Should().OnlyContain(f => f.IsSelected);
        vm.SelectedFiles.Should().HaveCount(2);
    }

    [Fact]
    public void ClearSelection_DeselectsAllFiles()
    {
        // Arrange
        var vm = CreateViewModel();
        var files = new ObservableCollection<FileItem>
        {
            new() { Name = "file1.txt", IsSelected = true },
            new() { Name = "file2.txt", IsSelected = true },
        };
        vm.DisplayedFiles = files;
        vm.SelectedFiles = new ObservableCollection<FileItem>(files);

        // Act
        vm.ClearSelectionCommand.Execute(null);

        // Assert
        vm.DisplayedFiles.Should().OnlyContain(f => !f.IsSelected);
        vm.SelectedFiles.Should().BeEmpty();
    }

    [Fact]
    public void PickAndAddCustomFolder_WhenFolderSelected_AddsToScanLocations()
    {
        // Arrange
        var vm = CreateViewModel();
        var tempDir = Path.GetTempPath();
        _mockDialog.Setup(d => d.ShowFolderBrowser(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns(tempDir);

        // Act
        vm.PickAndAddCustomFolderCommand.Execute(null);

        // Assert
        vm.ScanLocations.Should().Contain(l => l.Path == tempDir && !l.IsSystemLocation);
    }

    [Fact]
    public async Task DeleteFilesAsync_WhenUserConfirms_CallsDeleteServiceAndRefreshes()
    {
        // Arrange
        var vm = CreateViewModel();
        var file = new FileItem { Name = "test.txt", IsSelected = true };
        vm.DisplayedFiles = new ObservableCollection<FileItem> { file };

        _mockDialog.Setup(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns(true);
        _mockFileOp.Setup(f => f.DeleteFilesAsync(It.IsAny<System.Collections.Generic.IEnumerable<FileItem>>(), true))
                   .ReturnsAsync(true);
        _mockScanner.Setup(s => s.ScanLocationsAsync(It.IsAny<System.Collections.Generic.IEnumerable<ScanLocation>>(), It.IsAny<System.IProgress<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new ObservableCollection<FileItem>());

        // Act
        await vm.DeleteFilesCommand.ExecuteAsync(null);

        // Assert
        _mockFileOp.Verify(f => f.DeleteFilesAsync(It.Is<System.Collections.Generic.IEnumerable<FileItem>>(list => list.Contains(file)), true), Times.Once);
    }

    [Fact]
    public async Task DeleteFilesAsync_WhenUserCancels_DoesNotCallDeleteService()
    {
        // Arrange
        var vm = CreateViewModel();
        var file = new FileItem { Name = "test.txt", IsSelected = true };
        vm.DisplayedFiles = new ObservableCollection<FileItem> { file };

        _mockDialog.Setup(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns(false);

        // Act
        await vm.DeleteFilesCommand.ExecuteAsync(null);

        // Assert
        _mockFileOp.Verify(f => f.DeleteFilesAsync(It.IsAny<System.Collections.Generic.IEnumerable<FileItem>>(), It.IsAny<bool>()), Times.Never);
    }
}

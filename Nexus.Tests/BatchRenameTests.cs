using System.Collections.Generic;
using FluentAssertions;
using Nexus.Models;
using Nexus.ViewModels;
using Xunit;

namespace Nexus.Tests;

public class BatchRenameTests
{
    [Fact]
    public void UpdatePreview_WithZeroPaddedPattern_GeneratesFormattedNames()
    {
        // Arrange
        var vm = new BatchRenameViewModel();
        var files = new List<FileItem>
        {
            new() { Name = "vacation1.jpg", Extension = ".jpg" },
            new() { Name = "vacation2.jpg", Extension = ".jpg" },
            new() { Name = "notes.txt", Extension = ".txt" },
        };

        // Act
        vm.Pattern = "Holiday_###";
        vm.IncludeExtension = false;
        vm.LoadFiles(files);

        // Assert
        vm.PreviewItems.Should().HaveCount(3);
        vm.PreviewItems[0].NewName.Should().Be("Holiday_001.jpg");
        vm.PreviewItems[1].NewName.Should().Be("Holiday_002.jpg");
        vm.PreviewItems[2].NewName.Should().Be("Holiday_003.txt");
    }

    [Fact]
    public void UpdatePreview_WithSingleHash_GeneratesSingleDigitIndex()
    {
        // Arrange
        var vm = new BatchRenameViewModel();
        var files = new List<FileItem>
        {
            new() { Name = "docA.pdf", Extension = ".pdf" },
            new() { Name = "docB.pdf", Extension = ".pdf" }
        };

        // Act
        vm.Pattern = "Doc_#";
        vm.IncludeExtension = false;
        vm.LoadFiles(files);

        // Assert
        vm.PreviewItems[0].NewName.Should().Be("Doc_1.pdf");
        vm.PreviewItems[1].NewName.Should().Be("Doc_2.pdf");
    }

    [Fact]
    public void UpdatePreview_WhenIncludeExtensionIsTrue_DoesNotAppendOriginalExtension()
    {
        // Arrange
        var vm = new BatchRenameViewModel();
        var files = new List<FileItem>
        {
            new() { Name = "data.csv", Extension = ".csv" }
        };

        // Act
        vm.Pattern = "Export_###.backup";
        vm.IncludeExtension = true;
        vm.LoadFiles(files);

        // Assert
        vm.PreviewItems[0].NewName.Should().Be("Export_001.backup");
    }
}

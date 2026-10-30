using System;
using System.Collections.ObjectModel;
using FluentAssertions;
using Nexus.Models;
using Nexus.Services;
using Xunit;

namespace Nexus.Tests;

public class SearchServiceTests
{
    private readonly SearchService _sut = new();

    private ObservableCollection<FileItem> CreateSampleFiles()
    {
        return new ObservableCollection<FileItem>
        {
            new() { Name = "report.txt", FullName = @"C:\docs\report.txt", Extension = ".txt", Size = 500, DateModified = DateTime.Today },
            new() { Name = "photo.png", FullName = @"C:\pics\photo.png", Extension = ".png", Size = 50_000, DateModified = DateTime.Today.AddDays(-2) },
            new() { Name = "video.mp4", FullName = @"C:\vids\video.mp4", Extension = ".mp4", Size = 150_000_000, DateModified = DateTime.Today.AddMonths(-2) },
            new() { Name = "archive.zip", FullName = @"C:\archives\archive.zip", Extension = ".zip", Size = 5_000_000, DateModified = DateTime.Today.AddYears(-2) },
            new() { Name = "data.TXT", FullName = @"C:\docs\data.TXT", Extension = ".txt", Size = 1200, DateModified = DateTime.Today.AddDays(-10) },
        };
    }

    [Fact]
    public void FilterAndSort_ExtensionFilter_MatchesCaseInsensitivelyWithoutLeadingDot()
    {
        // Arrange
        var files = CreateSampleFiles();

        // Act - user selects "TXT" from extension dropdown
        var result = _sut.FilterAndSort(files, extensionFilter: "TXT");

        // Assert - Both report.txt and data.TXT must be returned (resolves the leading dot bug)
        result.Should().HaveCount(2);
        result.Should().OnlyContain(f => f.Extension.Equals(".txt", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void FilterAndSort_ExtensionFilter_All_ReturnsAllFiles()
    {
        // Arrange
        var files = CreateSampleFiles();

        // Act
        var result = _sut.FilterAndSort(files, extensionFilter: "All");

        // Assert
        result.Should().HaveCount(5);
    }

    [Theory]
    [InlineData(FileSizeFilter.Tiny, 1)]      // 500 B
    [InlineData(FileSizeFilter.Small, 2)]     // 1200 B, 50,000 B
    [InlineData(FileSizeFilter.Large, 1)]     // 5,000,000 B
    [InlineData(FileSizeFilter.Huge, 1)]      // 150,000,000 B
    public void FilterAndSort_SizeFilter_FiltersCorrectly(FileSizeFilter filter, int expectedCount)
    {
        // Arrange
        var files = CreateSampleFiles();

        // Act
        var result = _sut.FilterAndSort(files, sizeFilter: filter);

        // Assert
        result.Should().HaveCount(expectedCount);
    }

    [Fact]
    public void FilterAndSort_DateFilter_Today_ReturnsOnlyTodayFiles()
    {
        // Arrange
        var files = CreateSampleFiles();

        // Act
        var result = _sut.FilterAndSort(files, dateFilter: DateFilter.Today);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("report.txt");
    }

    [Fact]
    public void FilterAndSort_SearchTerm_FiltersByNameCaseInsensitive()
    {
        // Arrange
        var files = CreateSampleFiles();

        // Act
        var result = _sut.FilterAndSort(files, searchTerm: "port");

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("report.txt");
    }

    [Fact]
    public void FilterAndSort_SortBySize_AscendingAndDescending()
    {
        // Arrange
        var files = CreateSampleFiles();

        // Act
        var asc = _sut.FilterAndSort(files, sortField: SortField.Size, sortDirection: SortDirection.Ascending);
        var desc = _sut.FilterAndSort(files, sortField: SortField.Size, sortDirection: SortDirection.Descending);

        // Assert
        asc[0].Size.Should().Be(500);
        asc[^1].Size.Should().Be(150_000_000);

        desc[0].Size.Should().Be(150_000_000);
        desc[^1].Size.Should().Be(500);
    }

    [Fact]
    public void GetUniqueExtensions_ReturnsUppercaseDistinctListWithoutDots()
    {
        // Arrange
        var files = CreateSampleFiles();

        // Act
        var extensions = _sut.GetUniqueExtensions(files);

        // Assert
        extensions.Should().BeEquivalentTo(new[] { "MP4", "PNG", "TXT", "ZIP" });
    }
}

# Nexus - Modern Windows File Manager & Automation Tool

[![Download Latest Release](https://img.shields.io/github/v/release/project-devo/Nexus?label=Download%20Nexus%20v1.1&style=for-the-badge&logo=windows&color=0078D6)](https://github.com/project-devo/Nexus/releases/latest)
[![Tests Passing](https://img.shields.io/badge/tests-18%20passed-brightgreen?style=for-the-badge&logo=dotnet)](https://github.com/project-devo/Nexus)

A high-performance, asynchronous Windows desktop file manager built with **.NET 8**, **WPF**, and **CommunityToolkit.Mvvm**. Designed with clean MVVM architecture, full dependency injection, and high-throughput UI virtualization.

---

## 📥 Quick Download (No Install Required)
1. Download **[`Nexus-v1.1.0-Windows-x64.zip`](https://github.com/project-devo/Nexus/releases/latest)**.
2. Extract the ZIP anywhere on your PC.
3. Double-click **`Nexus.exe`** to run.
*(Self-contained: No .NET runtime or SDK installation needed).*

---

## Key Highlights & Features

- **⚡ High-Performance Location Scanner**: Asynchronously scans system and custom folders with non-blocking concurrency and instantaneous cancellation (`CancellationTokenSource`).
- **🚀 UI Virtualization**: Smooth 60 FPS scrolling over 50,000+ files using `VirtualizingStackPanel` with container recycling.
- **🔍 Advanced Search & Instant Filtering**: 
  - Search by file name with 200ms input debouncing.
  - Multi-criteria filtering by extension, file size categories (Tiny, Small, Medium, Large, Huge), and date modified ranges.
  - Multi-column ascending/descending sorting.
- **🏷️ Batch Rename Engine**: Dedicated dialog supporting sequential number padding (`#`, `##`, `###`), extension toggling, and live before-and-after preview.
- **🖱️ Complete Desktop Interactivity**:
  - Double-click to open files in default applications.
  - Full right-click context menu (Open, Open Containing Folder, Copy, Move, Batch Rename, Recycle Bin Delete).
  - Add custom directories to scan locations dynamically.
- **🎨 Windows 11 Fluent Design System & Dynamic Theming**:
  - Native Windows 11 WinUI 3 styling with Segoe Fluent Icons / MDL2 vector glyphs.
  - Signature Blue theme tokens with seamless real-time switching between Fluent Dark (`#202020` Mica / Smoke) and Fluent Light (`#F3F3F3` Cloud).
  - Unified Windows 11 Command Bar, visual file type badges, and illustrated empty states.
- **🧪 100% Testable Architecture**:
  - Decoupled OS dialogs behind `IDialogService`.
  - Service abstractions (`IFileScannerService`, `ISearchService`, `IFileOperationService`).
  - Automated unit test suite using **xUnit**, **Moq**, and **FluentAssertions**.

---

## Architecture

```
Nexus/
├── Nexus.sln                 # Complete Solution file
├── FileManager.csproj        # Main WPF Application Project (.NET 8)
├── App.xaml / App.xaml.cs    # Application Entry & DI Container Configuration
├── Models/                   # Data Models & Enums
│   ├── FileItem.cs           # File model with property change notification
│   ├── ScanLocation.cs       # System and custom scan locations
│   └── SearchFilters.cs      # Filter criteria enums
├── Services/                 # Business Logic & Decoupled Abstractions
│   ├── IDialogService.cs     # Abstraction for folder pickers, messages, dialogs
│   ├── DialogService.cs      # WPF / WinForms dialog implementation
│   ├── IFileScannerService.cs# Asynchronous recursive folder scanner
│   ├── FileScannerService.cs # Implementation with exclusion rules & progress
│   ├── IFileOperationService.cs # Copy, Move, Recycle Bin, Batch Rename
│   ├── FileOperationService.cs  # Safe file system implementation
│   ├── ISearchService.cs     # In-memory search & sort contract
│   └── SearchService.cs      # LINQ-based filtering and sorting engine
├── ViewModels/               # MVVM ViewModels
│   ├── MainViewModel.cs      # Core state, command bindings, debouncing
│   └── BatchRenameViewModel.cs # Batch renaming state & live preview calculation
├── Views/                    # Modern XAML Views
│   ├── MainWindow.xaml       # Main virtualized file manager UI
│   └── BatchRenameWindow.xaml# Modal batch renaming dialog with preview
├── Converters/               # Value converters for XAML bindings
├── Nexus.Tests/              # Automated Test Project (.NET 8)
│   ├── Nexus.Tests.csproj    # xUnit + Moq + FluentAssertions
│   ├── SearchServiceTests.cs # Filtering, extension handling, and sort tests
│   ├── BatchRenameTests.cs   # Naming patterns and preview formatting tests
│   └── MainViewModelTests.cs # ViewModel state and mocked service tests
└── build.ps1                 # Automated restore, build, and test script
```

---

## Build & Run

### Prerequisites
- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Quick Build & Test
Run the automated PowerShell build script:
```powershell
.\build.ps1
```

Or build manually via .NET CLI:
```powershell
dotnet restore Nexus.sln
dotnet build Nexus.sln -c Release
dotnet test Nexus.sln -c Release --no-build
```

### Launch Application
```powershell
dotnet run --project FileManager.csproj
```

---

## Testing & Quality Assurance
The project includes automated xUnit tests validating:
- **Case-insensitive extension matching** (with or without leading dot).
- **Size and date boundary filters** across all preset intervals.
- **Batch rename formatting** with various hash digit lengths (`#`, `###`).
- **ViewModel behavior** and dialog interactions via Moq.

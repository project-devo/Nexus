# Nexus - File Manager

A modern Windows file manager application built with .NET 8 WPF.

## Features

- **Location Scanner**: Scan Desktop, Documents, Downloads, Pictures, Music, Videos
- **Search & Filter**: Search by name, filter by extension, size, and date
- **File Operations**: Copy, Move, Delete, Rename files
- **Batch Operations**: Select multiple files and perform operations
- **Modern UI**: Light and Dark themes with excellent contrast
- **Theme Toggle**: Switch between light and dark modes instantly

## Prerequisites

You need **.NET 8 SDK** installed. Download from:
https://dotnet.microsoft.com/download/dotnet/8.0

## Build & Run

1. Open PowerShell or Command Prompt in the `Nexus` folder
2. Run:
   ```bash
   dotnet restore
   dotnet build
   dotnet run
   ```

Or publish as standalone executable:
```bash
dotnet publish -c Release -o ./publish
```

## Project Structure

```
Nexus/
├── Models/           # Data models (FileItem, ScanLocation, Filters)
├── ViewModels/       # MVVM ViewModels (Main, FolderBrowser, BatchRename)
├── Views/            # XAML Views (MainWindow)
├── Services/         # Business logic (Scanner, Search, FileOperations)
├── Commands/         # ICommand implementations
├── Converters/       # Value converters for XAML bindings
├── App.xaml          # Application entry point
└── Nexus.csproj      # Project file
```

## Usage

1. **Scan Locations**: Click "🔍 Scan" to scan all enabled locations
2. **Select Files**: Use checkboxes to select files
3. **Search**: Type in the search box to filter results
4. **Filter**: Use dropdown filters for extension, size, date
5. **Sort**: Click sort dropdown and ⬆/⬇ button to change order
6. **Operations**: Use Copy, Move, Delete buttons
7. **Theme**: Click 🌙/☀️ to toggle light/dark mode

## Keyboard Shortcuts

- `Ctrl+A` - Select all files
- `Delete` - Delete selected files
- `Ctrl+C` - Copy selected files
- `Ctrl+V` - Move selected files

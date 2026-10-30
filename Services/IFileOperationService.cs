using System.Collections.Generic;
using System.Threading.Tasks;
using Nexus.Models;

namespace Nexus.Services;

public interface IFileOperationService
{
    Task<bool> CopyFilesAsync(IEnumerable<FileItem> files, string destinationPath);
    Task<bool> MoveFilesAsync(IEnumerable<FileItem> files, string destinationPath);
    Task<bool> DeleteFilesAsync(IEnumerable<FileItem> files, bool useRecycleBin = true);
    Task<bool> RenameFileAsync(FileItem file, string newName);
    Task<bool> BatchRenameAsync(IEnumerable<FileItem> files, string pattern, bool includeExtension = false);
    Task<bool> CreateFolderAsync(string parentPath, string folderName);
    void OpenFile(FileItem file);
    void OpenContainingFolder(FileItem file);
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nexus.Models;

namespace Nexus.Services;

public class FileOperationService : IFileOperationService
{
    public async Task<bool> CopyFilesAsync(IEnumerable<FileItem> files, string destinationPath)
    {
        return await Task.Run(() =>
        {
            bool success = true;
            foreach (var file in files)
            {
                try
                {
                    if (file.IsDirectory)
                    {
                        CopyDirectory(file.FullName, Path.Combine(destinationPath, file.Name));
                    }
                    else
                    {
                        string destFile = Path.Combine(destinationPath, file.Name);
                        File.Copy(file.FullName, destFile, true);
                    }
                }
                catch
                {
                    success = false;
                }
            }
            return success;
        });
    }

    public async Task<bool> MoveFilesAsync(IEnumerable<FileItem> files, string destinationPath)
    {
        return await Task.Run(() =>
        {
            bool success = true;
            foreach (var file in files)
            {
                try
                {
                    if (file.IsDirectory)
                    {
                        MoveDirectorySafely(file.FullName, Path.Combine(destinationPath, file.Name));
                    }
                    else
                    {
                        string destFile = Path.Combine(destinationPath, file.Name);
                        if (File.Exists(destFile))
                        {
                            File.Delete(destFile);
                        }
                        File.Move(file.FullName, destFile);
                    }
                }
                catch
                {
                    success = false;
                }
            }
            return success;
        });
    }

    public async Task<bool> DeleteFilesAsync(IEnumerable<FileItem> files, bool useRecycleBin = true)
    {
        return await Task.Run(() =>
        {
            bool success = true;
            foreach (var file in files)
            {
                try
                {
                    if (file.IsDirectory)
                    {
                        if (useRecycleBin)
                        {
                            Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(
                                file.FullName,
                                Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                                Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                        }
                        else
                        {
                            Directory.Delete(file.FullName, true);
                        }
                    }
                    else
                    {
                        if (useRecycleBin)
                        {
                            Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(
                                file.FullName,
                                Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                                Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                        }
                        else
                        {
                            File.Delete(file.FullName);
                        }
                    }
                }
                catch
                {
                    success = false;
                }
            }
            return success;
        });
    }

    public async Task<bool> RenameFileAsync(FileItem file, string newName)
    {
        return await Task.Run(() =>
        {
            try
            {
                string newPath = Path.Combine(file.ParentPath, newName);
                if (file.IsDirectory)
                {
                    Directory.Move(file.FullName, newPath);
                }
                else
                {
                    File.Move(file.FullName, newPath);
                }
                file.FullName = newPath;
                file.Name = newName;
                file.DisplayName = newName;
                return true;
            }
            catch
            {
                return false;
            }
        });
    }

    public async Task<bool> BatchRenameAsync(IEnumerable<FileItem> files, string pattern, bool includeExtension = false)
    {
        return await Task.Run(() =>
        {
            bool success = true;
            int index = 1;

            // Compute hash pattern length (e.g. ### -> 3, ## -> 2, # -> 1)
            int hashCount = 0;
            int firstHash = pattern.IndexOf('#');
            if (firstHash >= 0)
            {
                int endHash = firstHash;
                while (endHash < pattern.Length && pattern[endHash] == '#')
                {
                    endHash++;
                }
                hashCount = endHash - firstHash;
            }

            string hashPlaceholder = hashCount > 0 ? new string('#', hashCount) : "#";
            string formatString = hashCount > 1 ? $"D{hashCount}" : "D";

            foreach (var file in files)
            {
                try
                {
                    string extension = Path.GetExtension(file.Name);
                    string formattedIndex = index.ToString(formatString);
                    string newBaseName = pattern.Contains('#') 
                        ? pattern.Replace(hashPlaceholder, formattedIndex) 
                        : $"{pattern}_{formattedIndex}";

                    string newName = newBaseName;
                    if (!includeExtension && !file.IsDirectory)
                    {
                        newName += extension;
                    }

                    string newPath = Path.Combine(file.ParentPath, newName);
                    if (file.IsDirectory)
                    {
                        Directory.Move(file.FullName, newPath);
                    }
                    else
                    {
                        File.Move(file.FullName, newPath);
                    }

                    file.FullName = newPath;
                    file.Name = newName;
                    file.DisplayName = newName;
                    index++;
                }
                catch
                {
                    success = false;
                }
            }
            return success;
        });
    }

    public async Task<bool> CreateFolderAsync(string parentPath, string folderName)
    {
        return await Task.Run(() =>
        {
            try
            {
                string fullPath = Path.Combine(parentPath, folderName);
                Directory.CreateDirectory(fullPath);
                return true;
            }
            catch
            {
                return false;
            }
        });
    }

    public void OpenFile(FileItem file)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = file.FullName,
                UseShellExecute = true
            });
        }
        catch { }
    }

    public void OpenContainingFolder(FileItem file)
    {
        try
        {
            string targetPath = file.IsDirectory ? file.FullName : file.ParentPath;
            if (File.Exists(file.FullName) && !file.IsDirectory)
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{file.FullName}\"",
                    UseShellExecute = true
                });
            }
            else if (Directory.Exists(targetPath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = targetPath,
                    UseShellExecute = true
                });
            }
        }
        catch { }
    }

    private void CopyDirectory(string sourceDir, string destinationDir)
    {
        Directory.CreateDirectory(destinationDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            string destFile = Path.Combine(destinationDir, Path.GetFileName(file));
            File.Copy(file, destFile, true);
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            string destDir = Path.Combine(destinationDir, Path.GetFileName(dir));
            CopyDirectory(dir, destDir);
        }
    }

    private void MoveDirectorySafely(string sourceDir, string destinationDir)
    {
        if (!Directory.Exists(destinationDir))
        {
            Directory.Move(sourceDir, destinationDir);
            return;
        }

        // Merge directories safely rather than deleting the destination
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            string destFile = Path.Combine(destinationDir, Path.GetFileName(file));
            if (File.Exists(destFile))
            {
                File.Delete(destFile);
            }
            File.Move(file, destFile);
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            string destDir = Path.Combine(destinationDir, Path.GetFileName(dir));
            MoveDirectorySafely(dir, destDir);
        }

        if (Directory.Exists(sourceDir) && !Directory.EnumerateFileSystemEntries(sourceDir).Any())
        {
            Directory.Delete(sourceDir);
        }
    }
}

using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Items;
using File.Manager.API.Filesystem.Models.Operator;
using File.Manager.API.Filesystem.Models.Plan;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Modules.Filesystem.Local
{
    internal class LocalOperator : IFilesystemOperator
    {
        // Private fields -----------------------------------------------------

        private readonly string startPath;
        private readonly string currentPath;

        // Private methods ----------------------------------------------------

        private List<BasePlanItem> CreatePlanForFolderRecursive(string address, string fileMaskOverride, IReadOnlyList<Item> selectedItems)
        {
            var result = new List<BasePlanItem>();

            var info = new DirectoryInfo(address);

            foreach (var directory in info.GetDirectories())
            {
                if (selectedItems != null && selectedItems.FirstOrDefault(i => i is FolderItem folderItem && folderItem.Name.ToLowerInvariant() == directory.Name.ToLowerInvariant()) == null)
                    continue;

                try
                {
                    var directoryItems = CreatePlanForFolderRecursive(Path.Combine(address, directory.Name), fileMaskOverride, null);
                    var directoryItem = new PlanFolder(directory.Name, directoryItems);
                    result.Add(directoryItem);
                }
                catch
                {
                    // Intentionally left empty (folder won't be taken into account in the plan)
                    // TODO Ask user?
                }
            }

            var files = string.IsNullOrEmpty(fileMaskOverride) ? info.GetFiles() : info.GetFiles(fileMaskOverride);

            foreach (var file in files)
            {
                if (selectedItems != null && selectedItems.FirstOrDefault(i => i is FileItem fileItem && fileItem.Name.ToLowerInvariant() == file.Name.ToLowerInvariant()) == null)
                    continue;

                var fileItem = new PlanFile(file.Name,
                    file.Length,
                    file.Attributes.HasFlag(FileAttributes.ReadOnly),
                    file.Attributes.HasFlag(FileAttributes.Hidden),
                    file.Attributes.HasFlag(FileAttributes.System));
                result.Add(fileItem);
            }

            return result;
        }

        // Public methods -----------------------------------------------------

        public LocalOperator(string startPath)
        {
            this.startPath = startPath;
            this.currentPath = startPath;
        }

        public OperationPlan BuildOperationPlanFromSelection(IReadOnlyList<Item> selectedItems, string fileMaskOverride)
        {
            var items = CreatePlanForFolderRecursive(startPath, fileMaskOverride, selectedItems);
            return new OperationPlan(items);
        }

        public bool CreateFolder(string name)
        {
            try
            {
                Directory.CreateDirectory(System.IO.Path.Combine(currentPath, name));
            }
            catch
            {
                return false;
            }

            return true;
        }

        public bool DeleteFile(string name)
        {
            try
            {
                System.IO.File.Delete(System.IO.Path.Combine(currentPath, name));
            }
            catch
            {
                return false;
            }

            return true;
        }

        public bool DeleteFolder(string name)
        {
            try
            {
                Directory.Delete(System.IO.Path.Combine(currentPath, name));
            }
            catch
            {
                return false;
            }

            return true;
        }

        public void Dispose()
        {

        }

        public IFilesystemOperator EnterFolder(string name)
        {
            var targetPath = System.IO.Path.Combine(currentPath, name);

            if (!Directory.Exists(targetPath))
                return null;

            try
            {
                var info = new DirectoryInfo(targetPath);
            }
            catch
            {
                return null;
            }

            return new LocalOperator(targetPath);
        }

        public bool FileExists(string name)
            => System.IO.File.Exists(Path.Combine(currentPath, name));

        public bool FolderExists(string name)
            => Directory.Exists(Path.Combine(currentPath, name));

        public IReadOnlyList<BaseOperatorItem> List()
        {
            var result = new List<BaseOperatorItem>();

            var info = new DirectoryInfo(currentPath);

            result.AddRange(info.GetDirectories()
                .Select(directory => new OperatorFolderItem(directory.Name)));

            result.AddRange(info.GetFiles()
                .Select(file => new OperatorFileItem(file.Name,
                    file.Length,
                    file.Attributes.HasFlag(FileAttributes.ReadOnly),
                    file.Attributes.HasFlag(FileAttributes.Hidden),
                    file.Attributes.HasFlag(FileAttributes.System))));

            return result;
        }

        public Stream OpenFileForReading(string name)
        {
            try
            {
                return new FileStream(Path.Combine(currentPath, name), FileMode.Open, FileAccess.Read);
            }
            catch
            {
                return null;
            }
        }

        public Stream OpenFileForWriting(string name)
        {
            try
            {
                return new FileStream(Path.Combine(currentPath, name), FileMode.Create, FileAccess.Write);
            }
            catch
            {
                return null;    
            }
        }

        public FileAttributes? GetFileAttributes(string targetName)
        {
            try
            {
                var fi = new FileInfo(Path.Combine(currentPath, targetName));
                return fi.Attributes;
            }
            catch
            {
                return null;
            }
        }

        public bool SetFileAttributes(string targetName, FileAttributes attributes)
        {
            try
            {
                var fi = new FileInfo(Path.Combine(currentPath, targetName));
                fi.Attributes = attributes;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string CurrentPath => currentPath;
    }
}

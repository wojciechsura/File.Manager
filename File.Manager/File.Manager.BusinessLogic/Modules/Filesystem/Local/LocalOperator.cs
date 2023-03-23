using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Items.Listing;
using File.Manager.API.Filesystem.Models.Items.Operator;
using File.Manager.API.Filesystem.Models.Items.Plan;
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
    internal class LocalOperator : FilesystemOperator
    {
        // Private fields -----------------------------------------------------

        private readonly string currentPath;
        private readonly string startPath;
        // Private methods ----------------------------------------------------

        private List<BasePlanItem> CreatePlanForFolderRecursive(string address, 
            string fileMaskOverride, 
            IReadOnlyList<Item> selectedItems)
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

                try
                {
                    var fileItem = new PlanFile(file.Name,
                        file.Length,
                        file.Attributes.HasFlag(FileAttributes.ReadOnly),
                        file.Attributes.HasFlag(FileAttributes.Hidden),
                        file.Attributes.HasFlag(FileAttributes.System));
                    result.Add(fileItem);
                }
                catch
                {
                    // Intentionally left empty (folder won't be taken into account in the plan)
                    // TODO Ask user?
                }
            }

            return result;
        }

        // Public methods -----------------------------------------------------

        public LocalOperator(string startPath)
        {
            this.startPath = startPath;
            this.currentPath = startPath;
        }

        public override OperationPlan BuildOperationPlanFromSelection(IReadOnlyList<Item> selectedItems, string? fileMaskOverride)
        {
            if (selectedItems == null)
                throw new ArgumentNullException(nameof(selectedItems));

            var items = CreatePlanForFolderRecursive(startPath, fileMaskOverride, selectedItems);
            return new OperationPlan(items);
        }

        public override bool? CheckIsSubfolderEmpty(string name)
        {
            var targetPath = System.IO.Path.Combine(currentPath, name);

            if (!Directory.Exists(targetPath))
                return null;

            try
            {
                var info = new DirectoryInfo(targetPath);
                return !info.EnumerateFileSystemInfos().Any();
            }
            catch
            {
                return null;
            }
        }

        public override bool CreateFolder(string name)
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

        public override bool DeleteEmptyFolder(string name)
        {
            try
            {
                Directory.Delete(System.IO.Path.Combine(currentPath, name), false);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public override bool DeleteFile(string name)
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

        public override void Dispose()
        {

        }

        public override FilesystemOperator EnterFolder(string name)
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

        public override bool? FileExists(string name)
        {
            try
            {
                return System.IO.File.Exists(Path.Combine(currentPath, name));
            }
            catch
            {
                return null;
            }
        }

        public override bool? FolderExists(string name)
        {
            try
            {
                return Directory.Exists(Path.Combine(currentPath, name));
            }
            catch
            {
                return null;
            }
        }

        public override FileAttributes? GetFileAttributes(string targetName)
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

        public override IReadOnlyList<BaseOperatorItem> List(IReadOnlyList<Item> selectedItems, string fileMaskOverride)
        {
            var result = new List<BaseOperatorItem>();

            var info = new DirectoryInfo(startPath);

            foreach (var directory in info.GetDirectories())
            {
                if (selectedItems != null && selectedItems.FirstOrDefault(i => i is FolderItem folderItem && folderItem.Name.ToLowerInvariant() == directory.Name.ToLowerInvariant()) == null)
                    continue;

                try
                {
                    var directoryItem = new OperatorFolderItem(directory.Name);
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

                try
                {
                    var fileItem = new OperatorFileItem(file.Name,
                        file.Length,
                        file.Attributes.HasFlag(FileAttributes.ReadOnly),
                        file.Attributes.HasFlag(FileAttributes.Hidden),
                        file.Attributes.HasFlag(FileAttributes.System));
                    result.Add(fileItem);
                }
                catch
                {
                    // Intentionally left empty (file won't be taken into account in the plan)
                    // TODO Ask user?
                }
            }

            return result;

        }

        public override Stream OpenFileForReading(string name)
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

        public override Stream OpenFileForWriting(string name)
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
        public override bool SetFileAttributes(string targetName, FileAttributes attributes)
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
        public override string CurrentPath => currentPath;
    }
}

﻿using File.Manager.API.Filesystem;
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
        private string currentPath;

        // Private methods ----------------------------------------------------

        private List<BasePlanItem> CreatePlanForFolderRecursive(string address, string fileMaskOverride, IReadOnlyList<Item> selectedItems)
        {
            var result = new List<BasePlanItem>();

            var info = new DirectoryInfo(address);

            foreach (var directory in info.GetDirectories())
            {
                if (selectedItems != null && selectedItems.FirstOrDefault(i => i is FolderItem folderItem && folderItem.Name.ToLowerInvariant() == directory.Name.ToLowerInvariant()) == null)
                    continue;

                var directoryItems = CreatePlanForFolderRecursive(Path.Combine(address, directory.Name), fileMaskOverride, null);
                var directoryItem = new PlanFolder(directory.Name, directoryItems);
                result.Add(directoryItem);
            }

            var files = string.IsNullOrEmpty(fileMaskOverride) ? info.GetFiles(fileMaskOverride) : info.GetFiles();

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

        public bool EnterFolder(string name)
        {
            var targetPath = System.IO.Path.Combine(currentPath, name);

            if (!Directory.Exists(targetPath))
                return false;

            try
            {
                var info = new DirectoryInfo(targetPath);
            }
            catch
            {
                return false;
            }

            currentPath = targetPath;
            return true;
        }

        public bool ExitFolder()
        {
            try
            {
                if (currentPath.ToLower() == startPath.ToLower())
                    throw new InvalidOperationException("Operator cannot exit the start path!");

                currentPath = Path.GetFullPath(Path.Combine(currentPath, ".."));
            }
            catch
            {
                return false;
            }

            return true;
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
            => new FileStream(Path.Combine(currentPath, name), FileMode.Open, FileAccess.Read);

        public Stream OpenFileForWriting(string name)
            => new FileStream(Path.Combine(currentPath, name), FileMode.Open, FileAccess.Read);

        public void ReturnToStartLocation()
        {
            currentPath = startPath;
        }

        public string CurrentPath => currentPath;
    }
}

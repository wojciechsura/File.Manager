using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Items.Listing;
using File.Manager.API.Filesystem.Models.Items.Operator;
using File.Manager.API.Filesystem.Models.Items.Plan;
using File.Manager.Common.Helpers;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Speech.Synthesis.TtsEngine;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace File.Manager.BusinessLogic.Modules.Filesystem.Zip
{
    public class ZipOperator : IFilesystemOperator
    {
        // Private fields -----------------------------------------------------

        private readonly string rootPath;
        private readonly string zipFilePath;
        private readonly ZipFile zipFile;

        // Private methods ----------------------------------------------------

        private List<BasePlanItem> CreatePlanForFolderRecursive(string path, List<ZipEntry> zipEntries, string fileMaskOverride, IReadOnlyList<Item> selectedItems)
        {
            HashSet<string> foldersToProcess = new();
            List<ZipEntry> subfolderItemsToProcess = new();

            List<BasePlanItem> result = new();

            foreach (var entry in zipEntries)
            {
                (string location, string name) = ZipPathTools.GetZipEntryLocation(entry.Name, true);

                // Check if entry is in this folder
                if (!location.StartsWith(path))
                    continue;

                // Check if entry is directly in this folder
                if (location == path)
                {
                    if (selectedItems != null && selectedItems.FirstOrDefault(si => si.Name == name) == null)
                        continue;

                    // Entry directly in this folder

                    if (entry.IsFile)
                    {
                        if (!string.IsNullOrEmpty(fileMaskOverride) && !PatternMatcher.StrictMatchPattern(fileMaskOverride, name))
                            continue;

                        var planFile = new PlanFile(name,
                            entry.Size,
                            false,
                            false,
                            false);
                        result.Add(planFile);
                    }
                    else
                    {
                        if (!foldersToProcess.Contains(entry.Name))
                            foldersToProcess.Add(name);
                    }
                }
                else
                {
                    // Entry in subfolder
                    // Extract the direct subfolder name

                    int slashPos = location.IndexOf('/', path.Length);
                    if (slashPos == -1)
                        slashPos = location.Length;
                    var subfolderName = location[path.Length..(slashPos)];

                    if (selectedItems != null && selectedItems.FirstOrDefault(si => si.Name == subfolderName) == null)
                        continue;

                    if (!foldersToProcess.Contains(subfolderName))
                    {
                        foldersToProcess.Add(subfolderName);
                    }

                    // Collect all items indirectly below
                    // current location to speed up processing of
                    // subfolders (there won't be need to process
                    // all zip entries, which doesn't match the current
                    // location or were already processed)
                    subfolderItemsToProcess.Add(entry);
                }
            }

            // Now recursively process subfolders
            foreach (var folder in foldersToProcess)
            {
                var planItems = CreatePlanForFolderRecursive($"{path}{folder}/",
                    subfolderItemsToProcess,
                    fileMaskOverride,
                    null);

                var planFolder = new PlanFolder(folder, planItems);
                result.Add(planFolder);
            }

            return result;
        }

        // Public methods -----------------------------------------------------

        public ZipOperator(ZipFile zipFile, string zipPath, string rootPath)
        {
            this.zipFile = zipFile ?? throw new ArgumentNullException(nameof(zipFile));
            this.zipFilePath = zipPath ?? throw new ArgumentNullException(nameof(zipPath));

            if (!string.IsNullOrEmpty(rootPath) && !rootPath.EndsWith('/'))
                rootPath = $"{rootPath}/";

            this.rootPath = rootPath;
        }

        public OperationPlan BuildOperationPlanFromSelection(IReadOnlyList<Item> selectedItems, string fileMaskOverride)
        {
            if (selectedItems == null)
                throw new ArgumentNullException(nameof(selectedItems));

            var zipEntries = zipFile.Cast<ZipEntry>().ToList();

            var items = CreatePlanForFolderRecursive(rootPath, zipEntries, fileMaskOverride, selectedItems);
            return new OperationPlan(items);
        }

        public bool? CheckIsSubfolderEmpty(string name)
        {
            var subfolderPath = $"{rootPath}{name}/";

            return !zipFile
                .Cast<ZipEntry>()
                .Any(ze => ze.Name.StartsWith(subfolderPath) && ze.Name.Length > subfolderPath.Length);
        }

        public bool CreateFolder(string name)
        {
            try
            {
                zipFile.BeginUpdate();

                var subfolderPath = $"{rootPath}{name}/";
                zipFile.AddDirectory(subfolderPath);

                zipFile.CommitUpdate();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteFile(string name)
        {
            try
            {
                string filePath = $"{rootPath}{name}";
                var entry = zipFile.GetEntry(filePath);

                if (!entry.IsFile)
                    return false;

                zipFile.BeginUpdate();
                zipFile.Delete(filePath);
                zipFile.CommitUpdate();

                return true;
            }
            catch
            {
                zipFile.AbortUpdate();
                return false;
            }
        }

        public bool DeleteEmptyFolder(string name)
        {
            try
            {
                string folderPath = $"{rootPath}{name}/";
                var entry = zipFile.GetEntry(folderPath);

                if (entry.IsFile)
                    return false;

                if (CheckIsSubfolderEmpty(name) != true)
                    return false;
                
                zipFile.BeginUpdate();
                zipFile.Delete(entry);
                zipFile.CommitUpdate();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            // Nothing to do here. zipFile will get disposed with
            // its navigator.
        }

        public IFilesystemOperator EnterFolder(string name)
        {
            return new ZipOperator(zipFile, zipFilePath, $"{rootPath}{name}/");
        }

        public bool? FileExists(string name)
        {
            var filePath = $"{rootPath}{name}";
            return zipFile.Cast<ZipEntry>()
                .Any(e => e.Name == filePath && e.IsFile);
        }

        public bool? FolderExists(string name)
        {
            var folderPath = $"{rootPath}{name}/";
            return zipFile.Cast<ZipEntry>()
                .Any(e => e.Name == folderPath && !e.IsFile);
        }

        public FileAttributes? GetFileAttributes(string targetName)
        {
            // TODO handle attributes
            return (FileAttributes)0;
        }

        public IReadOnlyList<BaseOperatorItem> List(IReadOnlyList<Item> selectedItems, string fileMaskOverride)
        {
            var zipEntries = zipFile.OfType<ZipEntry>();

            HashSet<string> subfolders = new();

            List<BaseOperatorItem> result = new();

            foreach (var entry in zipEntries)
            {
                (string location, string name) = ZipPathTools.GetZipEntryLocation(entry.Name);

                // Check if entry is in this folder
                if (!location.StartsWith(rootPath))
                    continue;

                // Check if entry is directly in this folder
                if (location == rootPath)
                {
                    // Entry directly in this folder

                    if (entry.IsFile)
                    {
                        if (!string.IsNullOrEmpty(fileMaskOverride) && !PatternMatcher.StrictMatchPattern(fileMaskOverride, name))
                            continue;

                        // TODO handle attributes
                        var operatorFile = new OperatorFileItem(name,
                            entry.Size,
                            false,
                            false,
                            false);
                        result.Add(operatorFile);
                    }
                    else
                    {
                        if (!subfolders.Contains(entry.Name))
                        {
                            subfolders.Add(name);

                            var operatorFolder = new OperatorFolderItem(name);
                            result.Add(operatorFolder);
                        }
                    }
                }
                else
                {
                    // Entry in subfolder
                    // Extract the direct subfolder name

                    int slashPos = location.IndexOf('/', rootPath.Length);
                    if (slashPos == -1)
                        slashPos = location.Length;
                    var subfolderName = location[rootPath.Length..(slashPos - 1)];
                    if (!subfolders.Contains(subfolderName))
                    {
                        subfolders.Add(subfolderName);

                        var operatorFolder = new OperatorFolderItem(name);
                        result.Add(operatorFolder);
                    }
                }
            }

            return result;
        }

        public Stream OpenFileForReading(string name)
        {
            try
            {
                string path = $"{rootPath}{name}";

                var entry = zipFile.GetEntry(path);
                if (entry == null)
                    return null;

                return zipFile.GetInputStream(entry);
            }
            catch
            {
                return null;
            }
        }

        public Stream OpenFileForWriting(string name)
        {
            string path = $"{rootPath}{name}";
            return new ZipFileCreateStream(zipFile, path);
        }

        public bool SetFileAttributes(string targetName, FileAttributes attributes)
        {
            // TODO handle attributes
            return true;
        }

        // Public properties --------------------------------------------------

        public string CurrentPath => ZipPathTools.BuildAddress(zipFilePath, rootPath);
    }
}

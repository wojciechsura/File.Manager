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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Shapes;

namespace File.Manager.BusinessLogic.Modules.Filesystem.Zip
{
    public class ZipOperator : FilesystemOperator
    {
        // Private types ------------------------------------------------------

        private class StreamZipSource : IStaticDataSource
        {
            private readonly Stream stream;

            public StreamZipSource(Stream stream)
            {
                this.stream = stream;
            }

            public Stream GetSource() => stream;
        }

        // Plan item sources - used while building operation plan

        private abstract class BasePlanItemSource
        {
            public abstract BasePlanItem ToPlanItem();

            public abstract string Name { get; }
        }

        private class PlanFileSource : BasePlanItemSource
        {
            private PlanFile planFile;

            public override BasePlanItem ToPlanItem() => planFile;

            public PlanFileSource(PlanFile planFile)
            {
                if (string.IsNullOrEmpty(planFile.Name))
                    throw new ArgumentException(nameof(planFile));

                this.planFile = planFile;
            }

            public override string Name => planFile.Name;
        }

        private abstract class BasePlanFolderSource : BasePlanItemSource
        {
            public List<BasePlanItemSource> Items { get; } = new();
        }

        private class PlanFolderSource : BasePlanFolderSource
        {
            private string name;

            public override BasePlanItem ToPlanItem()
            {
                return new PlanFolder(name, Items.Select(i => i.ToPlanItem()).ToList());
            }

            public PlanFolderSource(string name)
            {
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentException(nameof(name));

                this.name = name;
            }

            public override string Name => name;            
        }

        private class RootSource : BasePlanFolderSource
        {
            private string rootPath;
            private string name;

            public RootSource(string rootPath)
            {
                this.rootPath = rootPath == string.Empty || rootPath.EndsWith('/') ? rootPath : rootPath + '/';
                this.name = rootPath == string.Empty ? rootPath : rootPath[..^1];
            }

            public override BasePlanItem ToPlanItem()
            {
                throw new InvalidOperationException("Do not call ToPlanItem on root!");
            }

            public override string Name => name;
            public string RootPath => rootPath;
        }

        private class LocationStack
        {
            private RootSource root;
            private readonly List<BasePlanFolderSource> stack = new();
            private string currentLocationCache = null;

            private void BuildCurrentLocation()
            {
                currentLocationCache = root.RootPath + string.Join('/', stack
                    .Select(s => s.Name)
                    .Where(x => !string.IsNullOrEmpty(x)));

                if (currentLocationCache != string.Empty && !currentLocationCache.EndsWith('/'))
                    currentLocationCache += "/";
            }

            public LocationStack(string rootPath)
            {
                root = new RootSource(rootPath);
                stack.Add(root);
                currentLocationCache = null;
            }

            public void ReachLocation(string location)
            {
                // Check if root location matches
                if (!location.StartsWith(root.RootPath))
                    throw new ArgumentException("Location is not rooted in the same root path as the location stack!");

                // Normalize the location to be reached
                if (location != string.Empty && !location.EndsWith('/'))
                    location += '/';

                // Exit upwards until current location
                // becomes prefix to the required location
                while (!location.StartsWith(CurrentLocation))
                {
                    // If we're trying to exit root folder,
                    // there's something wrong with the algorithm.
                    // Throw an exception to prevent any damage
                    if (stack.Last() is RootSource)
                        throw new InvalidOperationException("Trying to exit root folder - error in algorithm!");
                    
                    stack.RemoveAt(stack.Count - 1);
                    currentLocationCache = null;
                }

                // Do we need to go deeper?
                if (location.Length > CurrentLocation.Length)
                {
                    // Go downwards to reach requested location
                    string[] folders = location[CurrentLocation.Length..^1].Split('/');

                    foreach (var folder in folders)
                    {
                        var existingFolder = CurrentItem.Items.FirstOrDefault(i => i.Name == folder);
                        if (existingFolder != null)
                        {
                            if (existingFolder is BasePlanFolderSource planFolder)
                            {
                                stack.Add(planFolder);
                                currentLocationCache = null;
                                continue;
                            }
                            else if (existingFolder is PlanFileSource)
                            {
                                throw new InvalidOperationException("Cannot reach location: one of its parts is an existing file!");
                            }
                        }

                        var newFolder = new PlanFolderSource(folder);
                        CurrentItem.Items.Add(newFolder);
                        stack.Add(newFolder);
                        currentLocationCache = null;
                    }
                }                
            }

            public string CurrentLocation
            {
                get
                {
                    if (currentLocationCache == null)
                        BuildCurrentLocation();

                    return currentLocationCache;
                }
            }

            public BasePlanFolderSource CurrentItem => stack.Last();

            public RootSource Root => root;
        }

        // Private fields -----------------------------------------------------

        private readonly string rootPath;
        private readonly string zipFilePath;
        private readonly ZipFile zipFile;

        // Private methods ----------------------------------------------------

        private List<BasePlanItem> InternalCreatePlanForFolder(string rootPath, 
            List<ZipEntry> zipEntries, 
            string fileMaskOverride, 
            IReadOnlyList<Item> selectedItems)
        {
            var locationStack = new LocationStack(rootPath);
            
            foreach (var entry in zipEntries)
            {
                (string location, string name) = ZipPathTools.GetZipEntryLocation(entry.Name, true);

                // Item is outside location we're processing
                if (!location.StartsWith(locationStack.Root.RootPath))
                    continue;

                // If items were selected, current item is on the root level
                // and it is not among selected ones, skip it
                if (selectedItems != null && 
                    location == locationStack.Root.RootPath && 
                    !selectedItems.Any(si => si.Name == name))
                    continue;

                // If file mask override is in place and current item
                // does not match the mask, skip it
                if (entry.IsFile && !string.IsNullOrEmpty(fileMaskOverride) && !PatternMatcher.StrictMatchPattern(fileMaskOverride, name))
                    continue;

                // Reach item's location in the stack
                locationStack.ReachLocation(location);

                if (entry.IsFile)
                {
                    if (locationStack.CurrentItem.Items.Any(i => i.Name == name))
                        throw new InvalidOperationException("There already is an item with given name in the folder!");

                    // If it is a file, simply add it to the current location                  

                    var planFile = new PlanFile(name,
                        entry.Size,
                        false,
                        false,
                        false);
                    locationStack.CurrentItem.Items.Add(new PlanFileSource(planFile));
                }
                else
                {
                    // If it is a folder, either create it or update its parameters
                    var existingFolder = locationStack.CurrentItem.Items.FirstOrDefault(i => i.Name == name);
                    if (existingFolder != null)
                    {
                        if (existingFolder is PlanFolderSource planFolderSource)
                        {
                            // TODO update information
                        }
                        else
                        {
                            throw new InvalidOperationException("There already is an item with given name in the folder!");
                        }
                    }
                    else
                    {
                        var folder = new PlanFolderSource(name);
                        locationStack.CurrentItem.Items.Add(folder);
                    }
                }
            }

            return locationStack.Root.Items
                .Select(i => i.ToPlanItem())
                .ToList();
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

        public override OperationPlan BuildOperationPlanFromSelection(IReadOnlyList<Item> selectedItems, string fileMaskOverride)
        {
            if (selectedItems == null)
                throw new ArgumentNullException(nameof(selectedItems));

            var zipEntries = zipFile.Cast<ZipEntry>().ToList();

            var items = InternalCreatePlanForFolder(rootPath, zipEntries, fileMaskOverride, selectedItems);
            return new OperationPlan(items);
        }

        public override bool? CheckIsSubfolderEmpty(string name)
        {
            var subfolderPath = $"{rootPath}{name}/";

            return !zipFile
                .Cast<ZipEntry>()
                .Any(ze => ze.Name.StartsWith(subfolderPath) && ze.Name.Length > subfolderPath.Length);
        }

        public override bool CreateFolder(string name)
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

        public override bool DeleteFile(string name)
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

        public override bool DeleteEmptyFolder(string name)
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

        public override void Dispose()
        {
            // Nothing to do here. zipFile will get disposed with
            // its navigator.
        }

        public override FilesystemOperator EnterFolder(string name)
        {
            return new ZipOperator(zipFile, zipFilePath, $"{rootPath}{name}/");
        }

        public override bool? FileExists(string name)
        {
            var filePath = $"{rootPath}{name}";
            return zipFile.Cast<ZipEntry>()
                .Any(e => e.Name == filePath && e.IsFile);
        }

        public override bool? FolderExists(string name)
        {
            var folderPath = $"{rootPath}{name}/";
            return zipFile.Cast<ZipEntry>()
                .Any(e => e.Name == folderPath && !e.IsFile);
        }

        public override FileAttributes? GetFileAttributes(string targetName)
        {
            // TODO handle attributes
            return (FileAttributes)0;
        }

        public override IReadOnlyList<BaseOperatorItem> List(IReadOnlyList<Item> selectedItems, string fileMaskOverride)
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

        public override Stream OpenFileForReading(string name)
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

        public override Stream OpenFileForWriting(string name)
        {
            return new MemoryStream();            
        }

        public override bool CloseWrittenFile(Stream stream, string name)
        {
            try
            {
                var ms = (MemoryStream)stream;
                ms.Seek(0, SeekOrigin.Begin);

                var path = $"{rootPath}{name}";

                var entry = new ZipEntry(path);

                zipFile.BeginUpdate();
                zipFile.Add(new StreamZipSource(ms), entry);
                zipFile.CommitUpdate();

                base.CloseWrittenFile(stream, name);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override bool SetFileAttributes(string targetName, FileAttributes attributes)
        {
            // TODO handle attributes
            return true;
        }

        // Public properties --------------------------------------------------

        public override string CurrentPath => ZipPathTools.BuildAddress(zipFilePath, rootPath);
    }
}

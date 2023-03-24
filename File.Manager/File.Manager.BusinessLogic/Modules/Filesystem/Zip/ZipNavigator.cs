using File.Manager.API.Exceptions.Filesystem;
using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Focus;
using File.Manager.API.Filesystem.Models.Items.Listing;
using File.Manager.API.Tools;
using File.Manager.API.Types;
using File.Manager.Resources.Modules.Filesystem.Zip;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Modules.Filesystem.Zip
{
    public class ZipNavigator : FilesystemNavigator
    {
        // Public constants ---------------------------------------------------
        
        public static readonly Regex ZipAddressRegex = new(@"\\\\Zip\\([^>]+)>(.*)");

        // Private types ------------------------------------------------------

        private sealed class ZipFileItem : FileItem
        {
            public ZipFileItem(ZipFolderItem parent, string name)
                : base(name, true)
            {
                Parent = parent;
            }

            public ZipFolderItem Parent { get; }
        }

        private sealed class ZipFolderItem : FolderItem
        {
            private readonly List<Item> items = new();

            public ZipFolderItem(ZipFolderItem parent, string name)
                : base(name)
            {
                Parent = parent;
            }

            public List<Item> Items => items;

            public string FullPath => string.IsNullOrEmpty(Location) ? Name : Location + $"/{Name}";

            public string Location => Parent == null ? string.Empty : $"{Parent.FullPath}";

            public ZipFolderItem Parent { get; }
        }

        private class ItemComparer : IComparer<Item>
        {
            public int Compare(Item x, Item y)
            {
                if (x.GetType() != y.GetType())
                {
                    if (x is UpFolderItem)
                        return -1;
                    else if (y is UpFolderItem)
                        return 1;
                    else if (x is ZipFolderItem)
                        return -1;
                    else if (y is ZipFolderItem)
                        return 1;
                    else
                        throw new InvalidOperationException("Invalid item!");
                }

                return x.Name.CompareTo(y.Name);
            }
        }

        // Private fields -----------------------------------------------------

        private static ItemComparer itemComparer = new();

        private string zipFilePath;
        private ZipFile zipFile;

        private List<Item> items;
        private ZipFolderItem root;
        private ZipFolderItem current;

        // Private methods ----------------------------------------------------

        private (string zipPath, string internalPath) GetPaths(string address)
        {
            var match = ZipAddressRegex.Match(address);
            if (!match.Success)
                throw new InvalidOperationException("Invalid zip module address!");

            return (match.Groups[1].Value, match.Groups[2].Value);
        }

        private ZipFolderItem BuildCache(ZipFile zipFile)
        {
            var cacheRoot = new ZipFolderItem(null, string.Empty);

            var locationCache = new Dictionary<string, ZipFolderItem>()
            {
                { string.Empty, cacheRoot }
            };

            ZipFolderItem EnsureLocation(string location)
            {
                if (locationCache.TryGetValue(location, out ZipFolderItem result))
                    return result;

                var parts = location.Split('/');
                var current = cacheRoot;
                foreach (var part in parts)
                {
                    var next = current.Items.FirstOrDefault(item => item.Name == part);
                    if (next != null)
                    {
                        if (next is ZipFolderItem zipFolderItem)
                        {
                            current = zipFolderItem;
                            continue;
                        }
                        else if (next is ZipFileItem)
                        {
                            throw new NavigationException(Strings.Error_DamagedZipFile);
                        }
                        else
                            throw new InvalidOperationException("Invalid item!");
                    }

                    // This generally shouldn't happen if all folders are
                    // specified in the zip file as separate entries. If
                    // we need to create folder this way, we will lose
                    // chance to extract metadata about it.
                    var nextFolder = new ZipFolderItem(current, part);
                    nextFolder.SizeDisplay = Strings.SizeDisplay_Directory;
                    current.Items.Add(nextFolder);

                    current = nextFolder;
                }

                locationCache[location] = current;

                return current;
            }

            foreach (var folder in zipFile
                .Cast<ZipEntry>()
                .Where(ze => !ze.IsFile)
                .OrderBy(ze => ze.Name))
            {
                // Safety measure
                if (folder.Name.Contains(".."))
                    continue;

                (string location, string name) = ZipPathTools.GetZipEntryLocation(folder.Name);

                var parentFolder = EnsureLocation(location);

                var item = new ZipFolderItem(parentFolder, name);
                item.Modified = folder.DateTime;
                item.SizeDisplay = Strings.SizeDisplay_Directory;
                parentFolder.Items.Add(item);
            }

            foreach (var file in zipFile
                .Cast<ZipEntry>()
                .Where(ze => ze.IsFile)
                .OrderBy(ze => ze.Name))
            {
                // Safety measure
                if (file.Name.Contains(".."))
                    continue;

                (string location, string name) = ZipPathTools.GetZipEntryLocation(file.Name);

                var parentFolder = EnsureLocation(location);

                var item = new ZipFileItem(parentFolder, name);
                item.Modified = file.DateTime;
                item.Size = file.Size;
                item.SizeDisplay = SizeTools.BytesToHumanReadable(file.Size);
                parentFolder.Items.Add(item);
            }

            return cacheRoot;
        }

        private void LoadItems()
        {
            items = new List<Item>
            {
                new UpFolderItem()
            };

            foreach (var item in current.Items.OrderBy(i => i, itemComparer))
            {
                items.Add(item);
            }

            Handler?.NotifyChanged(null);
            OnAddressChanged();
        }

        private void LoadFileStructureFromZip()
        {
            // Build structure

            this.root = BuildCache(zipFile);
            this.current = root;
        }

        private void NavigateToZipPath(string newPath)
        {
            // Locate inner folder if possible

            var parts = newPath.Split('\\')
                .Where(part => !string.IsNullOrEmpty(part));

            current = root;
            foreach (var part in parts)
            {
                var next = current.Items.FirstOrDefault(i => i.Name.ToLowerInvariant() == part.ToLowerInvariant());
                if (next is not ZipFolderItem zipFolderItem)
                    return;

                current = zipFolderItem;
            }
        }

        // Public methods -----------------------------------------------------

        public ZipNavigator(string address)
        {
            (zipFilePath, string innerPath) = GetPaths(address);

            // Load zip file

            try
            {
                var fs = new FileStream(zipFilePath, FileMode.Open, FileAccess.ReadWrite);
                zipFile = new ZipFile(fs);
            }
            catch
            {
                throw new NavigationException(Strings.Error_FailedToOpenZipFile);
            }

            LoadFileStructureFromZip();
            NavigateToZipPath(innerPath);
            LoadItems();
        }

        public override bool CanCustomEdit(Item focusedItem) => false;

        public override void CustomEdit(Item focusedItem)
            => throw new NotSupportedException();        

        public override FilesystemOperator CreateOperatorForCurrentLocation()
        {
            return new ZipOperator(zipFile, zipFilePath, current.FullPath);
        }

        public override void Dispose()
        {
            zipFile.Close();
            zipFile = null;
        }

        public override void Execute(Item item)
        {
            if (item is UpFolderItem)
            {
                var parent = current.Parent;
                if (parent != null)
                {
                    current = parent;
                    LoadItems();
                }
                else
                {
                    string address = System.IO.Path.GetDirectoryName(zipFilePath);
                    string filename = System.IO.Path.GetFileName(zipFilePath);
                    Handler?.RequestNavigateToAddress(address, new FilenameFocusedItemData(filename));
                }
            }
            else if (item is ZipFolderItem zipFolderItem)
            {
                current = zipFolderItem;
                LoadItems();
            }
        }

        public override LocationCapabilities GetLocationCapabilities()
        {
            return LocationCapabilities.BufferedRead |
                LocationCapabilities.BufferedWrite |
                LocationCapabilities.Plan |
                LocationCapabilities.CreateFolder |
                LocationCapabilities.Delete;
        }

        public override void Refresh()
        {
            var storedPath = current.FullPath;

            LoadFileStructureFromZip();
            NavigateToZipPath(storedPath);
            LoadItems();
        }

        public override Item ResolveFocusedItem(FocusedItemData data)
        {
            if (data is not FilenameFocusedItemData filenameData)
                return null;

            var filename = filenameData.Filename.ToLowerInvariant();

            foreach (var item in items)
            {
                if (item is ZipFileItem fileItem && fileItem.Name.ToLowerInvariant() == filename)
                    return item;
                else if (item is ZipFolderItem folderItem && folderItem.Name.ToLowerInvariant() == filename)
                    return item;
            }

            return null;
        }

        public static bool SupportsAddress(string address)
        {
            return ZipAddressRegex.IsMatch(address);
        }

        // Public properties --------------------------------------------------

        public override string Address => ZipPathTools.BuildAddress(zipFilePath, current?.FullPath ?? "");

        public override bool RestoreLocationAfterRestart => true;

        public override IReadOnlyList<Item> Items => items;
    }
}

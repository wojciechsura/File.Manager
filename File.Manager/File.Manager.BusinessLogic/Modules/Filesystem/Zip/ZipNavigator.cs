using File.Manager.API.Exceptions.Filesystem;
using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Focus;
using File.Manager.API.Filesystem.Models.Items.Listing;
using File.Manager.API.Types;
using File.Manager.Resources.Modules.Filesystem.Zip;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Modules.Filesystem.Zip
{
    public class ZipNavigator : FilesystemNavigator
    {
        // Public constants ---------------------------------------------------

        public const string ROOT_ADDRESS = @"\\Zip\";
        public static readonly Regex ZipAddressRegex = new Regex(@"\\\\Zip\\([^>]+)>(.*)");

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

            public string FullPath => Location + $"{Name}\\";

            public string Location => Parent?.FullPath ?? string.Empty;

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
            var root = new ZipFolderItem(null, null);
            var locationCache = new Dictionary<string, ZipFolderItem>();

            for (int i = 0; i < zipFile.Count; i++)
            {
                var zipEntry = zipFile[i];

                System.Diagnostics.Debug.WriteLine($"Processing {zipEntry.Name}...");

                string zipEntryName = zipEntry.Name.EndsWith('/') ? zipEntry.Name[..^1] : zipEntry.Name;

                int lastSlash = zipEntryName.LastIndexOf('/');

                string zipEntryFilename, zipEntryLocation;
                if (lastSlash == -1)
                {
                    zipEntryFilename = zipEntryName;
                    zipEntryLocation = string.Empty;
                }
                else
                {
                    zipEntryFilename = zipEntryName[(lastSlash + 1)..];
                    zipEntryLocation = zipEntryName[0..lastSlash];
                }

                if (!locationCache.TryGetValue(zipEntryLocation.ToLower(), out ZipFolderItem current))
                {
                    var pathParts = zipEntryLocation.Split('/');
                    current = root;

                    for (int j = 0; j < pathParts.Length; j++)
                    {
                        var item = current.Items.FirstOrDefault(it => it.Name.ToLower() == pathParts[j].ToLower());

                        if (item != null)
                        {
                            if (item is ZipFolderItem zipFolderItem)
                            {
                                current = zipFolderItem;
                            }
                            else
                                throw new InvalidOperationException("Cannot ensure path - there is a file, which conflicts with folder name!");
                        }
                        else
                        {
                            // TODO fill additional fields if possible
                            var zipFolderItem = new ZipFolderItem(current, pathParts[j]);
                            current.Items.Add(zipFolderItem);

                            current = zipFolderItem;
                        }
                    }
                }

                // We now should have proper list to add new item to.
                if (zipEntry.IsFile)
                {
                    var zipFileItem = new ZipFileItem(current, zipEntryFilename);
                    zipFileItem.Size = zipEntry.Size;
                    zipFileItem.Created = zipEntry.DateTime;
                    current.Items.Add(zipFileItem);
                }
                else
                {
                    var zipFolderItem = new ZipFolderItem(current, zipEntryFilename);
                    current.Items.Add(zipFolderItem);
                }
            }

            return root;
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

            // Build structure

            this.root = BuildCache(zipFile);
            this.current = root;

            // Locate inner folder if possible

            var parts = innerPath.Split('\\')
                .Where(part => !string.IsNullOrEmpty(part));

            current = root;
            foreach (var part in parts)
            {
                var next = current.Items.FirstOrDefault(i => i.Name.ToLowerInvariant() == part.ToLowerInvariant());
                if (next is not ZipFolderItem zipFolderItem)
                    return;

                current = zipFolderItem;
            }

            LoadItems();
        }

        public static bool SupportsAddress(string address)
        {
            return ZipAddressRegex.IsMatch(address);
        }

        public override IFilesystemOperator CreateOperatorForCurrentLocation()
        {
            // TODO
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public override void Refresh()
        {
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

        public override string Address => $"{ROOT_ADDRESS}{zipFilePath}>{current?.FullPath ?? ""}";

        public override IReadOnlyList<Item> Items => items;
    }
}

using File.Manager.API.Exceptions.Filesystem;
using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Focus;
using File.Manager.API.Filesystem.Models.Items.Listing;
using File.Manager.API.Types;
using File.Manager.Resources.Modules.Filesystem.Zip;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
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
            public ZipFileItem(string name)
                : base(name, true)
            {

            }
        }

        private sealed class ZipFolderItem : FolderItem
        {
            private readonly List<Item> items = new();

            public ZipFolderItem(string name)
                : base(name)
            {
                
            }

            public List<Item> Items => items;
        }

        // Private fields -----------------------------------------------------

        private string address;
        private ZipFile zipFile;
        private List<Item> items;
        private List<Item> currentLocation;

        // Private methods ----------------------------------------------------

        private (string zipPath, string internalPath) GetPaths(string address)
        {
            var match = ZipAddressRegex.Match(address);
            if (!match.Success)
                throw new InvalidOperationException("Invalid zip module address!");

            return (match.Groups[1].Value, match.Groups[2].Value);
        }

        private List<Item> BuildItems(ZipFile zipFile)
        {
            var result = new List<Item>();

            for (int i = 0; i < zipFile.Count; i++)
            {
                var zipEntry = zipFile[i];

                var pathParts = zipEntry.Name.Split('\\');

                var current = result;
                for (int j = 0; j < pathParts.Length - 1; i++)
                {
                    var item = current.FirstOrDefault(it => it.Name.ToLower() == pathParts[j].ToLower());

                    if (item != null)
                    {
                        if (item is ZipFolderItem zipFolderItem)
                            current = zipFolderItem.Items;
                        else
                            throw new InvalidOperationException("Cannot ensure path - there is a file, which conflicts with folder name!");
                    }
                    else
                    {
                        // TODO fill additional fields if possible
                        var zipFolderItem = new ZipFolderItem(pathParts[j]);
                        current.Add(zipFolderItem);
                        current = zipFolderItem.Items;
                    }
                }

                // We now should have proper list to add new item to.
                if (zipEntry.IsFile)
                {
                    var zipFileItem = new ZipFileItem(pathParts.Last());
                    current.Add(zipFileItem);
                }
                else
                {
                    var zipFolderItem = new ZipFolderItem(pathParts.Last());
                    current.Add(zipFolderItem);
                }
            }

            return result;
        }

        // Public methods -----------------------------------------------------

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

            throw new NotImplementedException();
        }

        public override LocationCapabilities GetLocationCapabilities()
        {
            throw new NotImplementedException();
        }

        public override void NavigateFromEntry(object data)
        {
            throw new InvalidOperationException("Navigation from root module entry is not supported for zip file!");
        }

        public override void NavigateToAddress(string address)
        {
            if (zipFile != null)
                throw new InvalidOperationException("Already navigated to an address.");

            (string zipPath, string innerPath) = GetPaths(address);

            // Load zip file

            try
            {
                var fs = new FileStream(zipPath, FileMode.Open, FileAccess.ReadWrite);
                zipFile = new ZipFile(fs);
            }
            catch (Exception e)
            {
                throw new NavigationException(Strings.Error_FailedToOpenZipFile);
            }

            // Build structure

            this.items = BuildItems(zipFile);

            // Locate inner folder if possible
        }

        public override void Refresh()
        {
            throw new NotImplementedException();
        }

        public override Item ResolveFocusedItem(FocusedItemData data)
        {
            throw new NotImplementedException();
        }

        public override string Address => throw new NotImplementedException();

        public override IReadOnlyList<Item> Items => throw new NotImplementedException();
    }
}

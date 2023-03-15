using File.Manager.API;
using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Navigation;
using File.Manager.API.Filesystem.Models.Focus;
using File.Manager.BusinessLogic.Modules.Filesystem.Home;
using File.Manager.BusinessLogic.Services.Icons;
using File.Manager.OsInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Diagnostics;
using File.Manager.API.Tools;
using File.Manager.API.Exceptions.Filesystem;
using File.Manager.API.Types;
using File.Manager.API.Filesystem.Models.Items.Listing;

namespace File.Manager.BusinessLogic.Modules.Filesystem.Local
{
    public class LocalNavigator : FilesystemNavigator
    {
        // Constants ----------------------------------------------------------

        public const string ROOT_ADDRESS = @"\\Local\";

        private static readonly Regex driveRootAddress = new(@"^[a-zA-Z]:\\*$");
        private static readonly Regex localAddressRegex = new(@"^[a-zA-Z]:\\.*$");

        // Private types ------------------------------------------------------

        private sealed class LocalDriveItem : FolderItem
        {
            public LocalDriveItem(string name, string filename)
                : base(name, false)
            {
                Filename = filename;
            }

            public string Filename { get; }
        }

        private sealed class LocalFolderItem : FolderItem
        {
            public LocalFolderItem(string name, string filename)
                : base(name)
            {
                Filename = filename;
            }

            public string Filename { get; }
        }

        private sealed class LocalFileItem : FileItem
        {
            public LocalFileItem(string name, string filename)
                : base(name)
            {
                Filename = filename;
            }

            public string Filename { get; }
        }

        // Private fields -----------------------------------------------------

        private List<Item> items;
        private string address;
        private int? rootEntryId;

        // Private methods ----------------------------------------------------

        private LocalNavigator()
        {
            this.rootEntryId = null;
            this.items = null;
            this.address = null;
        }

        private List<Item> TryLoadItems(string address)
        {
            var newItems = new List<Item>
            {
                new UpFolderItem()
                {
                    SizeDisplay = Resources.Modules.Filesystem.Local.Strings.SizeDisplay_UpDirectory
                }
            };

            if (address.ToLowerInvariant() == ROOT_ADDRESS.ToLowerInvariant())
            {
                // Drives

                var drives = DriveInfo.GetDrives();
                foreach (var drive in drives)
                {
                    string driveAddress = drive.RootDirectory.FullName;
                    (ImageSource smallIcon, ImageSource largeIcon) = OSServices.GetFileIcon(driveAddress);

                    string displayName;
                    if (drive.IsReady && !string.IsNullOrEmpty(drive.VolumeLabel))
                        displayName = $"{driveAddress} ({drive.VolumeLabel})";
                    else
                        displayName = driveAddress;

                    var driveItem = new LocalDriveItem(displayName, driveAddress)
                    {
                        SmallIcon = smallIcon,
                        LargeIcon = largeIcon,
                        SizeDisplay = Resources.Modules.Filesystem.Local.Strings.SizeDisplay_Drive
                    };

                    newItems.Add(driveItem);
                }
            }
            else
            {
                // Files

                var directoryInfo = new DirectoryInfo(address);

                foreach (var folder in directoryInfo.GetDirectories())
                {
                    var folderItem = new LocalFolderItem(folder.Name, folder.Name)
                    {
                        SizeDisplay = Resources.Modules.Filesystem.Local.Strings.SizeDisplay_Directory,
                        Created = folder.CreationTime,
                        Modified = folder.LastWriteTime,
                        Attributes = folder.Attributes
                    };
                    newItems.Add(folderItem);
                }

                foreach (var file in directoryInfo.GetFiles())
                {
                    var fileItem = new LocalFileItem(file.Name, file.Name)
                    {
                        SizeDisplay = SizeTools.BytesToHumanReadable(file.Length),
                        Size = file.Length,
                        Created = file.CreationTime,
                        Modified = file.LastWriteTime,
                        Attributes = file.Attributes
                    };
                    newItems.Add(fileItem);
                }                
            }

            return newItems;
        }

        private (bool result, string message) InternalOpenAddress(string newAddress)
        {
            if (newAddress != ROOT_ADDRESS && !Directory.Exists(newAddress))
                return (false, String.Format(Resources.Modules.Filesystem.Local.Strings.Error_PathDoesNotExist, newAddress));

            try
            {
                var newItems = TryLoadItems(newAddress);
                items = newItems;
                address = newAddress;
                OnAddressChanged();

                return (true, null);
            }
            catch (Exception e)
            {
                return (false, String.Format(Resources.Modules.Filesystem.Local.Strings.Error_FailedToNavigateToAddress, newAddress, e.Message));
            }
        }

        private void TryExecuteOpeningFolder(string newAddress, FocusedItemData data = null)
        {
            (bool result, string message) = InternalOpenAddress(newAddress);

            if (result)
                Handler?.NotifyChanged(data);
            else
                throw new ItemExecutionException(message);
        }

        private void TryNavigateToAddress(string newAddress)
        {
            (bool result, string message) = InternalOpenAddress(newAddress);

            if (result)
                Handler?.NotifyChanged(null);
            else
                throw new NavigationException(message);
        }        

        // Public methods -----------------------------------------------------

        public LocalNavigator(string address)
            : this()
        {           
            TryNavigateToAddress(address);
        }
        
        public LocalNavigator(RootModuleEntryData data)
            : this()
        {
            var localNavigationData = (LocalModuleEntryData)data;
            rootEntryId = localNavigationData.RootEntryId;
            TryNavigateToAddress(localNavigationData.Address);
        }

        public override void Dispose()
        {
            
        }

        public override void Execute(Item item)
        {
            if (item is LocalDriveItem driveItem)
            {
                TryExecuteOpeningFolder(driveItem.Filename);                
            }
            else if (item is LocalFolderItem folderItem)
            {
                TryExecuteOpeningFolder(System.IO.Path.Combine(address, folderItem.Filename));
            }
            else if (item is LocalFileItem fileItem)
            {
                // Try to execute item with explorer
                var path = System.IO.Path.Combine(address, fileItem.Filename);

                // Handle zip files separately
                if (System.IO.Path.GetExtension(fileItem.Filename).ToLowerInvariant() == ".zip")
                {
                    string address = $"{Zip.ZipNavigator.ROOT_ADDRESS}{path}>";
                    Handler.RequestNavigateToAddress(address, null);
                    return;
                }

                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo(path);
                    psi.UseShellExecute = true;
                    Process.Start(psi);                    
                }
                catch (Exception e)
                {
                    throw new ItemExecutionException(String.Format(Resources.Modules.Filesystem.Local.Strings.Error_CannotOpenFile, path, e.Message));
                }
            }
            else if (item is UpFolderItem)
            {
                if (address.ToLowerInvariant() == ROOT_ADDRESS.ToLowerInvariant())
                    Handler?.RequestReturnHome(new HomeFocusedItemData(LocalModule.ModuleUid, rootEntryId));
                else if (driveRootAddress.IsMatch(address))
                {
                    var data = new FilenameFocusedItemData(address);
                    TryExecuteOpeningFolder(ROOT_ADDRESS, data);
                }
                else
                {
                    string newAddress = Path.GetFullPath(Path.Combine(address, ".."));

                    string current = Path.GetFileName(Path.EndsInDirectorySeparator(address) ? address[..^1] : address);
                    var data = new FilenameFocusedItemData(current);

                    TryExecuteOpeningFolder(newAddress, data);
                }
            }
            else
            {
                throw new InvalidOperationException("Unsupported item type!");
            }
        }

        public override Item ResolveFocusedItem(FocusedItemData data)
        {
            if (data is not FilenameFocusedItemData localData)
                return null;

            var filename = localData.Filename.ToLowerInvariant();

            foreach (var item in items)
            {
                if (item is LocalFileItem fileItem && fileItem.Filename.ToLowerInvariant() == filename)
                    return item;
                else if (item is LocalFolderItem folderItem && folderItem.Filename.ToLowerInvariant() == filename)
                    return item;
                else if (item is LocalDriveItem driveItem && driveItem.Filename.ToLowerInvariant() == filename)
                    return item;                
            }

            return null;
        }

        public override void Refresh()
        {
            TryNavigateToAddress(address);
        }

        public override LocationCapabilities GetLocationCapabilities()
        {
            if (address.ToLowerInvariant() == ROOT_ADDRESS.ToLowerInvariant())
            {
                return (LocationCapabilities)0;
            }
            else
            {
                return LocationCapabilities.BufferedRead |
                    LocationCapabilities.BufferedWrite |
                    LocationCapabilities.CreateFolder |
                    LocationCapabilities.Delete | 
                    LocationCapabilities.Plan;
            }
        }

        public override IFilesystemOperator CreateOperatorForCurrentLocation()
        {
            if (address.ToLowerInvariant() == ROOT_ADDRESS.ToLowerInvariant())
                throw new InvalidOperationException("Cannot create operator for the root folder");
            
            return new LocalOperator(address);
        }

        public static bool SupportsAddress(string address)
        {
            if (address.ToLowerInvariant() == ROOT_ADDRESS.ToLowerInvariant())
                return true;

            if (localAddressRegex.IsMatch(address))
                return true;

            return false;            
        }

        // Public properties --------------------------------------------------

        public override IReadOnlyList<Item> Items => items;

        public override string Address => address;
    }
}

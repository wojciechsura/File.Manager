using File.Manager.API;
using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Execution;
using File.Manager.API.Filesystem.Models.Items;
using File.Manager.API.Filesystem.Models.Navigation;
using File.Manager.OsInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace File.Manager.BusinessLogic.Modules.Filesystem.Local
{
    public class LocalNavigator : FilesystemNavigator
    {
        // Private constants --------------------------------------------------

        private const string ROOT_ADDRESS = @"\\Local\";

        // Private types ------------------------------------------------------

        private sealed class LocalDriveItem : FolderItem
        {
            public LocalDriveItem(string name, string path)
                : base(name)
            {
                Path = path;
            }

            public string Path { get; }
        }

        private sealed class LocalFolderItem : FolderItem
        {
            public LocalFolderItem(string name, string path)
                : base(name)
            {
                Path = path;
            }

            public string Path { get; }
        }

        private sealed class LocalFileItem : FolderItem
        {
            public LocalFileItem(string name, string path)
                : base(name)
            {
                Path = path;
            }

            public string Path { get; }
        }

        // Private fields -----------------------------------------------------

        private List<Item> items;
        private string address;

        // Private methods ----------------------------------------------------

        private List<Item> TryLoadItems(string address)
        {
            var newItems = new List<Item>();

            if (address == ROOT_ADDRESS)
            {
                // Drives

                var drives = DriveInfo.GetDrives();
                foreach (var drive in drives)
                {
                    string driveAddress = drive.RootDirectory.FullName;
                    (ImageSource smallIcon, ImageSource largeIcon) = IconGenerator.GetFileIcon(driveAddress);
                    var driveItem = new LocalDriveItem(driveAddress, driveAddress)
                    {
                        SmallIcon = smallIcon,
                        LargeIcon = largeIcon
                    };

                    newItems.Add(driveItem);
                }
            }
            else
            {
                // Files

                throw new NotImplementedException();
            }

            return newItems;
        }

        private <TODO>

        private ExecutionOutcome TryExecuteOpeningFolder(string newAddress)
        {
            if (newAddress != ROOT_ADDRESS && !Directory.Exists(newAddress))
                return ExecutionOutcome.Error(String.Format(Resources.Modules.Filesystem.Local.Strings.Error_PathDoesNotExist, newAddress));

            try
            {
                var newItems = TryLoadItems(newAddress);
                items = newItems;
                address = newAddress;
                return ExecutionOutcome.NeedsRefresh();
            }
            catch (Exception e)
            {
                return ExecutionOutcome.Error(String.Format(Resources.Modules.Filesystem.Local.Strings.Error_FailedToNavigateToAddress, newAddress, e.Message));
            }
        }

        private NavigationOutcome TryNavigateToAddress(string newAddress)
        {

        }

        // Public methods -----------------------------------------------------

        public LocalNavigator()
        {
            
        }
        

        public override void Dispose()
        {
            
        }

        public override ExecutionOutcome Execute(Item item)
        {
            if (item is LocalDriveItem driveItem)
            {
                return TryExecuteOpeningFolder(driveItem.Path);                
            }
            if (item is LocalFolderItem folderItem)
            {
                return TryExecuteOpeningFolder(System.IO.Path.Combine(address, folderItem.Path));
            }
            else if (item is LocalFileItem fileItem)
            {
                throw new NotImplementedException();
            }
            else if (item is UpFolderItem upFolderItem)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new InvalidOperationException("Unsupported item type!");
            }
        }

        public override NavigationOutcome NavigateToRoot()
        {
            try
            {
                var newItems = TryLoadItems(ROOT_ADDRESS);
                items = newItems;
                address = ROOT_ADDRESS;
                return NavigationOutcome.NavigationSuccess();
            }
            catch (Exception e)
            {
                return NavigationOutcome.NavigationError(String.Format(File.Manager.Resources.Modules.Filesystem.Local.Strings.Error_FailedToObtainDriveList, e.Message));
            }
        }

        public override NavigationOutcome NavigateToAddress(string address)
        {
            throw new NotImplementedException();
        }

        // Public properties --------------------------------------------------

        public override IReadOnlyList<Item> Items => items;

        public override string Address => address;
    }
}

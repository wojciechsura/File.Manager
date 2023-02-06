using File.Manager.API;
using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Execution;
using File.Manager.API.Filesystem.Models.Items;
using File.Manager.API.Filesystem.Models.Navigation;
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

namespace File.Manager.BusinessLogic.Modules.Filesystem.Local
{
    public class LocalNavigator : FilesystemNavigator
    {
        // Private constants --------------------------------------------------

        private const string ROOT_ADDRESS = @"\\Local\";
        private readonly Regex driveRootAddress = new(@"^[a-zA-Z]:\\*$");

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

        private sealed class LocalFileItem : FileItem
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
            var newItems = new List<Item>
            {
                new UpFolderItem()
            };

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

                var folders = Directory.EnumerateDirectories(address)
                    .OrderBy(d => d)
                    .ToList();

                foreach (var folder in folders)
                {
                    var folderItem = new LocalFolderItem(Path.GetFileName(folder), Path.GetFileName(folder));
                    newItems.Add(folderItem);
                }

                var files = Directory.EnumerateFiles(address)
                    .OrderBy(d => d)
                    .ToList();

                foreach (var file in files)
                {
                    var fileItem = new LocalFileItem(Path.GetFileName(file), Path.GetFileName(file));
                    newItems.Add(fileItem);
                }
            }

            return newItems;
        }

        private (bool result, string message) InternalOpenAddress<T>(string newAddress)
        {
            if (newAddress != ROOT_ADDRESS && !Directory.Exists(newAddress))
                return (false, String.Format(Resources.Modules.Filesystem.Local.Strings.Error_PathDoesNotExist, newAddress));

            try
            {
                var newItems = TryLoadItems(newAddress);
                items = newItems;
                address = newAddress;
                return (true, null);
            }
            catch (Exception e)
            {
                return (false, String.Format(Resources.Modules.Filesystem.Local.Strings.Error_FailedToNavigateToAddress, newAddress, e.Message));
            }
        }

        private ExecutionOutcome TryExecuteOpeningFolder(string newAddress)
        {
            (bool result, string message) = InternalOpenAddress<ExecutionOutcome>(newAddress);
            return result ? ExecutionOutcome.NeedsRefresh() : ExecutionOutcome.Error(message);
        }

        private NavigationOutcome TryNavigateToAddress(string newAddress)
        {
            (bool result, string message) = InternalOpenAddress<NavigationOutcome>(newAddress);
            return result ? NavigationOutcome.NavigationSuccess() : NavigationOutcome.NavigationError(message);
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
                if (address == ROOT_ADDRESS)
                    return ExecutionOutcome.ReturnHome();
                else if (driveRootAddress.IsMatch(address))
                    return TryExecuteOpeningFolder(ROOT_ADDRESS);
                else
                {
                    string newAddress = Path.GetFullPath(Path.Combine(address, ".."));
                    return TryExecuteOpeningFolder(newAddress);
                }
            }
            else
            {
                throw new InvalidOperationException("Unsupported item type!");
            }
        }

        public override NavigationOutcome NavigateToRoot()
        {
            return TryNavigateToAddress(ROOT_ADDRESS);
        }

        public override NavigationOutcome NavigateToAddress(string address)
        {
            return TryNavigateToAddress(address);
        }

        // Public properties --------------------------------------------------

        public override IReadOnlyList<Item> Items => items;

        public override string Address => address;
    }
}

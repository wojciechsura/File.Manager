using File.Manager.API;
using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Execution;
using File.Manager.API.Filesystem.Models.Items;
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
            public LocalDriveItem(string name, string filename)
                : base(name)
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

        // Private methods ----------------------------------------------------

        private List<Item> TryLoadItems(string address)
        {
            var newItems = new List<Item>
            {
                new UpFolderItem()
                {
                    SizeDisplay = Resources.Modules.Filesystem.Local.Strings.SizeDisplay_UpDirectory
                }
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
                        Attributes = folder.Attributes.ToString()
                    };
                    newItems.Add(folderItem);
                }

                foreach (var file in directoryInfo.GetFiles())
                {
                    var fileItem = new LocalFileItem(file.Name, file.Name)
                    {
                        SizeDisplay = file.Length.ToString(),
                        Size = file.Length,
                        Created = file.CreationTime,
                        Modified = file.LastWriteTime,
                        Attributes = file.Attributes.ToString()
                    };
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

        private ExecutionOutcome TryExecuteOpeningFolder(string newAddress, FocusedItemData data = null)
        {
            (bool result, string message) = InternalOpenAddress<ExecutionOutcome>(newAddress);
            return result ? ExecutionOutcome.NeedsRefresh(data) : ExecutionOutcome.Error(message);
        }

        private NavigationOutcome TryNavigateToAddress(string newAddress = null)
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
                return TryExecuteOpeningFolder(driveItem.Filename);                
            }
            if (item is LocalFolderItem folderItem)
            {
                return TryExecuteOpeningFolder(System.IO.Path.Combine(address, folderItem.Filename));
            }
            else if (item is LocalFileItem fileItem)
            {
                // Try to execute item with explorer
                var path = System.IO.Path.Combine(address, fileItem.Filename);

                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo(path);
                    psi.UseShellExecute = true;
                    Process.Start(psi);

                    return ExecutionOutcome.Handled();
                }
                catch (Exception e)
                {
                    return ExecutionOutcome.Error(String.Format(Resources.Modules.Filesystem.Local.Strings.Error_CannotOpenFile, path, e.Message));
                }
            }
            else if (item is UpFolderItem)
            {
                if (address == ROOT_ADDRESS)
                    return ExecutionOutcome.ReturnHome(new HomeFocusedItemData(LocalModule.ModuleUid));
                else if (driveRootAddress.IsMatch(address))
                {
                    var data = new LocalFocusedItemData(address);
                    return TryExecuteOpeningFolder(ROOT_ADDRESS, data);
                }
                else
                {
                    string newAddress = Path.GetFullPath(Path.Combine(address, ".."));

                    string current = Path.GetFileName(Path.EndsInDirectorySeparator(address) ? address[..^1] : address);
                    var data = new LocalFocusedItemData(current);

                    return TryExecuteOpeningFolder(newAddress, data);
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

        public override Item ResolveFocusedItem(FocusedItemData data)
        {
            if (data is not LocalFocusedItemData localData)
                return null;

            var filename = localData.Filename.ToLowerInvariant();

            foreach (var item in items)
            {
                if (item is LocalFileItem fileItem && fileItem.Filename.ToLower() == filename)
                    return item;
                else if (item is LocalFolderItem folderItem && folderItem.Filename.ToLower() == filename)
                    return item;
                else if (item is LocalDriveItem driveItem && driveItem.Filename.ToLower() == filename)
                    return item;                
            }

            return null;
        }

        // Public properties --------------------------------------------------

        public override IReadOnlyList<Item> Items => items;

        public override string Address => address;
    }
}

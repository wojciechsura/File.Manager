using File.Manager.API.Exceptions.Filesystem;
using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Focus;
using File.Manager.API.Filesystem.Models.Items.Listing;
using File.Manager.API.Tools;
using File.Manager.API.Types;
using File.Manager.BusinessLogic.Services.Configuration;
using File.Manager.BusinessLogic.Services.Dialogs;
using File.Manager.BusinessLogic.Services.Messaging;
using File.Manager.Common.Helpers;
using File.Manager.Resources.Modules.Filesystem.Ftp;
using FluentFTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace File.Manager.BusinessLogic.Modules.Filesystem.Ftp
{
    public class FtpNavigator : FilesystemNavigator
    {
        private class FtpFolderItem : FolderItem
        {
            public FtpFolderItem(string name, bool isSelectable = true) 
                : base(name, isSelectable)
            {

            }
        }

        private class FtpFileItem : FileItem
        {
            public FtpFileItem(string name, bool isSelectable = true) 
                : base(name, isSelectable)
            {

            }
        }

        private readonly FtpClient client;
        private readonly string sessionName;
        private readonly (string host, string username) sessionKey;
        private readonly IActiveFtpSessions activeFtpSessions;
        private readonly IConfigurationService configurationService;
        private readonly IDialogService dialogService;
        private readonly IMessagingService messagingService;
        private readonly string rootDirectory;
        private string workingDirectory;
        private List<Item> items;

        private void LoadItems(FtpFocusedItemData focusedItem = null)
        {
            items = new List<Item>();

            client.SetWorkingDirectory(workingDirectory);
            var listing = client.GetListing();

            var upDir = new UpFolderItem()
            {
                SizeDisplay = Strings.SizeDisplay_UpDirectory
            };
            items.Add(upDir);

            // Directories
            foreach (var entry in listing
                .Where(e => e.Type == FtpObjectType.Directory && e.SubType == FtpObjectSubType.SubDirectory)
                .OrderBy(e => e.Name))
            {
                var item = new FtpFolderItem(entry.Name, true);
                item.SizeDisplay = Strings.SizeDisplay_Directory;
                items.Add(item);
            }

            // Files
            foreach (var entry in listing.Where(e => e.Type == FtpObjectType.File || e.Type == FtpObjectType.Link)
                .OrderBy(e => e.Name))
            {
                var item = new FtpFileItem(entry.Name, true)
                {
                    Size = entry.Size,
                    SizeDisplay = SizeTools.BytesToHumanReadable(entry.Size),
                    Created = entry.Created,
                    Modified = entry.Modified
                };
                items.Add(item);
            }

            Handler?.NotifyChanged(focusedItem);
            OnAddressChanged();
        }

        public FtpNavigator(FtpClient client, 
            string sessionName,
            (string host, string username) sessionKey, 
            IActiveFtpSessions activeFtpSessions,
            IConfigurationService configurationService,
            IDialogService dialogService,
            IMessagingService messagingService)
        {
            this.client = client;
            this.sessionName = sessionName;
            this.sessionKey = sessionKey;
            this.activeFtpSessions = activeFtpSessions;
            this.configurationService = configurationService;
            this.dialogService = dialogService;
            this.messagingService = messagingService;

            rootDirectory = workingDirectory = PathHelper.EnsureTrailingSlash(client.GetWorkingDirectory());

            LoadItems();
        }

        public override bool CanCustomEdit(Item item) => false;        

        public override FilesystemOperator CreateOperatorForCurrentLocation()
        {
            return new FtpOperator(client, workingDirectory, sessionKey);
        }

        public override void CustomEdit(Item item) =>
            throw new NotSupportedException();
        
        public override void Dispose()
        {
            client.Dispose();
            activeFtpSessions.ActiveSessions.Remove(sessionKey);
        }

        public override void Execute(Item item)
        {
            if (item is UpFolderItem)
            {
                // FTP is case sensitive
                if (workingDirectory == rootDirectory)
                {
                    // Exit
                    var navigator = new FtpSessionsNavigator(configurationService, dialogService, messagingService, activeFtpSessions);
                    Handler?.RequestReplaceNavigator(navigator, new FtpSessionFocusedItemData(sessionName));
                }
                else
                {
                    if (!workingDirectory.StartsWith(rootDirectory))
                        throw new InvalidOperationException("Working directory does not start with root directory!");
                    if (workingDirectory.Length == 0)
                        throw new InvalidOperationException("Invalid working directory!");

                    // Strip the root part from working directory
                    var subfolderPart = workingDirectory[(rootDirectory.Length)..(workingDirectory.Length - 1)];

                    // Split into separate folders
                    var pathParts = subfolderPart.Split("/");

                    // Go one level up
                    var oldDirectory = pathParts[pathParts.Length - 1];
                    var newPathParts = pathParts.Take(pathParts.Length - 1).ToArray();

                    // New working directory
                    var newWorkingDirectory = PathHelper.EnsureTrailingSlash($"{rootDirectory}{string.Join('/', newPathParts)}");

                    try
                    {
                        client.SetWorkingDirectory(workingDirectory);
                        workingDirectory = newWorkingDirectory;
                        LoadItems(new FtpFocusedItemData(oldDirectory));
                    }
                    catch (Exception e)
                    {
                        throw new NavigationException(string.Format(Strings.Error_FailedToNavigateToDirectory, newWorkingDirectory, e.Message));
                    }
                }
            }
            else if (item is FtpFolderItem ftpFolder)
            {
                var newWorkingDirectry = $"{PathHelper.EnsureTrailingSlash(workingDirectory)}{ftpFolder.Name}/";

                try
                {
                    client.SetWorkingDirectory(workingDirectory);
                    workingDirectory = newWorkingDirectry;
                }
                catch (Exception e)
                {
                    throw new NavigationException(string.Format(Strings.Error_FailedToNavigateToDirectory, newWorkingDirectry, e.Message));
                }

                LoadItems();
            }
            else if (item is FtpFileItem)
            {
                // Ignore
            }
            else
                throw new InvalidOperationException("Unsupported item!");
        }

        public override LocationCapabilities GetLocationCapabilities()
        {
            return LocationCapabilities.BufferedRead |
                LocationCapabilities.BufferedWrite |
                LocationCapabilities.CreateFolder |
                LocationCapabilities.Delete;
        }

        public override void Refresh()
        {
            LoadItems();
        }

        public override Item ResolveFocusedItem(FocusedItemData data)
        {
            if (data is FtpFocusedItemData ftpFocusedItemData)
            {
                // FTP is case-sensitive
                return items.FirstOrDefault(i => i.Name == ftpFocusedItemData.Name);
            }
            else
                throw new InvalidOperationException("Unsupported focused item data!");
        }

        public override string Address => $"{FtpSessionsNavigator.ROOT_ADDRESS}{sessionKey.username}@{sessionKey.host}>{workingDirectory}";

        public override IReadOnlyList<Item> Items => items;
        
        public override bool RestoreLocationAfterRestart => false;
    }
}

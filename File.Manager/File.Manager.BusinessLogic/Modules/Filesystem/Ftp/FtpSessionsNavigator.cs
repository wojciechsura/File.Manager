using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Focus;
using File.Manager.API.Filesystem.Models.Items.Listing;
using File.Manager.API.Types;
using File.Manager.BusinessLogic.Models.Configuration.Ftp;
using File.Manager.BusinessLogic.Modules.Filesystem.Home;
using File.Manager.BusinessLogic.Services.Configuration;
using File.Manager.BusinessLogic.Services.Dialogs;
using File.Manager.Common.Helpers;
using File.Manager.Resources.Modules.Filesystem.Ftp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace File.Manager.BusinessLogic.Modules.Filesystem.Ftp
{
    public class FtpSessionsNavigator : FilesystemNavigator
    {
        // Public constants ---------------------------------------------------

        public static string ROOT_ADDRESS = @"\\Ftp\";

        // Private types ------------------------------------------------------

        private class SessionItem : FileItem
        {
            public SessionItem(FtpSession session)
                : base(session.SessionName.Value, true)
            {
                
            }
        }

        private class AddSessionItem : FileItem
        {
            public AddSessionItem()
                : base(Strings.Name_NewSession)
            {

            }
        }

        // Private fields -----------------------------------------------------

        private readonly ImageSource ftpSessionSmallIcon;
        private readonly ImageSource ftpSessionLargeIcon;
        private readonly ImageSource addFtpSessionSmallIcon;
        private readonly ImageSource addFtpSessionLargeIcon;

        private readonly IConfigurationService configurationService;
        private readonly IDialogService dialogService;
        private List<Item> items;

        // Private methods ----------------------------------------------------

        private void LoadItems()
        {
            items = new List<Item>();

            items.Add(new UpFolderItem());

            foreach (var session in configurationService.Configuration.Ftp.Sessions)
            {
                var item = new SessionItem(session)
                {
                    SmallIcon = ftpSessionSmallIcon,
                    LargeIcon = ftpSessionLargeIcon
                };

                items.Add(item);
            }

            var addSessionItem = new AddSessionItem()
            {
                SmallIcon = addFtpSessionSmallIcon,
                LargeIcon = addFtpSessionLargeIcon
            };

            items.Add(addSessionItem);
        }

        private void DoAddSession()
        {
            (bool result, FtpSession model) = dialogService.ShowFtpSessionEditorDialog(null);
            if (result)
            {
                LoadItems();
                Handler.NotifyChanged(new FtpSessionFocusedItemData(model.SessionName.Value));
            }
        }

        // Public methods -----------------------------------------------------

        public FtpSessionsNavigator(IConfigurationService configurationService, IDialogService dialogService)
        {
            this.configurationService = configurationService;
            this.dialogService = dialogService;
            var assembly = Assembly.GetExecutingAssembly();

            ftpSessionSmallIcon = ResourceHelper.FromEmbeddedResource(assembly, @"File.Manager.BusinessLogic.Resources.Images.Ftp.FtpSession16.png");
            ftpSessionLargeIcon = ResourceHelper.FromEmbeddedResource(assembly, @"File.Manager.BusinessLogic.Resources.Images.Ftp.FtpSession32.png");
            addFtpSessionSmallIcon = ResourceHelper.FromEmbeddedResource(assembly, @"File.Manager.BusinessLogic.Resources.Images.Ftp.AddSession16.png");
            addFtpSessionLargeIcon = ResourceHelper.FromEmbeddedResource(assembly, @"File.Manager.BusinessLogic.Resources.Images.Ftp.AddSession32.png");

            LoadItems();
        }

        public override FilesystemOperator CreateOperatorForCurrentLocation()
            => throw new NotSupportedException("Creating operator for FTP sessions navigator is not supported.");

        public override void Dispose()
        {
            
        }

        public override void Execute(Item item)
        {
            if (item is UpFolderItem)
            {
                Handler.RequestReturnHome(new HomeFocusedItemData(FtpModule.ModuleUid, 0));
            }
            else if (item is AddSessionItem)
            {
                DoAddSession();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override LocationCapabilities GetLocationCapabilities()
        {
            throw new NotImplementedException();
        }

        public override void Refresh()
        {
            LoadItems();
            Handler?.NotifyChanged(null);
        }

        public override Item ResolveFocusedItem(FocusedItemData data)
        {
            var ftpSessionData = (FtpSessionFocusedItemData)data;

            return items
                .OfType<SessionItem>()
                .FirstOrDefault(i => i.Name.ToLowerInvariant() == ftpSessionData.SessionName.ToLowerInvariant());
        }

        public static bool SupportsAddress(string address)
        {
            if (address.ToLowerInvariant() == ROOT_ADDRESS.ToLowerInvariant())
                return true;

            return false;
        }


        public override string Address => ROOT_ADDRESS;

        public override IReadOnlyList<Item> Items => items;

        public override bool RestoreAddress => true;
    }
}

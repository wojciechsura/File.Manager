using File.Manager.API.Exceptions.Filesystem;
using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Focus;
using File.Manager.API.Filesystem.Models.Items.Listing;
using File.Manager.API.Types;
using File.Manager.BusinessLogic.Models.Configuration.Ftp;
using File.Manager.BusinessLogic.Models.Dialogs.FtpCredentials;
using File.Manager.BusinessLogic.Modules.Filesystem.Home;
using File.Manager.BusinessLogic.Services.Configuration;
using File.Manager.BusinessLogic.Services.Dialogs;
using File.Manager.BusinessLogic.Services.Messaging;
using File.Manager.Common.Helpers;
using File.Manager.Resources.Modules.Filesystem.Ftp;
using SmartFormat.Utilities;
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
                this.Session = session;
            }

            public FtpSession Session { get; }
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
        private readonly IMessagingService messagingService;
        private readonly IActiveFtpSessions activeFtpSessions;

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
                    LargeIcon = ftpSessionLargeIcon,
                    SizeDisplay = Strings.SizeDisplay_Session
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

        private void DoStartSession(FtpSession session)
        {
            (bool result, FtpCredentialsModel credentials) = dialogService.ShowFtpCredentialsDialog(session.Username.Value);
            if (result)
            {
                // Check, if there is no existing session for this host and user name
                if (activeFtpSessions.ActiveSessions.ContainsKey((session.Host.Value.ToLowerInvariant(), credentials.Username.ToLowerInvariant())))
                {
                    messagingService.Inform(Strings.Information_SessionAlreadyExists);
                    return;
                }

                // Try to open FTP session

                try
                {
                    FluentFTP.FtpClient client = new FluentFTP.FtpClient();
                    client.Host = session.Host.Value;
                    client.Port = session.Port.Value;
                    client.Credentials = new System.Net.NetworkCredential(credentials.Username, credentials.Password);

                    var profile = client.AutoConnect();
                    if (profile == null)
                        throw new NavigationException(String.Format(Strings.Error_FailedToOpenFtpSession, Strings.Reason_FailedToConnect));

                    var navigator = new FtpNavigator(client, 
                        session.SessionName.Value, 
                        (session.Host.Value, credentials.Username), 
                        activeFtpSessions,
                        configurationService,
                        dialogService,
                        messagingService);

                    Handler.RequestReplaceNavigator(navigator, null);
                }
                catch (NavigationException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new NavigationException(String.Format(Strings.Error_FailedToOpenFtpSession, e.Message));
                }
            }
        }


        // Public methods -----------------------------------------------------

        public FtpSessionsNavigator(IConfigurationService configurationService, 
            IDialogService dialogService,
            IMessagingService messagingService,
            IActiveFtpSessions activeFtpSessions)
        {
            this.configurationService = configurationService;
            this.dialogService = dialogService;
            this.messagingService = messagingService;
            this.activeFtpSessions = activeFtpSessions;
            var assembly = Assembly.GetExecutingAssembly();

            ftpSessionSmallIcon = ResourceHelper.FromEmbeddedResource(assembly, @"File.Manager.BusinessLogic.Resources.Images.Ftp.FtpSession16.png");
            ftpSessionLargeIcon = ResourceHelper.FromEmbeddedResource(assembly, @"File.Manager.BusinessLogic.Resources.Images.Ftp.FtpSession32.png");
            addFtpSessionSmallIcon = ResourceHelper.FromEmbeddedResource(assembly, @"File.Manager.BusinessLogic.Resources.Images.Ftp.AddSession16.png");
            addFtpSessionLargeIcon = ResourceHelper.FromEmbeddedResource(assembly, @"File.Manager.BusinessLogic.Resources.Images.Ftp.AddSession32.png");

            LoadItems();
        }

        public override bool CanCustomEdit(Item item)
        {
            return item is SessionItem;
        }

        public override void CustomEdit(Item item)
        {
            var session = (SessionItem)item;

            var ftpSession = configurationService.Configuration.Ftp.Sessions.FirstOrDefault(s => s.SessionName.Value.ToLowerInvariant() == session.Name.ToLowerInvariant());
            if (ftpSession != null)
            {
                (bool result, ftpSession) = dialogService.ShowFtpSessionEditorDialog(ftpSession);
                if (result)
                {
                    LoadItems();
                    Handler.NotifyChanged(new FtpSessionFocusedItemData(ftpSession.SessionName.Value));
                }
            }
        }

        public override FilesystemOperator CreateOperatorForCurrentLocation()
            => new FtpSessionOperator(configurationService);

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
            else if (item is SessionItem sessionItem)
            {
                DoStartSession(sessionItem.Session);
            }
            else
                throw new InvalidOperationException("Unsupported item!");
        }

        public override LocationCapabilities GetLocationCapabilities()
        {
            return LocationCapabilities.CustomEdit | LocationCapabilities.Delete;
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

        public override bool RestoreLocationAfterRestart => true;
    }
}

using File.Manager.API;
using File.Manager.API.Filesystem;
using File.Manager.BusinessLogic.Services.Configuration;
using File.Manager.BusinessLogic.Services.Dialogs;
using File.Manager.BusinessLogic.Services.Messaging;
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
    public class FtpModule : FilesystemModule, IActiveFtpSessions
    {
        public static readonly string ModuleUid = "FTP";

        private readonly IConfigurationService configurationService;
        private readonly IMessagingService messagingService;
        private readonly IDialogService dialogService;

        private readonly ImageSource ftpSmallIcon;
        private readonly ImageSource ftpLargeIcon;

        private readonly Dictionary<(string host, string username), ActiveFtpSession> activeSessions = new();

        Dictionary<(string host, string username), ActiveFtpSession> IActiveFtpSessions.ActiveSessions => activeSessions;

        public FtpModule(IModuleHost host, 
            IConfigurationService configurationService, 
            IMessagingService messagingService,
            IDialogService dialogService) 
            : base(host)
        {
            this.configurationService = configurationService;
            this.messagingService = messagingService;
            this.dialogService = dialogService;

            var assembly = Assembly.GetExecutingAssembly();

            ftpSmallIcon = ResourceHelper.FromEmbeddedResource(assembly, @"File.Manager.BusinessLogic.Resources.Images.Ftp.FtpSession16.png");
            ftpLargeIcon = ResourceHelper.FromEmbeddedResource(assembly, @"File.Manager.BusinessLogic.Resources.Images.Ftp.FtpSession32.png");
        }

        public override FilesystemNavigator CreateNavigator(string address)
        {
            if (address == FtpSessionsNavigator.ROOT_ADDRESS)
            {
                return new FtpSessionsNavigator(configurationService, dialogService, messagingService, this);
            }

            // TODO?
            throw new NotImplementedException();
        }

        public override FilesystemNavigator CreateNavigator(RootModuleEntryData data)
        {
            return new FtpSessionsNavigator(configurationService, dialogService, messagingService, this);
        }

        public override IEnumerable<RootModuleEntry> GetRootEntries()
        {
            yield return new RootModuleEntry(0,
                Strings.FtpDisplayName,
                ftpSmallIcon,
                ftpLargeIcon,
                null);
        }

        public override bool SupportsAddress(string address)
        {
            if (address == FtpSessionsNavigator.ROOT_ADDRESS)
                return true;

            return false;
        }

        public override string Uid => ModuleUid;
    }
}

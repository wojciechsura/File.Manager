using File.Manager.API;
using File.Manager.API.Filesystem;
using File.Manager.OsInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace File.Manager.BusinessLogic.Modules.Filesystem.Local
{
    internal class LocalModule : FilesystemModule
    {
        private readonly ImageSource computerLargeIcon;
        private readonly ImageSource computerSmallIcon;
        private readonly ImageSource desktopLargeIcon;
        private readonly ImageSource desktopSmallIcon;
        private readonly ImageSource documentsLargeIcon;
        private readonly ImageSource documentsSmallIcon;
        public static readonly string ModuleUid = "LOCAL";
        public LocalModule(IModuleHost host)
            : base(host)
        {
            (computerSmallIcon, computerLargeIcon) = OSServices.GetMyComputerIcon();
            (desktopSmallIcon, desktopLargeIcon) = OSServices.GetDesktopIcon();
            (documentsSmallIcon, documentsLargeIcon) = OSServices.GetDocumentsIcon();
        }

        public override FilesystemNavigator CreateNavigator()
        {
            return new LocalNavigator();
        }

        public override IEnumerable<RootModuleEntry> GetRootEntries()
        {
            yield return new RootModuleEntry(0,
                Resources.Modules.Filesystem.Local.Strings.ComputerDisplayName,
                computerSmallIcon,
                computerLargeIcon,
                new LocalNavigationData(LocalNavigator.ROOT_ADDRESS, 0));

            yield return new RootModuleEntry(1,
                Resources.Modules.Filesystem.Local.Strings.DesktopDisplayName,
                desktopSmallIcon,
                desktopLargeIcon,
                new LocalNavigationData(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), 1));

            yield return new RootModuleEntry(2,
                Resources.Modules.Filesystem.Local.Strings.DocumentsDisplayName,
                documentsSmallIcon,
                documentsLargeIcon,
                new LocalNavigationData(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 2));
        }

        public override bool SupportsAddress(string address)
        {
            return LocalNavigator.SupportsAddress(address);
        }
        public override string Uid => ModuleUid;
    }
}

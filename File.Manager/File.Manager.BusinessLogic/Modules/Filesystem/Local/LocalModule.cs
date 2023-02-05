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
        private readonly ImageSource smallIcon;
        private readonly ImageSource largeIcon;

        public LocalModule(IModuleHost host)
            : base(host)
        {
            (smallIcon, largeIcon) = IconGenerator.GetMyComputerIcon();
        }

        public override bool SupportsAddress(string address)
        {
            // TODO
            throw new NotImplementedException();
        }

        public override FilesystemNavigator CreateNavigator()
        {
            return new LocalNavigator();
        }

        public override ImageSource SmallIcon => smallIcon;
        public override ImageSource LargeIcon => largeIcon;
        public override string DisplayName => Resources.Modules.Filesystem.Local.Strings.DisplayName;
        public override string Uid => "LOCAL";
    }
}

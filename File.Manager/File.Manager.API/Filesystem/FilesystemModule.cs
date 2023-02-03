using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace File.Manager.API.Filesystem
{
    public abstract class FilesystemModule
    {
        protected readonly IModuleHost host;

        public FilesystemModule(IModuleHost host)
        {
            this.host = host;
        }

        public abstract bool SupportsAddress(string address);
        public abstract FilesystemNavigator OpenRoot();
        public abstract FilesystemNavigator OpenAddress(string address);

        public abstract ImageSource SmallIcon { get; }
        public abstract ImageSource LargeIcon { get; }
        public abstract string DisplayName { get; }
        public abstract string Uid { get; }
    }
}

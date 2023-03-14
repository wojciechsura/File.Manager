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

        protected FilesystemModule(IModuleHost host)
        {
            this.host = host;
        }

        public abstract FilesystemNavigator CreateNavigator();

        public abstract IEnumerable<RootModuleEntry> GetRootEntries();

        public abstract bool SupportsAddress(string address);

        public abstract string Uid { get; }
    }
}

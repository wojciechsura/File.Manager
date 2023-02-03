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
        public abstract bool SupportsAddress(string address);
        public abstract FilesystemNavigator OpenRoot();
        public abstract FilesystemNavigator OpenAddress(string address);

        public abstract ImageSource SmallIcon { get; set; }
        public abstract ImageSource LargeIcon { get; set; }
        public abstract string DisplayName { get; set; }
        public abstract string Uid { get; set; }
    }
}

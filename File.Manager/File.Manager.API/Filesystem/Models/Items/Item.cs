using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace File.Manager.API.Filesystem.Models.Items
{
    /// <summary>
    /// Base class for items provided to File.Manager by the Navigator
    /// </summary>
    public class Item
    {
        private protected Item(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public ImageSource SmallIcon { get; set; }
        public ImageSource LargeIcon { get; set; }
    }
}

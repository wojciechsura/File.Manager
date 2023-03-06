using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace File.Manager.API.Filesystem.Models.Items.Listing
{
    /// <summary>
    /// Represents a folder-like item (one, which when
    /// executed, should result in location change)
    /// </summary>
    public class FolderItem : Item
    {
        public FolderItem(string name)
            : base(name)
        {

        }

        public FolderItem(string name, ImageSource smallIcon, ImageSource largeIcon)
            : this(name)
        {
            SmallIcon = smallIcon;
            LargeIcon = largeIcon;
        }
    }
}

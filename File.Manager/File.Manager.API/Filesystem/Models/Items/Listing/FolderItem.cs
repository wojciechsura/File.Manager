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
        public FolderItem(string name, bool isSelectable = true)
            : base(name, isSelectable)
        {

        }

        public FolderItem(string name, ImageSource smallIcon, ImageSource largeIcon, bool isSelectable = true)
            : this(name, isSelectable)
        {
            SmallIcon = smallIcon;
            LargeIcon = largeIcon;
        }
    }
}

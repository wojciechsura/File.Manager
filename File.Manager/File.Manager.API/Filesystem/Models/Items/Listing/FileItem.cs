using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace File.Manager.API.Filesystem.Models.Items.Listing
{
    /// <summary>
    /// Represents a file-like item (executable, copyable, viewable,
    /// editable etc.)
    /// </summary>
    public class FileItem : Item
    {
        public FileItem(string name, bool isSelectable = true)
            : base(name, isSelectable)
        {

        }

        public FileItem(string name, ImageSource smallIcon, ImageSource largeIcon, bool isSelectable = true)
            : this(name, isSelectable)
        {
            SmallIcon = smallIcon;
            LargeIcon = largeIcon;
        }
    }
}

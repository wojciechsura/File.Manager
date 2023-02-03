using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace File.Manager.API.Filesystem.Models.Items
{
    /// <summary>
    /// Represents a file-like item (executable, copyable, viewable,
    /// editable etc.)
    /// </summary>
    public class FileItem : Item
    {
        private protected FileItem(string name, 
            ImageSource smallIcon, 
            ImageSource largeIcon) 
            : base(name, smallIcon, largeIcon)
        {

        }
    }
}

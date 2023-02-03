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
        public FileItem(string name) 
            : base(name)
        {

        }
    }
}

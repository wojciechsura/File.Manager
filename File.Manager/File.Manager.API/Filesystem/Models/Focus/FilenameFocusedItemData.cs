using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.API.Filesystem.Models.Focus
{
    public sealed class FilenameFocusedItemData : FocusedItemData
    {
        public FilenameFocusedItemData(string filename)
        {
            Filename = filename;
        }

        public string Filename { get; }
    }
}

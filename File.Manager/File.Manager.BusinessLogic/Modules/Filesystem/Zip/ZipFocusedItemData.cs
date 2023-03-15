using File.Manager.API.Filesystem.Models.Focus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Modules.Filesystem.Zip
{
    public class ZipFocusedItemData : FocusedItemData
    {
        public ZipFocusedItemData(string name)
        {
            Filename = name;
        }

        public string Filename { get; }
    }
}

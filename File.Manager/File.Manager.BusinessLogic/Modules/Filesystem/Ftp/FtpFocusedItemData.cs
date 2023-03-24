using File.Manager.API.Filesystem.Models.Focus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Modules.Filesystem.Ftp
{
    public class FtpFocusedItemData : FocusedItemData
    {
        public FtpFocusedItemData(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}

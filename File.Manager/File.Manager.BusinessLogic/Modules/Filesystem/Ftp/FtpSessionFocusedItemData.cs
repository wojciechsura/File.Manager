using File.Manager.API.Filesystem.Models.Focus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Modules.Filesystem.Ftp
{
    public class FtpSessionFocusedItemData : FocusedItemData
    {
        public FtpSessionFocusedItemData(string sessionName) 
        {
            SessionName = sessionName;
        }

        public string SessionName { get; }
    }
}

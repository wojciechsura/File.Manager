using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Focus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Modules.Filesystem.Home
{
    class HomeFocusedItemData : FocusedItemData
    {
        public HomeFocusedItemData(string moduleUid)
        {
            ModuleUid = moduleUid;
        }

        public string ModuleUid { get; }
    }
}

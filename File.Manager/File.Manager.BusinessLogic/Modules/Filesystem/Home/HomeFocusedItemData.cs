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
        public HomeFocusedItemData(string? moduleUid, int? rootEntryId)
        {
            ModuleUid = moduleUid;
            RootEntryId = rootEntryId;
        }

        /// <summary>UID of the module, which requests returning to the root location</summary>
        public string? ModuleUid { get; }
        
        /// <summary>Id of module's RootModuleEntry item, which should get focused</summary>
        public int? RootEntryId { get; }
    }
}

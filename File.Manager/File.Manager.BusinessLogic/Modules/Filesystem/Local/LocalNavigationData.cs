using File.Manager.API.Filesystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Modules.Filesystem.Local
{
    public class LocalModuleEntryData : RootModuleEntryData
    {
        public LocalModuleEntryData(string address, int rootEntryId)
        {
            Address = address;
            RootEntryId = rootEntryId;
        }

        public string Address { get; }
        public int RootEntryId { get; }
    }
}

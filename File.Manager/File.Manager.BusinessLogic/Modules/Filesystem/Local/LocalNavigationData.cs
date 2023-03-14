using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Modules.Filesystem.Local
{
    public class LocalNavigationData
    {
        public LocalNavigationData(string? address, int rootEntryId)
        {
            Address = address;
            RootEntryId = rootEntryId;
        }

        public string Address { get; }
        public int RootEntryId { get; }
    }
}

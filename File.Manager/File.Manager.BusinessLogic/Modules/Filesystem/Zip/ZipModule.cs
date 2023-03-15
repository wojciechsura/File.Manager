using File.Manager.API;
using File.Manager.API.Filesystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Modules.Filesystem.Zip
{
    public class ZipModule : FilesystemModule
    {
        public static readonly string ModuleUid = "ZIP";        

        public ZipModule(IModuleHost host)
            : base(host)
        {

        }

        public override FilesystemNavigator CreateNavigator(string address)
        {
            return new ZipNavigator(address);
        }

        public override FilesystemNavigator CreateNavigator(RootModuleEntryData data)
        {
            throw new NotSupportedException("Zip module does not support root-level entries");
        }

        public override IEnumerable<RootModuleEntry> GetRootEntries()
        {
            yield break;
        }

        public override bool SupportsAddress(string address)
        {
            return ZipNavigator.SupportsAddress(address);
        }

        public override string Uid => ModuleUid;
    }
}

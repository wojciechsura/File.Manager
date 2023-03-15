using File.Manager.API;
using File.Manager.API.Filesystem;
using File.Manager.BusinessLogic.Modules.Filesystem.Local;
using File.Manager.BusinessLogic.Modules.Filesystem.Zip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Services.Modules
{
    internal class ModuleService : IModuleService
    {
        private readonly List<FilesystemModule> filesystemModules;
        private readonly IModuleHost moduleHost;

        public ModuleService(IModuleHost moduleHost)
        {
            filesystemModules = new List<FilesystemModule>
            {
                new LocalModule(moduleHost),
                new ZipModule(moduleHost)
            };

            this.moduleHost = moduleHost;
        }

        public IReadOnlyList<FilesystemModule> FilesystemModules => filesystemModules;
    }
}

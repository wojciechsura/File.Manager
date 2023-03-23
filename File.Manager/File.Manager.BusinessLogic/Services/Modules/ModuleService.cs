using File.Manager.API;
using File.Manager.API.Filesystem;
using File.Manager.BusinessLogic.Modules.Filesystem.Ftp;
using File.Manager.BusinessLogic.Modules.Filesystem.Local;
using File.Manager.BusinessLogic.Modules.Filesystem.Zip;
using File.Manager.BusinessLogic.Services.Configuration;
using File.Manager.BusinessLogic.Services.Dialogs;
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
        private readonly IConfigurationService configurationService;
        private readonly IDialogService dialogService;

        public ModuleService(IModuleHost moduleHost, IConfigurationService configurationService, IDialogService dialogService)
        {
            filesystemModules = new List<FilesystemModule>
            {
                new LocalModule(moduleHost),
                new ZipModule(moduleHost),
                new FtpModule(moduleHost, configurationService, dialogService)
            };

            this.moduleHost = moduleHost;
            this.configurationService = configurationService;
            this.dialogService = dialogService;
        }

        public IReadOnlyList<FilesystemModule> FilesystemModules => filesystemModules;
    }
}

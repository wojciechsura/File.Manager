using File.Manager.API.Filesystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Services.Modules
{
    public interface IModuleService
    {
        IReadOnlyList<FilesystemModule> FilesystemModules { get; }
    }
}

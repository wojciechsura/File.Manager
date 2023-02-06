using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Modules.Filesystem.Home
{
    public class HomeSelectionMemento : SelectionMemento
    {
        public HomeSelectionMemento(FilesystemModule module)
        {
            Module = module;
        }

        public FilesystemModule Module { get; }
    }
}

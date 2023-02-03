using File.Manager.API.Filesystem;
using File.Manager.BusinessLogic.Services.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.ViewModels.Pane
{
    public class PaneViewModel
    {
        private readonly IPaneHandler handler;
        
        private FilesystemNavigator navigator;

        public PaneViewModel(IPaneHandler handler)
        {
            this.handler = handler;
            this.moduleService = moduleService;
            navigator = moduleService.;
        }
    }
}

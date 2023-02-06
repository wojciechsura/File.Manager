using File.Manager.BusinessLogic.Services.Icons;
using File.Manager.BusinessLogic.Services.Messaging;
using File.Manager.BusinessLogic.Services.Modules;
using File.Manager.BusinessLogic.ViewModels.Base;
using File.Manager.BusinessLogic.ViewModels.Pane;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.ViewModels.Main
{
    public class MainWindowViewModel : BaseViewModel, IPaneHandler
    {
        // Private fields -----------------------------------------------------

        private readonly IMainWindowAccess access;

        // Public methods -----------------------------------------------------

        public MainWindowViewModel(IMainWindowAccess access, 
            IModuleService moduleService, 
            IIconService iconService,
            IMessagingService messagingService)
        {
            this.access = access;

            LeftPane = new PaneViewModel(this, moduleService, iconService, messagingService);
            RightPane = new PaneViewModel(this, moduleService, iconService, messagingService);
        }

        // Public properties --------------------------------------------------

        public PaneViewModel LeftPane { get; }
        public PaneViewModel RightPane { get; }
    }
}

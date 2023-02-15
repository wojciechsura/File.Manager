using File.Manager.BusinessLogic.Services.Icons;
using File.Manager.BusinessLogic.Services.Messaging;
using File.Manager.BusinessLogic.Services.Modules;
using File.Manager.BusinessLogic.ViewModels.Base;
using File.Manager.BusinessLogic.ViewModels.Pane;
using Spooksoft.VisualStateManager.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace File.Manager.BusinessLogic.ViewModels.Main
{
    public class MainWindowViewModel : BaseViewModel, IPaneHandler
    {


        // Private fields -----------------------------------------------------

        private readonly IMainWindowAccess access;

        private PaneViewModel leftPane;
        private PaneViewModel rightPane;
        
        private PaneViewModel activePane;

        // Private methods ----------------------------------------------------

        // IPaneHandler implementation ----------------------------------------

        void IPaneHandler.RequestSwithPane()
        {
            if (ActivePane == leftPane)
            {
                leftPane.Active = false;
                rightPane.Active = true;

            }
            else
            {
                rightPane.Active = false;
                leftPane.Active = true;
            }
        }

        public void NotifyActivated(PaneViewModel paneViewModel)
        {
            activePane = paneViewModel;
            InactivePane.Active = false;
            ActivePane.Active = true;
        }

        // Public methods -----------------------------------------------------

        public MainWindowViewModel(IMainWindowAccess access, 
            IModuleService moduleService, 
            IIconService iconService,
            IMessagingService messagingService)
        {
            this.access = access;

            leftPane = new PaneViewModel(this, moduleService, iconService, messagingService);
            rightPane = new PaneViewModel(this, moduleService, iconService, messagingService);

            activePane = leftPane;
        }

        // Public properties --------------------------------------------------

        public PaneViewModel LeftPane
        {
            get => leftPane;
            set => Set(ref leftPane, value);
        }

        public PaneViewModel RightPane
        {
            get => rightPane;
            set => Set(ref rightPane, value);
        }

        public PaneViewModel ActivePane
        {
            get => activePane;
            set => Set(ref activePane, value);
        }

        public PaneViewModel InactivePane
        {
            get
            {
                if (activePane == leftPane)
                    return rightPane;
                else if (activePane == rightPane)
                    return leftPane;
                else
                    return null;
            }
        }
    }
}

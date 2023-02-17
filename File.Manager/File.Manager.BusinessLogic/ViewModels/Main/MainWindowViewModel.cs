using File.Manager.API.Types;
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

        private void DoBufferedCopy(PaneViewModel activePane, PaneViewModel inactivePane)
        {
            var items = activePane.GetSelectedItems();
            if (!items.Any())
                return;


        }

        private void DoCopy()
        {
            var activePane = ActivePane;
            var inactivePane = InactivePane;

            var activeCapabilities = activePane.Navigator.GetLocationCapabilities();
            var inactiveCapabilities = inactivePane.Navigator.GetLocationCapabilities();

            LocationCapabilities requiredActiveCapabilities = LocationCapabilities.BufferedRead;
            LocationCapabilities requiredInactiveCapabilities = LocationCapabilities.BufferedWrite | LocationCapabilities.CreateFolder;

            // TODO direct copy

            if ((activeCapabilities & requiredActiveCapabilities) == requiredActiveCapabilities &&
                (inactiveCapabilities & requiredInactiveCapabilities) == requiredInactiveCapabilities)
            {
                DoBufferedCopy(activePane, inactivePane);
            }
        }

        private void DoSwitchPanes()
        {
            var oldLeftPane = leftPane;
            var oldRightPane = rightPane;

            LeftPane = null;
            RightPane = oldLeftPane;
            LeftPane = oldRightPane;

            access.FocusActivePane();
        }

        // Private properties -------------------------------------------------

        private PaneViewModel ActivePane
        {
            get => activePane;            
        }

        private PaneViewModel InactivePane
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

        // Public methods -----------------------------------------------------

        public MainWindowViewModel(IMainWindowAccess access, 
            IModuleService moduleService, 
            IIconService iconService,
            IMessagingService messagingService)
        {
            this.access = access;

            leftPane = new PaneViewModel(this, moduleService, iconService, messagingService);
            rightPane = new PaneViewModel(this, moduleService, iconService, messagingService);

            SwitchPanesCommand = new AppCommand(obj => DoSwitchPanes());
            CopyCommand = new AppCommand(obj => DoCopy());

            activePane = leftPane;
        }

        public void NotifyActivated(PaneViewModel paneViewModel)
        {
            activePane = paneViewModel;
            if (InactivePane != null)
                InactivePane.Active = false;
            if (ActivePane != null)
                ActivePane.Active = true;
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

        public ICommand SwitchPanesCommand { get; }

        public ICommand CopyCommand { get; }
    }
}

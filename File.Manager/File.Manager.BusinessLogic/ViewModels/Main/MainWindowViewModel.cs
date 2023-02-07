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
        // Private types ------------------------------------------------------

        private enum PaneKind
        {
            Left = 1,
            Right = 2
        }

        // Private fields -----------------------------------------------------

        private readonly IMainWindowAccess access;

        private PaneViewModel leftPane;
        private PaneViewModel rightPane;

        private bool leftPaneFocused = false;
        private bool rightPaneFocused = false;

        // Private methods ----------------------------------------------------

        private void DoSwitchPanes()
        {
            var tmp = LeftPane;
            LeftPane = RightPane;
            RightPane = tmp;
        }

        private void NotifyPaneFocusChange(PaneViewModel paneViewModel, bool newFocus)
        {
            if (paneViewModel == leftPane && leftPaneFocused != newFocus)
            {
                leftPaneFocused = newFocus;
                OnPropertyChanged(nameof(ActivePane));
                OnPropertyChanged(nameof(InactivePane));
            }
            else if (paneViewModel == rightPane && rightPaneFocused != newFocus)
            {
                rightPaneFocused = true;
                OnPropertyChanged(nameof(ActivePane));
                OnPropertyChanged(nameof(InactivePane));
            }
            else
                throw new InvalidOperationException("Unknown pane!");
        }

        // IPaneHandler implementation ----------------------------------------

        void IPaneHandler.NotifyPaneFocused(PaneViewModel paneViewModel)
        {
            NotifyPaneFocusChange(paneViewModel, true);
        }

        void IPaneHandler.NotifyPaneUnfocused(PaneViewModel paneViewModel)
        {
            NotifyPaneFocusChange(paneViewModel, false);
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
            get
            {
                if (leftPaneFocused && !rightPaneFocused)
                    return leftPane;
                else if (!leftPaneFocused && rightPaneFocused)
                    return rightPane;
                else
                    return null;
            }
        }

        public PaneViewModel InactivePane
        {
            get
            {
                if (leftPaneFocused && !rightPaneFocused)
                    return rightPane;
                else if (!leftPaneFocused && rightPaneFocused)
                    return leftPane;
                else
                    return null;
            }
        }

        public ICommand SwitchPanesCommand { get; }        
    }
}

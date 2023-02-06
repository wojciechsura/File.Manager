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

        // Private methods ----------------------------------------------------

        private void DoSwitchPanes()
        {
            var tmp = LeftPane;
            LeftPane = RightPane;
            RightPane = tmp;
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

        public ICommand SwitchPanesCommand { get; }        
    }
}

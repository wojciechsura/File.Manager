using File.Manager.API.Filesystem.Models.Items.Listing;
using File.Manager.API.Types;
using File.Manager.BusinessLogic.Models.Dialogs.CopyMoveConfiguration;
using File.Manager.BusinessLogic.Services.Dialogs;
using File.Manager.BusinessLogic.Services.Icons;
using File.Manager.BusinessLogic.Services.Messaging;
using File.Manager.BusinessLogic.Services.Modules;
using File.Manager.BusinessLogic.Types;
using File.Manager.BusinessLogic.ViewModels.Base;
using File.Manager.BusinessLogic.ViewModels.Operations;
using File.Manager.BusinessLogic.ViewModels.Operations.CopyMove;
using File.Manager.BusinessLogic.ViewModels.Pane;
using File.Manager.Common.Helpers;
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
        private readonly IDialogService dialogService;
        private readonly IMessagingService messagingService;

        private PaneViewModel leftPane;
        private PaneViewModel rightPane;
        
        private PaneViewModel activePane;

        // Private methods ----------------------------------------------------

        private (bool result, CopyMoveConfigurationModel model) ShowCopyMoveDialog(DataTransferOperationType operationType, 
            IReadOnlyList<Item> items, 
            string fromAddress, 
            string toAddress)
        {
            if (!items.Any())
                return (false, null);

            var input = new CopyMoveConfigurationInputModel(operationType, fromAddress, toAddress, items);

            return dialogService.ShowCopyMoveConfigurationDialog(input);            
        }

        private void DoBufferedCopy(PaneViewModel activePane, PaneViewModel inactivePane, bool withPlan)
        {
            var items = activePane.GetSelectedItems();

            (bool result, CopyMoveConfigurationModel model) = ShowCopyMoveDialog(
                DataTransferOperationType.Copy,
                items,
                activePane.Navigator.Address,
                inactivePane.Navigator.Address);

            if (!result)
                return;

            BaseCopyMoveOperationViewModel operation;

            if (withPlan)
            {
                operation = new BufferedCopyMoveWithPlanOperationViewModel(dialogService,
                    messagingService,
                    DataTransferOperationType.Copy,
                    activePane.Navigator.CreateOperatorForCurrentLocation(),
                    inactivePane.Navigator.CreateOperatorForCurrentLocation(),
                    model,
                    items);
            }
            else
            {
                operation = new BufferedCopyMoveOperationViewModel(dialogService,
                    messagingService,
                    DataTransferOperationType.Copy,
                    activePane.Navigator.CreateOperatorForCurrentLocation(),
                    inactivePane.Navigator.CreateOperatorForCurrentLocation(),
                    model,
                    items);
            }

            dialogService.ShowCopyMoveProgress(operation);
        }

        private void DoCopy()
        {
            var copyFromPane = ActivePane;
            var copyToPane = InactivePane;

            var copyFromCapabilities = copyFromPane.Navigator.GetLocationCapabilities();
            var copyToCapabilities = copyToPane.Navigator.GetLocationCapabilities();

            if (copyFromCapabilities.HasFlag(LocationCapabilities.BufferedRead) &&
                copyToCapabilities.HasFlag(LocationCapabilities.BufferedWrite | LocationCapabilities.CreateFolder))
            {
                DoBufferedCopy(copyFromPane, copyToPane, copyFromCapabilities.HasFlag(LocationCapabilities.Plan));
            }

            // TODO direct
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
            IMessagingService messagingService,
            IDialogService dialogService)
        {
            this.access = access;
            this.dialogService = dialogService;
            this.messagingService = messagingService;

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

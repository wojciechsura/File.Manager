using File.Manager.API.Filesystem.Models.Items.Listing;
using File.Manager.API.Types;
using File.Manager.BusinessLogic.Models.Dialogs.CopyMoveConfiguration;
using File.Manager.BusinessLogic.Models.Dialogs.DeleteConfiguration;
using File.Manager.BusinessLogic.Models.Dialogs.NewFolderConfiguration;
using File.Manager.BusinessLogic.Services.Dialogs;
using File.Manager.BusinessLogic.Services.Icons;
using File.Manager.BusinessLogic.Services.Messaging;
using File.Manager.BusinessLogic.Services.Modules;
using File.Manager.BusinessLogic.Types;
using File.Manager.BusinessLogic.ViewModels.Base;
using File.Manager.BusinessLogic.ViewModels.Operations;
using File.Manager.BusinessLogic.ViewModels.Operations.CopyMove;
using File.Manager.BusinessLogic.ViewModels.Operations.Delete;
using File.Manager.BusinessLogic.ViewModels.Pane;
using File.Manager.Common.Helpers;
using File.Manager.Resources.Windows.MainWindow;
using Spooksoft.VisualStateManager.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        private void DoBufferedCopyMove(PaneViewModel activePane, PaneViewModel inactivePane, bool withPlan, DataTransferOperationType operationType)
        {
            var items = activePane.GetSelectedItems();

            (bool result, CopyMoveConfigurationModel model) = ShowCopyMoveDialog(
                operationType,
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
                    operationType,
                    activePane.Navigator.CreateOperatorForCurrentLocation(),
                    inactivePane.Navigator.CreateOperatorForCurrentLocation(),
                    model,
                    items);
            }
            else
            {
                operation = new BufferedCopyMoveOperationViewModel(dialogService,
                    messagingService,
                    operationType,
                    activePane.Navigator.CreateOperatorForCurrentLocation(),
                    inactivePane.Navigator.CreateOperatorForCurrentLocation(),
                    model,
                    items);
            }

            dialogService.ShowCopyMoveProgress(operation);
        }

        private void DoCopyMove(DataTransferOperationType operationType)
        {
            var copyFromPane = ActivePane;
            var copyToPane = InactivePane;

            var copyFromCapabilities = copyFromPane.Navigator.GetLocationCapabilities();
            var copyToCapabilities = copyToPane.Navigator.GetLocationCapabilities();

            if (copyFromCapabilities.HasFlag(LocationCapabilities.BufferedRead) &&
                copyToCapabilities.HasFlag(LocationCapabilities.BufferedWrite | LocationCapabilities.CreateFolder))
            {
                DoBufferedCopyMove(copyFromPane, copyToPane, copyFromCapabilities.HasFlag(LocationCapabilities.Plan), operationType);
            }

            // TODO direct

            foreach (var item in copyFromPane.Items)
            {
                item.IsSelected = false;
            }
            copyToPane.Refresh();
        }

        private (bool result, DeleteConfigurationModel model) ShowDeleteDialog(List<Item> items, string address)
        {
            if (!items.Any())
                return (false, null);

            var input = new DeleteConfigurationInputModel(address, items);

            return dialogService.ShowDeleteConfigurationDialog(input);
        }

        private void DoPerformDelete(PaneViewModel deletePane, bool withPlan)
        {
            var items = deletePane.GetSelectedItems();

            (bool result, DeleteConfigurationModel model) = ShowDeleteDialog(items,
                deletePane.Navigator.Address);

            if (!result)
                return;

            BaseDeleteOperationViewModel operation;

            if (withPlan)
            {
                operation = new DeleteWithPlanOperationViewModel(dialogService,
                    messagingService,
                    deletePane.Navigator.CreateOperatorForCurrentLocation(),                    
                    model,
                    items);
            }
            else
            {
                operation = new DeleteOperationViewModel(dialogService,
                    messagingService,
                    deletePane.Navigator.CreateOperatorForCurrentLocation(),
                    model,
                    items);
            }

            dialogService.ShowDeleteProgress(operation);

            deletePane.Refresh();
        }

        private void DoDelete()
        {
            var deletePane = ActivePane;

            var deleteCapabilities = deletePane.Navigator.GetLocationCapabilities();

            if (deleteCapabilities.HasFlag(LocationCapabilities.Delete))
            {
                DoPerformDelete(deletePane, deleteCapabilities.HasFlag(LocationCapabilities.Plan));
            }
        }

        private void DoPerformNewFolder(PaneViewModel newFolderPane)
        {
            (bool result, NewFolderConfigurationModel model) = dialogService.ShowNewFolderConfigurationDialog();

            if (result)
            {
                var filesystemOperator = newFolderPane.Navigator.CreateOperatorForCurrentLocation();

                bool? fileExists = filesystemOperator.FileExists(model.Name);
                if (fileExists == null)
                {
                    messagingService.ShowError(string.Format(Strings.Error_CannotCheckIfFileExists, model.Name));
                    return;
                }
                else if (fileExists == true)
                {
                    messagingService.ShowError(string.Format(Strings.Error_FileAlreadyExists, model.Name)); 
                    return;
                }

                bool? folderExists = filesystemOperator.FolderExists(model.Name);
                if (folderExists == null)
                {
                    messagingService.ShowError(string.Format(Strings.Error_CannotCheckIfFolderExists, model.Name));
                    return;
                }
                else if (folderExists == true)
                {
                    messagingService.ShowError(string.Format(Strings.Error_FolderAlreadyExists, model.Name));
                    return;
                }

                if (!filesystemOperator.CreateFolder(model.Name))
                {
                    messagingService.ShowError(string.Format(Strings.Error_CannotCreateFolder, model.Name));
                    return;
                }

                newFolderPane.Refresh();
                newFolderPane.SelectItem(model.Name);
            }
        }

        private void DoNewFolder()
        {
            var newFolderPane = ActivePane;

            var newFolderCapabilities = newFolderPane.Navigator.GetLocationCapabilities();

            if (newFolderCapabilities.HasFlag(LocationCapabilities.CreateFolder))
            {
                DoPerformNewFolder(newFolderPane);
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
            IMessagingService messagingService,
            IDialogService dialogService)
        {
            this.access = access;
            this.dialogService = dialogService;
            this.messagingService = messagingService;

            leftPane = new PaneViewModel(this, moduleService, iconService, messagingService);
            rightPane = new PaneViewModel(this, moduleService, iconService, messagingService);

            SwitchPanesCommand = new AppCommand(obj => DoSwitchPanes());
            CopyCommand = new AppCommand(obj => DoCopyMove(DataTransferOperationType.Copy));
            MoveCommand = new AppCommand(obj => DoCopyMove(DataTransferOperationType.Move));
            NewFolderCommand = new AppCommand(obj => DoNewFolder());
            DeleteCommand = new AppCommand(obj => DoDelete());

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
        public ICommand MoveCommand { get; }
        public ICommand NewFolderCommand { get; }
        public ICommand DeleteCommand { get; }
    }
}

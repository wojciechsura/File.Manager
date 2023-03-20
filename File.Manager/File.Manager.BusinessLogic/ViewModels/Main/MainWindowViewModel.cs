using File.Manager.API.Filesystem.Models.Items.Listing;
using File.Manager.API.Types;
using File.Manager.BusinessLogic.Models.Dialogs.CopyMoveConfiguration;
using File.Manager.BusinessLogic.Models.Dialogs.DeleteConfiguration;
using File.Manager.BusinessLogic.Models.Dialogs.NewFolderConfiguration;
using File.Manager.BusinessLogic.Models.Dialogs.Selection;
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
using System.IO;
using System.Linq;
using System.Net;
using System.Printing;
using System.Text;
using System.Text.RegularExpressions;
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

        private void DoView()
        {
            var viewPane = ActivePane;

            var capabilities = viewPane.Navigator.GetLocationCapabilities();
            if (!capabilities.HasFlag(LocationCapabilities.BufferedRead))
                return;

            var focusedItem = viewPane.GetFocusedItem();
            if (focusedItem is not FileItem)
                return;

            var filesystemOperator = viewPane.Navigator.CreateOperatorForCurrentLocation();
            Stream stream = filesystemOperator.OpenFileForReading(focusedItem.Name);
            if (stream == null)
                return;

            dialogService.ShowViewWindow(stream, focusedItem.Name);

            stream.Dispose();
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

            RefreshPanes();            
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

            RefreshPanes();
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

                RefreshPanes();

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

        private void DoInvertSelection()
        {
            foreach (var item in ActivePane.Items.Where(i => i.IsSelectable))
            {
                item.IsSelected = !item.IsSelected;
            }
        }

        private void DoChangeSelection(SelectionOperationKind operation)
        {
            static void ApplySelection(SelectionOperationKind operation, ItemViewModel item)
            {
                item.IsSelected = operation switch
                {
                    SelectionOperationKind.Add => true,
                    SelectionOperationKind.Remove => false,
                    _ => throw new InvalidOperationException("Unsupported selection operation kind")
                };
            }

            (bool result, SelectionResultModel model) = dialogService.ShowSelectionDialog(operation);
            if (result)
            {
                switch (model.SelectionMethod)
                {
                    case SelectionMethod.Mask:
                        {
                            foreach (var item in ActivePane.Items
                                .Where(i => i.IsSelectable && PatternMatcher.StrictMatchPattern(model.Mask, i.Name)))
                                ApplySelection(operation, item);

                            break;
                        }
                    case SelectionMethod.RegularExpression:
                        {
                            var regex = new Regex(model.RegularExpression, RegexOptions.IgnoreCase);

                            foreach (var item in ActivePane.Items.Where(i => i.IsSelectable && regex.IsMatch(i.Name)))
                                ApplySelection(operation, item);

                            break;
                        }
                    default:
                        throw new InvalidOperationException("Unsupported selection method!");
                }
            }
        }

        private void RefreshPanes()
        {
            leftPane.Refresh();
            rightPane.Refresh();
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

            ViewCommand = new AppCommand(obj => DoView());
            CopyCommand = new AppCommand(obj => DoCopyMove(DataTransferOperationType.Copy));
            MoveCommand = new AppCommand(obj => DoCopyMove(DataTransferOperationType.Move));
            NewFolderCommand = new AppCommand(obj => DoNewFolder());
            DeleteCommand = new AppCommand(obj => DoDelete());

            InvertSelectionCommand = new AppCommand(obj => DoInvertSelection());
            AddToSelectionCommand = new AppCommand(obj => DoChangeSelection(SelectionOperationKind.Add));
            RemoveFromSelectionCommand = new AppCommand(obj => DoChangeSelection(SelectionOperationKind.Remove));

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

        public ICommand ViewCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand MoveCommand { get; }
        public ICommand NewFolderCommand { get; }
        public ICommand DeleteCommand { get; }

        public ICommand InvertSelectionCommand { get; }
        public ICommand AddToSelectionCommand { get; }
        public ICommand RemoveFromSelectionCommand { get; }
    }
}

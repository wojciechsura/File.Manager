using File.Manager.BusinessLogic.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using File.Manager.Resources.Windows.CopyMoveConfigurationWindow;
using SmartFormat;
using File.Manager.BusinessLogic.Models.Dialogs.CopyMoveConfiguration;
using File.Manager.BusinessLogic.ViewModels.Base;
using System.Windows.Input;
using Spooksoft.VisualStateManager.Commands;
using File.Manager.API.Filesystem.Models.Items.Listing;
using Spooksoft.VisualStateManager.Conditions;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace File.Manager.BusinessLogic.ViewModels.CopyMoveConfiguration
{
    public class CopyMoveConfigurationWindowViewModel : BaseViewModel
    {
        // Private fields -----------------------------------------------------

        private readonly ICopyMoveCOnfigurationWindowAccess access;

        private DataTransferOperationType operationType;
        private IReadOnlyList<Item> selectedItems;

        private GenericProblemResolution overwritingOptions;
        private string fileMask;
        private bool renameFiles;
        private bool renameRecursive;
        private string renameFrom;
        private string renameTo;

        // Private methods ----------------------------------------------------

        private void DoOk()
        {
            Result = new CopyMoveConfigurationModel(FileMask, 
                OverwritingOptions, 
                RenameFiles, 
                RenameRecursive, 
                RenameFrom, 
                RenameTo);
            access.Close(true);
        }

        private void DoCancel()
        {
            Result = null;
            access.Close(false);
        }

        private static bool IsValidRegex(string regex)
        {
            try
            {
                _ = new Regex(regex);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Public methods -----------------------------------------------------

        public CopyMoveConfigurationWindowViewModel(CopyMoveConfigurationInputModel input, ICopyMoveCOnfigurationWindowAccess access)
        {
            this.access = access ?? throw new ArgumentNullException(nameof(access));

            this.operationType = input.OperationType;
            this.selectedItems = input.SelectedItems;
            this.SourceAddress = input.SourceAddress;
            this.DestinationAddress = input.DestinationAddress;

            renameFrom = @"^(.*)(?:\\.([^\\.]*))$";
            renameTo = @"$1\.$2";
            renameFiles = false;
            renameRecursive = false;

            var useRenamingCondition = new PropertyWatchCondition<CopyMoveConfigurationWindowViewModel>(this, vm => vm.RenameFiles, false);
            var renameFromValidCondition = new ChainedLambdaCondition<CopyMoveConfigurationWindowViewModel>(this, vm => IsValidRegex(RenameFrom), false);
            var canConfirmCondition = !useRenamingCondition | (useRenamingCondition & renameFromValidCondition);

            OkCommand = new AppCommand(obj => DoOk(), canConfirmCondition);
            CancelCommand = new AppCommand(obj => DoCancel());
        }

        // Public properties --------------------------------------------------

        public string Title => operationType switch
        {
            DataTransferOperationType.Copy => Strings.Title_Copy,
            DataTransferOperationType.Move => Strings.Title_Move,
            _ => null
        };

        public string SummaryHeader => operationType switch
        {
            DataTransferOperationType.Copy => Smart.Format(Strings.SummaryHeader_Copy, selectedItems.Count),
            DataTransferOperationType.Move => Smart.Format(Strings.SummaryHeader_Move, selectedItems.Count),
            _ => null
        };

        public string SummaryValue => selectedItems.Count switch
        {
            1 => selectedItems.Single().Name,
            > 1 => Smart.Format(Strings.SummaryValue_Plural, selectedItems.Count.ToString()),
            _ => throw new InvalidOperationException("Invalid count of items to process!")
        };

        public string SourceAddress { get; }

        public string DestinationAddress { get; }

        public GenericProblemResolution OverwritingOptions
        {
            get => overwritingOptions;
            set => Set(ref overwritingOptions, value);
        }

        public string FileMask
        {
            get => fileMask;
            set => Set(ref fileMask, value);
        }

        public bool RenameFiles
        {
            get => renameFiles;
            set => Set(ref renameFiles, value);
        }

        public bool RenameRecursive
        {
            get => renameRecursive;
            set => Set(ref renameRecursive, value);
        }

        public string RenameFrom
        {
            get => renameFrom;
            set => Set(ref renameFrom, value);
        }

        public string RenameTo
        {
            get => renameTo;
            set => Set(ref renameTo, value);
        }

        public ICommand OkCommand { get; }

        public ICommand CancelCommand { get; }

        public CopyMoveConfigurationModel Result { get; private set; }
    }
}

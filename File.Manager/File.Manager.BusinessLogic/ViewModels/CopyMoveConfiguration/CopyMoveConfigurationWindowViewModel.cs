using File.Manager.BusinessLogic.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using File.Manager.Resources.Windows.CopyMoveConfigurationWindow;
using File.Manager.API.Filesystem.Models.Items;
using SmartFormat;
using File.Manager.BusinessLogic.Models.Dialogs.CopyMoveConfiguration;
using File.Manager.BusinessLogic.ViewModels.Base;
using System.Windows.Input;
using Spooksoft.VisualStateManager.Commands;

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

        // Private methods ----------------------------------------------------

        private void DoOk()
        {
            Result = new CopyMoveConfigurationModel(FileMask, OverwritingOptions);
            access.Close(true);
        }

        private void DoCancel()
        {
            Result = null;
            access.Close(false);
        }

        // Public methods -----------------------------------------------------

        public CopyMoveConfigurationWindowViewModel(CopyMoveConfigurationInputModel input, ICopyMoveCOnfigurationWindowAccess access)
        {
            this.access = access ?? throw new ArgumentNullException(nameof(access));

            this.operationType = input.OperationType;
            this.selectedItems = input.SelectedItems;
            this.SourceAddress = input.SourceAddress;
            this.DestinationAddress = input.DestinationAddress;

            OkCommand = new AppCommand(obj => DoOk());
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

        public ICommand OkCommand { get; }

        public ICommand CancelCommand { get; }

        public CopyMoveConfigurationModel Result { get; private set; }
    }
}

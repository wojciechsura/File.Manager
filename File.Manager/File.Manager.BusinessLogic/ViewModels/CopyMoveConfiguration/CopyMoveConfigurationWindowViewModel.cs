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

namespace File.Manager.BusinessLogic.ViewModels.CopyMoveConfiguration
{
    public class CopyMoveConfigurationWindowViewModel
    {
        private DataTransferOperationType operationType;
        private IReadOnlyList<Item> selectedItems;

        public CopyMoveConfigurationWindowViewModel(DataTransferOperationType operationType, IReadOnlyList<Item> selectedItems)
        {
            this.operationType = operationType;
            this.selectedItems = selectedItems;
        }

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
    }
}

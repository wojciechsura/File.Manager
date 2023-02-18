using File.Manager.API.Filesystem.Models.Items;
using File.Manager.BusinessLogic.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Models.Dialogs.CopyMoveConfiguration
{
    public class CopyMoveConfigurationInputModel
    {
        public CopyMoveConfigurationInputModel(DataTransferOperationType operationType, string sourceAddress, string destinationAddress, List<Item> selectedItems)
        {
            OperationType = operationType;
            SourceAddress = sourceAddress;
            DestinationAddress = destinationAddress;
            SelectedItems = selectedItems;
        }

        public DataTransferOperationType OperationType { get; }
        public string SourceAddress { get; }
        public string DestinationAddress { get; } 
        public List<Item> SelectedItems { get; }
    }
}

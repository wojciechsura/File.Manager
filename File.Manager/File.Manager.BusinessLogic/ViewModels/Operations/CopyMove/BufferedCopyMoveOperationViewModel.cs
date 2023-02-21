using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Items;
using File.Manager.BusinessLogic.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.ViewModels.Operations.CopyMove
{
    public class BufferedCopyMoveOperationViewModel : BaseOperationViewModel
    {
        public BufferedCopyMoveOperationViewModel(DataTransferOperationType copy,
            IFilesystemOperator sourceNavigaor,
            IFilesystemOperator destinationNavigator, 
            List<Item> items)
        {
            throw new NotImplementedException();
        }

        public override void Run()
        {
            throw new NotImplementedException();
        }
    }
}

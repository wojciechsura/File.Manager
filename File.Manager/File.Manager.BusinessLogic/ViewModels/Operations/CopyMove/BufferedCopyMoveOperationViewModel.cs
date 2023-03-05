using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Items;
using File.Manager.BusinessLogic.Services.Dialogs;
using File.Manager.BusinessLogic.Services.Messaging;
using File.Manager.BusinessLogic.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.ViewModels.Operations.CopyMove
{
    public class BufferedCopyMoveOperationViewModel : BaseCopyMoveOperationViewModel
    {
        public BufferedCopyMoveOperationViewModel(IDialogService dialogService,
            IMessagingService messagingService,
            DataTransferOperationType copy,
            IFilesystemOperator sourceNavigaor,
            IFilesystemOperator destinationNavigator, 
            List<Item> items)
            : base(dialogService, messagingService)
        {
            throw new NotImplementedException();
        }

        public override void Run()
        {
            ProgressIndeterminate = true;

            throw new NotImplementedException();
        }
    }
}

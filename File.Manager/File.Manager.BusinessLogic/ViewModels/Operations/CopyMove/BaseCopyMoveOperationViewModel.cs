using File.Manager.BusinessLogic.Services.Dialogs;
using File.Manager.BusinessLogic.Services.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.ViewModels.Operations.CopyMove
{
    public abstract class BaseCopyMoveOperationViewModel : BaseOperationViewModel
    {
        private int fileProgress;
        private string fileProgressDescription;
        private string fromAddress;
        private string toAddress;

        public BaseCopyMoveOperationViewModel(IDialogService dialogService, IMessagingService messagingService)
            : base(dialogService, messagingService)
        {

        }

        public int FileProgress
        {
            get => fileProgress;
            set => Set(ref fileProgress, value);
        }

        public string FileProgressDescription
        {
            get => fileProgressDescription;
            set => Set(ref fileProgressDescription, value);
        }

        public string FromAddress
        {
            get => fromAddress; 
            set => Set(ref fromAddress, value);
        }

        public string ToAddress
        {
            get => toAddress;
            set => Set(ref toAddress, value);
        }
    }
}

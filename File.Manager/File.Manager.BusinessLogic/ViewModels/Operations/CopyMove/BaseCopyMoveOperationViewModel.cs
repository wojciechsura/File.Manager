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
    }
}

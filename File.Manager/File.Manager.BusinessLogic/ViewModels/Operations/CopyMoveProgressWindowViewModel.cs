using File.Manager.BusinessLogic.Services.Messaging;
using File.Manager.BusinessLogic.ViewModels.Base;
using File.Manager.BusinessLogic.ViewModels.Operations.CopyMove;
using File.Manager.Resources.Windows.CopyMoveProgressWindow;
using Spooksoft.VisualStateManager.Commands;
using Spooksoft.VisualStateManager.Conditions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;

namespace File.Manager.BusinessLogic.ViewModels.Operations
{
    public class CopyMoveProgressWindowViewModel : BaseViewModel
    {
        private readonly ICopyMoveProgressWindowAccess access;
        private readonly IMessagingService messagingService;
        private readonly BaseCopyMoveOperationViewModel operation;
        private bool isCancelling;

        private void HandleOperationFinished(object sender, EventArgs args)
        {
            access.Close();
        }

        private void DoCancel()
        {
            if (isCancelling)
                return;

            operation.Cancel();
            IsCancelling = true;
        }

        public CopyMoveProgressWindowViewModel(IMessagingService messagingService,
            BaseCopyMoveOperationViewModel operation, 
            ICopyMoveProgressWindowAccess access)
        {
            this.messagingService = messagingService;
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
            this.access = access;

            var canCancelCondition = new LambdaCondition<CopyMoveProgressWindowViewModel>(this, vm => !vm.IsCancelling, false);
            CancelCommand = new AppCommand(obj => DoCancel(), canCancelCondition);

            operation.Finished += HandleOperationFinished;
        }

        public void NotifyLoaded()
        {
            operation.Run();
        }

        public void NotifyCloseBeforeFinish()
        {
            if (CancelCommand.CanExecute(null))
            {
                if (messagingService.AskYesNo(Strings.Question_CancelOperationInProgress))
                {
                    CancelCommand.Execute(null);
                }
            }

        }

        public BaseOperationViewModel Operation => operation;

        public ICommand CancelCommand { get; }

        public bool IsCancelling
        {
            get => isCancelling;
            set => Set(ref isCancelling, value);
        }

        public bool IsFinished => Operation.IsFinished;
    }
}

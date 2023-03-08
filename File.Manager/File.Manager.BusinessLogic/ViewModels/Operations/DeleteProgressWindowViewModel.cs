using File.Manager.BusinessLogic.Services.Messaging;
using File.Manager.BusinessLogic.ViewModels.Base;
using File.Manager.BusinessLogic.ViewModels.Operations.Delete;
using File.Manager.Resources.Windows.DeleteProgressWindow;
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
    public class DeleteProgressWindowViewModel : BaseViewModel
    {
        private readonly IDeleteProgressWindowAccess access;
        private readonly IMessagingService messagingService;
        private readonly BaseDeleteOperationViewModel operation;
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

        public DeleteProgressWindowViewModel(IMessagingService messagingService,
            BaseDeleteOperationViewModel operation, 
            IDeleteProgressWindowAccess access)
        {
            this.messagingService = messagingService;
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
            this.access = access;

            var canCancelCondition = Condition.Lambda(this, vm => !vm.IsCancelling, false);
            CancelCommand = new AppCommand(obj => DoCancel(), canCancelCondition);

            operation.Finished += HandleOperationFinished;
        }

        public void NotifyLoaded()
        {
            operation.Run();
        }

        public void NotifyUserRequestedClose()
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

        public bool CanClose => Operation.IsFinished;
    }
}

using File.Manager.BusinessLogic.Services.Dialogs;
using File.Manager.BusinessLogic.Services.Messaging;
using File.Manager.BusinessLogic.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.ViewModels.Operations
{
    public abstract class BaseOperationViewModel : BaseViewModel
    {
        // Private fields -----------------------------------------------------

        private string title;
        private int progress;
        private string progressDescription;
        private bool progressIndeterminate;
        private bool isFinished;
        
        // Protected fields ---------------------------------------------------
        
        protected readonly IDialogService dialogService;
        protected readonly IMessagingService messagingService;

        // Protected methods --------------------------------------------------

        protected void OnFinished()
        {
            IsFinished = true;
            Finished?.Invoke(this, EventArgs.Empty);
        }

        // Public methods -----------------------------------------------------

        public BaseOperationViewModel(IDialogService dialogService, IMessagingService messagingService)
        {
            this.dialogService = dialogService;
            this.messagingService = messagingService;
        }

        public abstract void Run();

        // Public properties --------------------------------------------------

        public string Title
        {
            get => title;
            set => Set(ref title, value);
        }

        public int Progress
        {
            get => progress;
            set => Set(ref progress, value);
        }

        public string ProgressDescription
        {
            get => progressDescription;
            set => Set(ref progressDescription, value);
        }

        public bool ProgressIndeterminate
        {
            get => progressIndeterminate;
            set => Set(ref progressIndeterminate, value);
        }

        public bool IsFinished
        {
            get => isFinished;
            private set => Set(ref isFinished, value);
        }

        public event EventHandler Finished;
    }
}

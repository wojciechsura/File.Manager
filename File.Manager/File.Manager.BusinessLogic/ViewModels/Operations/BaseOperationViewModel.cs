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
        private object description;
        private int overallProgress;
        private string overallProgressDescription;
        private bool overallProgressVisible;
        private int detailedProgress;
        private string detailedProgressDescription;
        private bool detailedProgressVisible;
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

        public object Description
        {
            get => description;
            set => Set(ref description, value);
        }

        public int OverallProgress
        {
            get => overallProgress;
            set => Set(ref overallProgress, value);
        }

        public string OverallProgressDescription
        {
            get => overallProgressDescription;
            set => Set(ref overallProgressDescription, value);
        }

        public bool OverallProgressVisible
        {
            get => overallProgressVisible; 
            set => Set(ref overallProgressVisible, value);
        }

        public int DetailedProgress
        {
            get => detailedProgress;
            set => Set(ref detailedProgress, value);
        }

        public string DetailedProgressDescription
        {
            get => detailedProgressDescription;
            set => Set(ref detailedProgressDescription, value);
        }

        public bool DetailedProgressVisible
        {
            get => detailedProgressVisible; 
            set => Set(ref detailedProgressVisible, value);
        }

        public bool IsFinished
        {
            get => isFinished;
            private set => Set(ref IsFinished, value);
        }

        public event EventHandler Finished;
    }
}

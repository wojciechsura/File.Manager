using File.Manager.API.Filesystem.Models.Items.Plan;
using File.Manager.BusinessLogic.Services.Dialogs;
using File.Manager.BusinessLogic.Services.Messaging;
using File.Manager.BusinessLogic.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.ViewModels.Operations
{
    public abstract class BaseOperationViewModel : BaseViewModel
    {
        // Protected types ----------------------------------------------------

        protected class BaseWorker : BackgroundWorker
        {
            protected (TimeSpan elapsed, string elapsedString) EvalElapsed(DateTime startTime)
            {
                TimeSpan elapsed = DateTime.Now - startTime;
                string elapsedString = GetTimeSpanString(elapsed);
                return (elapsed, elapsedString);
            }

            protected string GetTimeSpanString(TimeSpan left)
            {
                return left.Days > 0 ? left.ToString("d'd'\\, hh\\:mm\\:ss") : left.ToString("hh\\:mm\\:ss");
            }

            protected (long totalSize, int totalFiles) EvaluatePlanTotalsRecursive(IReadOnlyList<BasePlanItem> items)
            {
                long totalSize = 0;
                int totalFiles = 0;

                foreach (var item in items)
                {
                    switch (item)
                    {
                        case PlanFile planFile:
                            {
                                totalSize += planFile.Size;
                                totalFiles++;
                                break;
                            }
                        case PlanFolder planFolder:
                            {
                                (long folderSize, int folderFiles) = EvaluatePlanTotalsRecursive(planFolder);
                                totalSize += folderSize;
                                totalFiles += folderFiles;
                                break;
                            }
                        default:
                            throw new InvalidOperationException("Unsupported plan item!");
                    }
                }

                return (totalSize, totalFiles);
            }
        }

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

        public abstract void Cancel();

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

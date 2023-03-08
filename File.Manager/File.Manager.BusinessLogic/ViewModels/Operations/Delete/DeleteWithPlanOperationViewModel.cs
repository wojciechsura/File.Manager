using File.Manager.API.Filesystem.Models.Items.Listing;
using File.Manager.API.Filesystem.Models.Items;
using File.Manager.API.Filesystem;
using File.Manager.BusinessLogic.Models.Dialogs.DeleteConfiguration;
using File.Manager.BusinessLogic.Services.Dialogs;
using File.Manager.BusinessLogic.Services.Messaging;
using File.Manager.BusinessLogic.Types;
using File.Manager.Resources.Operations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File.Manager.API.Filesystem.Models.Items.Plan;

namespace File.Manager.BusinessLogic.ViewModels.Operations.Delete
{
    public class DeleteWithPlanOperationViewModel : BaseDeleteOperationViewModel
    {
        // Private types ------------------------------------------------------

        private sealed class DeleteWorkerContext : BaseDeleteWorkerContext
        {
            public DeleteWorkerContext(DeleteConfigurationModel configuration, long totalSize, int totalFiles)
                : base(configuration)
            {
                TotalSize = totalSize;
                TotalFiles = totalFiles;
            }

            public long TotalSize { get; }
            public int TotalFiles { get; }
        }

        private sealed class DeleteWorker : BaseDeleteWorker<DeleteWorkerContext>
        {
            // Private fields -------------------------------------------------

            private DateTime startTime;

            // Protected methods ----------------------------------------------

            protected override (bool exit, DeleteWorkerResult result) DeleteFile(DeleteWorkerContext context, IFileInfo fileInfo, IFilesystemOperator filesystemOperator)
            {
                if (CancellationPending)
                    return (true, new CancelledDeleteWorkerResult());

                (bool exit, DeleteWorkerResult result) = base.DeleteFile(context, fileInfo, filesystemOperator);

                // Statistics

                context.DeletedFiles++;
                context.DeletedSize += fileInfo.Size;

                // Elapsed

                (TimeSpan elapsed, string elapsedString) = EvalElapsed(startTime);

                // Estimated left

                var millisecondsLeft = context.DeletedFiles switch
                {
                    > 0 => (long)elapsed.TotalMilliseconds * (context.TotalFiles - context.DeletedFiles) / context.DeletedFiles,
                    _ => 0
                };
                TimeSpan left = TimeSpan.FromMilliseconds(millisecondsLeft);
                string leftString = GetTimeSpanString(left);

                // Speed

                string deletedFilesPerSecond = elapsed.TotalSeconds > 0 ? $" ({context.DeletedFiles / elapsed.TotalSeconds})" : "";

                // Description

                var totalDescription = string.Format(Strings.Delete_Info_TotalDescription, 
                    context.DeletedFiles,
                    context.TotalFiles,
                    context.DeletedSize,
                    context.TotalSize,
                    elapsedString,
                    leftString,
                    deletedFilesPerSecond);

                ReportProgress(0, new DeleteProgress((int)(context.DeletedFiles * 100 / context.TotalFiles), 
                    totalDescription));

                return (exit, result);
            }

            protected override (bool exit, DeleteWorkerResult result) RetrieveFolderContents(DeleteWorkerContext context,
                IFolderInfo folderInfo,
                IFilesystemOperator filesystemOperator,
                ref IReadOnlyList<IBaseItemInfo> items)
            {
                items = (PlanFolder)folderInfo;
                return (false, null);
            }

            protected override void OnDoWork(DoWorkEventArgs e)
            {
                startTime = DateTime.Now;

                var input = (DeleteWorkerInput)e.Argument;
                var configuration = input.Configuration;

                // 1. Plan

                var plan = input.FilesystemOperator.BuildOperationPlanFromSelection(input.SelectedItems, input.Configuration.FileMask);

                // 2. Evaluate totals

                (long totalSize, int totalFiles) = EvaluatePlanTotalsRecursive(plan);
                var context = new DeleteWorkerContext(configuration, totalSize, totalFiles);

                // 3. Copying/moving files
                
                var result = ProcessItems(context, plan, input.FilesystemOperator);

                if (result != null)
                    e.Result = result;
                else
                    e.Result = new SuccessDeleteWorkerResult();
            }

            // Public methods -------------------------------------------------

            public DeleteWorker()
            {

            }
        }

        // Private fields -----------------------------------------------------

        private readonly DeleteWorker worker;

        // Private methods ----------------------------------------------------

        private void HandleWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is UserQuestionRequestProgress userQuestion)
            {
                (bool result, SingleDeleteProblemResolution resolution) = dialogService.ShowUserDecisionDialog(userQuestion.AvailableResolutions, userQuestion.Header);
                
                if (result)
                    worker.UserDecision = resolution;
                else
                    worker.UserDecision = SingleDeleteProblemResolution.Abort;

                worker.UserDecisionSemaphore.Release();
            }
            else if (e.UserState is DeleteProgress progress)
            {
                Progress = progress.Progress;
                ProgressDescription = progress.Description;
            }
        }

        private void HandleWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is CriticalFailureDeleteWorkerResult critical)
            {
                messagingService.ShowError(critical.LocalizedMessage);
            }

            OnFinished();
        }

        // Public methods -----------------------------------------------------

        public DeleteWithPlanOperationViewModel(IDialogService dialogService,
            IMessagingService messagingService,
            IFilesystemOperator filesystemOperator,
            DeleteConfigurationModel configuration,
            IReadOnlyList<Item> selectedItems)
            : base(dialogService,
                  messagingService,
                  filesystemOperator,
                  selectedItems,
                  configuration)
        {
            this.worker = new DeleteWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.ProgressChanged += HandleWorkerProgressChanged;
            worker.RunWorkerCompleted += HandleWorkerRunWorkerCompleted;
        }

        public override void Cancel()
        {
            worker.CancelAsync();
        }

        public override void Run()
        {
            Address = filesystemOperator.CurrentPath;

            ProgressIndeterminate = false;

            var input = new DeleteWorkerInput(filesystemOperator,
                configuration,
                selectedItems);

            worker.RunWorkerAsync(input);
        }
    }
}

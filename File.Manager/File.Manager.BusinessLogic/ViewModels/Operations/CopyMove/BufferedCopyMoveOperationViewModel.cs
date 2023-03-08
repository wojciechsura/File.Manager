using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Items;
using File.Manager.API.Filesystem.Models.Items.Listing;
using File.Manager.API.Filesystem.Models.Items.Operator;
using File.Manager.API.Filesystem.Models.Items.Plan;
using File.Manager.API.Tools;
using File.Manager.BusinessLogic.Dependencies;
using File.Manager.BusinessLogic.Models.Dialogs.CopyMoveConfiguration;
using File.Manager.BusinessLogic.Services.Dialogs;
using File.Manager.BusinessLogic.Services.Messaging;
using File.Manager.BusinessLogic.Types;
using File.Manager.Resources.Operations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.ViewModels.Operations.CopyMove
{
    public class BufferedCopyMoveOperationViewModel : BaseCopyMoveOperationViewModel
    {
        // Private types ------------------------------------------------------

        // Worker

        private sealed class CopyMoveWorkerContext : BaseCopyMoveWorkerContext
        {
            public CopyMoveWorkerContext(CopyMoveConfigurationModel configuration) : base(configuration)
            {

            }
        }

        private sealed class CopyMoveWorker : BaseCopyMoveWorker<CopyMoveWorkerContext>
        {
            // Private constants ----------------------------------------------

            private const long BUFFER_SIZE = 1024 * 1024;

            // Private fields -------------------------------------------------

            private DateTime startTime;

            // Protected methods ----------------------------------------------

            protected override (bool exit, CopyMoveWorkerResult result) CopyFile(CopyMoveWorkerContext context,
                IFileInfo operatorFile,
                Stream sourceStream,
                Stream destinationStream,
                byte[] buffer,
                ref bool cancelled)
            {
                int bytesRead;
                long bytesCopied = 0;

                do
                {
                    if (CancellationPending)
                    {
                        cancelled = true;
                        return (true, new CancelledCopyMoveWorkerResult());
                    }

                    // Copying

                    bytesRead = sourceStream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                        destinationStream.Write(buffer, 0, bytesRead);

                    bytesCopied += bytesRead;

                    // Elapsed

                    (TimeSpan elapsed, string elapsedString) = EvalElapsed(startTime);

                    // Transfer

                    long totalBytesCopied = context.CopiedSize + bytesCopied;
                    string transfer = (long)elapsed.TotalSeconds > 0 ? $" ({SizeTools.BytesToHumanReadable(totalBytesCopied / (long)elapsed.TotalSeconds)}ps)" : "";

                    // Progress description to display

                    string partialDescription = string.Format(Strings.CopyMove_Info_PartialDescription,
                        context.CopiedFiles,
                        SizeTools.BytesToHumanReadable(context.CopiedSize + bytesCopied),
                        elapsedString,
                        transfer);

                    ReportProgress(0, new CopyMoveProgress(0,
                        partialDescription,
                        (int)(bytesCopied * 100 / operatorFile.Size),
                        operatorFile.Name));
                }
                while (bytesRead > 0);

                return (false, null);
            }

            protected override (bool exit, CopyMoveWorkerResult result) RetrieveFolderContents(CopyMoveWorkerContext context,
                IFolderInfo folderInfo,
                IFilesystemOperator sourceFolderOperator,
                IFilesystemOperator destinationFolderOperator,
                ref IReadOnlyList<IBaseItemInfo> items)
            {
                items = sourceFolderOperator.List(null, context.Configuration.FileMask);
                if (items == null)
                {
                    var resolution = GetResolutionFor(CopyMoveProblemKind.CannotListFolderContents,
                        sourceFolderOperator.CurrentPath,
                        destinationFolderOperator.CurrentPath,
                        folderInfo.Name);

                    switch (resolution)
                    {
                        case GenericCopyMoveProblemResolution.Skip:
                            return (true, null);
                        case GenericCopyMoveProblemResolution.Abort:
                            return (true, new AbortedCopyMoveWorkerResult());
                        default:
                            throw new InvalidOperationException("Invalid resolution!");
                    }
                }
                
                return (false, null);
            }


            protected override void OnDoWork(DoWorkEventArgs e)
            {
                startTime = DateTime.Now;

                var input = (CopyMoveWorkerInput)e.Argument;

                var items = input.SourceOperator.List(input.SelectedItems, input.Configuration.FileMask);

                var context = new CopyMoveWorkerContext(input.Configuration);

                var result = ProcessItems(context, 
                    items, 
                    input.OperationType, 
                    input.SourceOperator, 
                    input.DestinationOperator,
                    true);

                if (result != null)
                    e.Result = result;
                else
                    e.Result = new SuccessCopyMoveWorkerResult();
            }

            // Public methods -------------------------------------------------

            public CopyMoveWorker()
            {

            }
        }

        // Private fields -----------------------------------------------------

        private readonly CopyMoveWorker worker;

        // Private methods ----------------------------------------------------

        private void HandleWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is UserQuestionRequestProgress userQuestion)
            {
                (bool result, SingleCopyMoveProblemResolution resolution) = dialogService.ShowUserDecisionDialog(userQuestion.AvailableResolutions, userQuestion.Header);
                if (result)
                    worker.UserDecision = resolution;
                else
                    worker.UserDecision = SingleCopyMoveProblemResolution.Abort;

                worker.UserDecisionSemaphore.Release();
            }
            else if (e.UserState is CopyMoveProgress progress)
            {
                Progress = progress.Progress;
                ProgressDescription = progress.Description;
                FileProgress = progress.FileProgress;
                FileProgressDescription = progress.FileDescription;
            }
        }

        // Public methods -----------------------------------------------------

        public BufferedCopyMoveOperationViewModel(IDialogService dialogService,
            IMessagingService messagingService,
            DataTransferOperationType operationType,
            IFilesystemOperator sourceOperator,
            IFilesystemOperator destinationOperator,
            CopyMoveConfigurationModel configuration,
            IReadOnlyList<Item> selectedItems)
            : base(dialogService, 
                  messagingService, 
                  sourceOperator, 
                  destinationOperator,
                  selectedItems,
                  configuration,
                  operationType)
        {
            this.worker = new CopyMoveWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.ProgressChanged += HandleWorkerProgressChanged;
            worker.RunWorkerCompleted += HandleWorkerRunWorkerCompleted;
        }

        private void HandleWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is CriticalFailureCopyMoveWorkerResult critical)
            {
                messagingService.ShowError(critical.LocalizedMessage);
            }

            OnFinished();
        }

        public override void Cancel()
        {
            worker.CancelAsync();
        }

        public override void Run()
        {
            Title = operationType switch
            {
                DataTransferOperationType.Copy => Strings.CopyMove_Title_CopyingFiles,
                DataTransferOperationType.Move => Strings.CopyMove_Title_MovingFiles,
                _ => throw new InvalidOperationException("Unsupported DataTransferOperationType!")
            };

            FromAddress = sourceOperator.CurrentPath;
            ToAddress = destinationOperator.CurrentPath;

            ProgressIndeterminate = true;

            var input = new CopyMoveWorkerInput(operationType,
                sourceOperator,
                destinationOperator,
                configuration,
                selectedItems);

            worker.RunWorkerAsync(input);
        }
    }
}

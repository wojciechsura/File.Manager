using BindingEnums;
using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Items;
using File.Manager.API.Filesystem.Models.Items.Listing;
using File.Manager.API.Filesystem.Models.Items.Operator;
using File.Manager.API.Filesystem.Models.Items.Plan;
using File.Manager.API.Tools;
using File.Manager.BusinessLogic.Attributes;
using File.Manager.BusinessLogic.Models.Dialogs.CopyMoveConfiguration;
using File.Manager.BusinessLogic.Services.Dialogs;
using File.Manager.BusinessLogic.Services.Messaging;
using File.Manager.BusinessLogic.Types;
using File.Manager.Common.Helpers;
using File.Manager.Resources.Operations;
using SmartFormat.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Documents;

namespace File.Manager.BusinessLogic.ViewModels.Operations.CopyMove
{
    public class BufferedCopyMoveWithPlanOperationViewModel : BaseCopyMoveOperationViewModel
    {
        // Private types ------------------------------------------------------

        // Worker

        private sealed class CopyMoveWorkerContext : BaseCopyMoveWorkerContext
        {
            public CopyMoveWorkerContext(CopyMoveConfigurationModel configuration, long totalSize, int totalFiles)
                : base(configuration)
            {
                TotalSize = totalSize;
                TotalFiles = totalFiles;
            }

            public long TotalSize { get; }
            public int TotalFiles { get; }
        }

        private sealed class CopyMoveWorker : BaseCopyMoveWorker<CopyMoveWorkerContext>
        {
            // Private fields -------------------------------------------------

            private DateTime startTime;

            // Protected methods ----------------------------------------------

            protected override (bool exit, CopyMoveWorkerResult result) CopyFile(CopyMoveWorkerContext context,
                IFileInfo fileInfo,
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

                    TimeSpan elapsed = DateTime.Now - startTime;
                    string elapsedString = GetTimeSpanString(elapsed);

                    // Estimated left

                    long totalBytesCopied = context.CopiedSize + bytesCopied;
                    var millisecondsLeft = totalBytesCopied switch
                    {
                        > 0 => (long)elapsed.TotalMilliseconds * (context.TotalSize - totalBytesCopied) / totalBytesCopied,
                        _ => 0,
                    };

                    TimeSpan left = TimeSpan.FromMilliseconds(millisecondsLeft);
                    string leftString = GetTimeSpanString(left);

                    // Transfer speed

                    string transfer = (long)elapsed.TotalSeconds > 0 ? $" ({SizeTools.BytesToHumanReadable(totalBytesCopied / (long)elapsed.TotalSeconds)}ps)" : "";

                    // Progress description to display

                    string totalDescription = string.Format(Strings.CopyMove_Info_TotalDescription,
                        context.CopiedFiles,
                        context.TotalFiles,
                        SizeTools.BytesToHumanReadable(context.CopiedSize + bytesCopied),
                        SizeTools.BytesToHumanReadable(context.TotalSize),
                        elapsedString,
                        leftString,
                        transfer);

                    ReportProgress(0, new CopyMoveProgress((int)((context.CopiedSize + bytesCopied) * 100 / context.TotalSize),
                        totalDescription,
                        (int)(fileInfo.Size > 0 ? (bytesCopied * 100 / fileInfo.Size) : 100),
                        fileInfo.Name));
                }
                while (bytesRead > 0);

                return (false, null);
            }

            protected override (bool exit, CopyMoveWorkerResult result) RetrieveFolderContents(CopyMoveWorkerContext context,
                IFolderInfo folderInfo,
                FilesystemOperator sourceFolderOperator,
                FilesystemOperator destinationFolderOperator,
                ref IReadOnlyList<IBaseItemInfo> items)
            {
                items = (PlanFolder)folderInfo;
                return (false, null);
            }

            protected override void OnDoWork(DoWorkEventArgs e)
            {
                startTime = DateTime.Now;

                var input = (CopyMoveWorkerInput)e.Argument;
                var configuration = input.Configuration;

                // 1. Plan

                var plan = input.SourceOperator.BuildOperationPlanFromSelection(input.SelectedItems, input.Configuration.FileMask);

                // 2. Evaluate totals

                (long totalSize, int totalFiles) = EvaluatePlanTotalsRecursive(plan);
                var context = new CopyMoveWorkerContext(configuration, totalSize, totalFiles);

                // 3. Copying/moving files

                var result = ProcessItems(context, plan, input.OperationType, input.SourceOperator, input.DestinationOperator, true);
                
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

        // Public methods -----------------------------------------------------

        public BufferedCopyMoveWithPlanOperationViewModel(IDialogService dialogService,
            IMessagingService messagingService,
            DataTransferOperationType operationType,
            FilesystemOperator sourceOperator,
            FilesystemOperator destinationOperator,
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

            ProgressIndeterminate = false;

            var input = new CopyMoveWorkerInput(operationType,
                sourceOperator,
                destinationOperator,
                configuration,
                selectedItems);

            worker.RunWorkerAsync(input);
        }
    }
}

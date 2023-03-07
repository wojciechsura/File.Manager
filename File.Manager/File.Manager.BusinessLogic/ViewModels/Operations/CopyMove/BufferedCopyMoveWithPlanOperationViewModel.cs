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

        // Input

        private sealed class CopyMoveWorkerInput
        {
            public CopyMoveWorkerInput(DataTransferOperationType operationType, IFilesystemOperator sourceOperator, IFilesystemOperator destinationOperator, CopyMoveConfigurationModel configuration, IReadOnlyList<Item> selectedItems)
            {
                OperationType = operationType;
                SourceOperator = sourceOperator;
                DestinationOperator = destinationOperator;
                Configuration = configuration;
                SelectedItems = selectedItems;
            }

            public DataTransferOperationType OperationType { get; }
            public IFilesystemOperator SourceOperator { get; }
            public IFilesystemOperator DestinationOperator { get; }
            public CopyMoveConfigurationModel Configuration { get; }
            public IReadOnlyList<Item> SelectedItems { get; }
        }

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
            private CopyMoveConfigurationModel configuration;

            // Private methods ------------------------------------------------

            private CopyMoveWorkerResult ProcessFile(CopyMoveWorkerContext context,
                PlanFile planFile,
                DataTransferOperationType operationType,
                IFilesystemOperator sourceOperator,
                IFilesystemOperator destinationOperator)
            {
                try
                {
                    bool exit;
                    CopyMoveWorkerResult result;

                    // Check if source file exists

                    (exit, result) = EnsureSourceFileExists(planFile, sourceOperator, destinationOperator);
                    if (exit)
                        return result;

                    // Ask about overwriting existing file

                    string targetName = planFile.Name;
                    (exit, result) = EnsureDestinationFileDoesNotExist(planFile, sourceOperator, destinationOperator, ref targetName);
                    if (exit)
                        return result;

                    // Ask about overwriting readonly file

                    (exit, result) = CheckForOverwritingReadOnlyFile(planFile, sourceOperator, destinationOperator, ref targetName);
                    if (exit)
                        return result;

                    // Ask about overwriting system file

                    (exit, result) = CheckForOverwritingSystemFile(planFile, sourceOperator, destinationOperator, ref targetName);

                    // Get the source stream

                    Stream sourceStream = null;
                    (exit, result) = OpenSourceFile(planFile, sourceOperator, destinationOperator, ref sourceStream);
                    if (exit)
                        return result;

                    // Get the destination stream

                    Stream destinationStream = null;
                    (exit, result) = OpenDestinationFile(planFile, sourceOperator, destinationOperator, targetName, ref destinationStream);
                    if (exit)
                        return result;

                    // Start copying
                    
                    bool cancelled = false;

                    try
                    {
                        (exit, result) = CopyFile(context, planFile, sourceStream, destinationStream, buffer, ref cancelled);
                        if (exit)
                            return result;

                        (exit, result) = CopyAttributes(planFile, sourceOperator, destinationOperator, targetName);
                        if (exit)
                            return result;

                        if (operationType == DataTransferOperationType.Move)
                        {
                            (exit, result) = DeleteSourceFile(planFile, sourceOperator, destinationOperator);
                            if (exit)
                                return result;
                        }
                    }
                    catch
                    {
                        destinationStream?.Dispose();
                        destinationStream = null;

                        // Try to delete partially-copied file
                        destinationOperator.DeleteFile(targetName);

                        var resolution = GetResolutionFor(ProcessingProblemKind.FailedToCopyFile,
                            sourceOperator.CurrentPath,
                            destinationOperator.CurrentPath,
                            planFile.Name);

                        switch (resolution)
                        {
                            case GenericProblemResolution.Skip:
                                return null;
                            case GenericProblemResolution.Abort:
                                return new AbortedCopyMoveWorkerResult();
                            default:
                                throw new InvalidOperationException("Invalid resolution!");
                        }
                    }
                    finally
                    {
                        if (cancelled)
                        {
                            // Try to remove file, which was not copied
                            destinationOperator.DeleteFile(targetName);
                        }

                        sourceStream?.Dispose();
                        sourceStream = null;
                        destinationStream?.Dispose();
                        destinationStream = null;
                    }
                }
                finally
                {
                    context.CopiedSize += planFile.Size;
                    context.CopiedFiles++;
                }

                return null;
            }

            private CopyMoveWorkerResult ProcessFolder(CopyMoveWorkerContext context, 
                PlanFolder planFolder,
                DataTransferOperationType operationType,
                IFilesystemOperator sourceOperator,
                IFilesystemOperator destinationOperator)
            {
                bool exit;
                CopyMoveWorkerResult result;

                // Create folder in remote location
                (exit, result) = CreateDestinationFolder(planFolder, sourceOperator, destinationOperator);
                if (exit)
                    return result;

                IFilesystemOperator sourceFolderOperator = null;

                (exit, result) = EnterSourceFolder(planFolder, sourceOperator, destinationOperator, ref sourceFolderOperator);
                if (exit)
                    return result;

                IFilesystemOperator destinationFolderOperator = null;

                (exit, result) = EnterDestinationFolder(planFolder, sourceOperator, destinationOperator, ref destinationFolderOperator);
                if (exit)
                    return result;

                return ProcessItems(context, planFolder, operationType, sourceFolderOperator, destinationFolderOperator);
            }

            private CopyMoveWorkerResult ProcessItems(CopyMoveWorkerContext context, 
                IReadOnlyList<BasePlanItem> items, 
                DataTransferOperationType operationType, 
                IFilesystemOperator sourceOperator, 
                IFilesystemOperator destinationOperator)
            {
                foreach (var item in items)
                {
                    if (item is PlanFolder planFolder)
                    {
                        var result = ProcessFolder(context, planFolder, operationType, sourceOperator, destinationOperator);
                        if (result != null)
                            return result;
                    }
                    else if (item is PlanFile planFile)
                    {
                        var result = ProcessFile(context,planFile, operationType, sourceOperator, destinationOperator);
                        if (result != null)
                            return result;
                    }
                    else
                        throw new InvalidOperationException("Invalid plan item!");
                }

                return null;
            }

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
                    string elapsedString = elapsed.Days > 0 ? elapsed.ToString("d'd'\\, hh\\:mm\\:ss") : elapsed.ToString("hh\\:mm\\:ss");

                    // Estimated left

                    long totalBytesCopied = context.CopiedSize + bytesCopied;
                    var millisecondsLeft = totalBytesCopied switch
                    {
                        > 0 => (long)elapsed.TotalMilliseconds * (context.TotalSize - (context.CopiedSize + totalBytesCopied)) / totalBytesCopied,
                        _ => 0,
                    };

                    TimeSpan left = TimeSpan.FromMilliseconds(millisecondsLeft);
                    string leftString = left.Days > 0 ? left.ToString("d'd'\\, hh\\:mm\\:ss") : left.ToString("hh\\:mm\\:ss");

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
                        (int)(bytesCopied * 100 / fileInfo.Size),
                        fileInfo.Name));
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
                items = (PlanFolder)folderInfo;
                return (false, null);
            }

            protected override void OnDoWork(DoWorkEventArgs e)
            {
                startTime = DateTime.Now;

                var input = (CopyMoveWorkerInput)e.Argument;
                configuration = input.Configuration;

                // 1. Plan

                var plan = input.SourceOperator.BuildOperationPlanFromSelection(input.SelectedItems, input.Configuration.FileMask);

                // 2. Evaluate totals

                (long totalSize, int totalFiles) = EvaluatePlanTotalsRecursive(plan);
                var context = new CopyMoveWorkerContext(configuration, totalSize, totalFiles);

                // 3. Copying/moving files

                var result = ProcessItems(context, plan, input.OperationType, input.SourceOperator, input.DestinationOperator);
                
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
                (bool result, SingleProblemResolution resolution) = dialogService.ShowUserDecisionDialog(userQuestion.AvailableResolutions, userQuestion.Header);
                if (result)
                    worker.UserDecision = resolution;
                else
                    worker.UserDecision = SingleProblemResolution.Abort;

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

        public BufferedCopyMoveWithPlanOperationViewModel(IDialogService dialogService,
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

        public override void Run()
        {
            base.Run();

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

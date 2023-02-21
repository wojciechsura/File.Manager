using BindingEnums;
using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Items;
using File.Manager.API.Filesystem.Models.Plan;
using File.Manager.BusinessLogic.Attributes;
using File.Manager.BusinessLogic.Models.Dialogs.CopyMoveConfiguration;
using File.Manager.BusinessLogic.Services.Dialogs;
using File.Manager.BusinessLogic.Types;
using File.Manager.Common.Helpers;
using File.Manager.Resources.Operations;
using SmartFormat.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace File.Manager.BusinessLogic.ViewModels.Operations.CopyMove
{
    public class BufferedCopyMoveWithPlanOperationViewModel : BaseOperationViewModel
    {
        // Private types ------------------------------------------------------

        // Input

        private class CopyMoveWorkerInput
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

        // Results

        private abstract class CopyMoveWorkerResult
        {

        }

        private class AbortedCopyMoveWorkerResult : CopyMoveWorkerResult
        {
            public AbortedCopyMoveWorkerResult()
            {

            }
        }

        private class CriticalFailureCopyMoveWorkerResult : CopyMoveWorkerResult
        {
            public CriticalFailureCopyMoveWorkerResult(string localizedMessage)
            {
                LocalizedMessage = localizedMessage;
            }

            public string LocalizedMessage { get; }
        }

        private class SuccessCopyMoveWorkerResult : CopyMoveWorkerResult
        {
            public SuccessCopyMoveWorkerResult()
            {

            }
        }

        // Progress

        public class UserQuestionRequestProgress
        {
            public UserQuestionRequestProgress(SingleProblemResolution[] availableResolutions, string header)
            {
                AvailableResolutions = availableResolutions;
                Header = header;
            }

            public SingleProblemResolution[] AvailableResolutions { get; }
            public string Header { get; }
        }

        // Worker

        private class CopyMoveWorker : BackgroundWorker
        {
            private const long BUFFER_SIZE = 1024 * 1024;

            private enum ProcessingProblemKind
            {
                [LocalizedDescription(nameof(Strings.CopyMove_Question_FailedToCreateDestinationFolder), typeof(Strings))]
                [AvailableResolutions(SingleProblemResolution.Skip, SingleProblemResolution.SkipAll, SingleProblemResolution.Abort)]
                CannotCreateDestinationFolder,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_FailedToEnterDestinationFolder), typeof(Strings))]
                [AvailableResolutions(SingleProblemResolution.Skip, SingleProblemResolution.SkipAll, SingleProblemResolution.Abort)]
                CannotEnterDestinationFolder,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_FailedToEnterSourceFolder), typeof(Strings))]
                [AvailableResolutions(SingleProblemResolution.Skip, SingleProblemResolution.SkipAll, SingleProblemResolution.Abort)]
                CannotEnterSourceFolder,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_SourceFileDoesNotExist), typeof(Strings))]
                [AvailableResolutions(SingleProblemResolution.Skip, SingleProblemResolution.SkipAll, SingleProblemResolution.Abort)]
                SourceFileDoesNotExist,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_DestinationFileAlreadyExists), typeof(Strings))]
                [AvailableResolutions(SingleProblemResolution.Skip, SingleProblemResolution.SkipAll,
                    SingleProblemResolution.Overwrite, SingleProblemResolution.OverwriteAll,
                    SingleProblemResolution.Rename, SingleProblemResolution.RenameAll,
                    SingleProblemResolution.Abort)]
                DestinationFileAlreadyExists,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_DestinationFileIsReadOnly), typeof(Strings))]
                [AvailableResolutions(SingleProblemResolution.Skip, SingleProblemResolution.SkipAll,
                    SingleProblemResolution.Overwrite, SingleProblemResolution.OverwriteAll,
                    SingleProblemResolution.Rename, SingleProblemResolution.RenameAll,
                    SingleProblemResolution.Abort)]
                DestinationFileIsReadOnly,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_DestinationFileIsSystem), typeof(Strings))]
                [AvailableResolutions(SingleProblemResolution.Skip, SingleProblemResolution.SkipAll,
                    SingleProblemResolution.Overwrite, SingleProblemResolution.OverwriteAll,
                    SingleProblemResolution.Rename, SingleProblemResolution.RenameAll,
                    SingleProblemResolution.Abort)]
                DestinationFileIsSystem,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_CannotOpenSourceFile), typeof(Strings))]
                [AvailableResolutions(SingleProblemResolution.Skip, SingleProblemResolution.SkipAll, SingleProblemResolution.Abort)]
                CannotOpenSourceFile,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_CannotOpenDestinationFile), typeof(Strings))]
                [AvailableResolutions(SingleProblemResolution.Skip, SingleProblemResolution.SkipAll, SingleProblemResolution.Abort)]
                CannotOpenDestinationFile,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_FailedToChangeFileAttributes), typeof(Strings))]
                [AvailableResolutions(SingleProblemResolution.Skip, SingleProblemResolution.SkipAll, SingleProblemResolution.Abort)]
                FailedToChangeFileAttributes,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_FailedToCopyFile), typeof(Strings))]
                [AvailableResolutions(SingleProblemResolution.Skip, SingleProblemResolution.SkipAll, SingleProblemResolution.Abort)]
                FailedToCopyFile,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_FailedToRemoveSourceFile), typeof(Strings))]
                [AvailableResolutions(SingleProblemResolution.Skip, SingleProblemResolution.SkipAll, SingleProblemResolution.Abort)]
                FailedToDeleteSourceFile
            }

            private readonly SemaphoreSlim userDecisionSemaphore;
            private Dictionary<ProcessingProblemKind, GenericProblemResolution> problemResolutions = new();

            private GenericProblemResolution GetResolutionFor(ProcessingProblemKind problemKind,
                string sourceAddress,
                string destinationAddress,
                string name)
            {
                if (problemResolutions.TryGetValue(problemKind, out GenericProblemResolution resolution) && resolution != GenericProblemResolution.Ask)
                    return resolution;

                LocalizedDescriptionAttribute localizedDescription = problemKind.GetAttribute<LocalizedDescriptionAttribute>();
                string header = string.Format(localizedDescription.Description, sourceAddress, destinationAddress, name);

                AvailableResolutionsAttribute availableResolutions = problemKind.GetAttribute<AvailableResolutionsAttribute>();

                var progress = new UserQuestionRequestProgress(availableResolutions.AvailableResolutions, header);
                ReportProgress(0, progress);

                UserDecisionSemaphore.Wait();

                switch (UserDecision)
                {
                    case SingleProblemResolution.Skip:
                        return GenericProblemResolution.Skip;
                    case SingleProblemResolution.SkipAll:
                        problemResolutions[problemKind] = GenericProblemResolution.Skip;
                        return GenericProblemResolution.Skip;
                    case SingleProblemResolution.Overwrite:
                        return GenericProblemResolution.Overwrite;
                    case SingleProblemResolution.OverwriteAll:
                        problemResolutions[problemKind] = GenericProblemResolution.Overwrite;
                        return GenericProblemResolution.Overwrite;
                    case SingleProblemResolution.Rename:
                        return GenericProblemResolution.Rename;
                    case SingleProblemResolution.RenameAll:
                        problemResolutions[problemKind] = GenericProblemResolution.Rename;
                        return GenericProblemResolution.Rename;
                    case SingleProblemResolution.Abort:
                        return GenericProblemResolution.Abort;                            
                    default:
                        throw new InvalidOperationException("Unsupported problem resolution!");
                }
            }

            private CopyMoveWorkerResult ProcessFile(PlanFile planFile, CopyMoveWorkerInput input)
            {
                static string FindAvailableName(PlanFile planFile, CopyMoveWorkerInput input)
                {
                    string targetName;
                    long i = 1;
                    string name = System.IO.Path.GetFileNameWithoutExtension(planFile.Name);
                    string extension = System.IO.Path.GetExtension(planFile.Name);

                    while (input.DestinationOperator.FileExists($"{name} ({i}){extension}"))
                        i++;

                    targetName = $"{name} ({i}){extension}";
                    return targetName;
                }

                // Check if source file exists

                if (!input.SourceOperator.FileExists(planFile.Name))
                {
                    var resolution = GetResolutionFor(ProcessingProblemKind.SourceFileDoesNotExist,
                        input.SourceOperator.CurrentPath,
                        input.DestinationOperator.CurrentPath,
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

                // Ask about overwriting existing file

                string targetName = planFile.Name;

                if (input.DestinationOperator.FileExists(planFile.Name))
                {
                    var resolution = GetResolutionFor(ProcessingProblemKind.DestinationFileAlreadyExists,
                        input.SourceOperator.CurrentPath,
                        input.DestinationOperator.CurrentPath,
                        planFile.Name);

                    switch (resolution)
                    {
                        case GenericProblemResolution.Skip:
                            return null;
                        case GenericProblemResolution.Abort:
                            return new AbortedCopyMoveWorkerResult();
                        case GenericProblemResolution.Overwrite:
                            break;
                        case GenericProblemResolution.Rename:
                            targetName = FindAvailableName(planFile, input);
                            break;
                        default:
                            throw new InvalidOperationException("Invalid resolution!");
                    }
                }

                // Ask about overwriting readonly file

                if (input.DestinationOperator.FileExists(targetName))
                {
                    var attributes = input.DestinationOperator.GetFileAttributes(targetName);

                    if (attributes.HasFlag(System.IO.FileAttributes.ReadOnly))
                    {
                        var resolution = GetResolutionFor(ProcessingProblemKind.DestinationFileIsReadOnly,
                            input.SourceOperator.CurrentPath,
                            input.DestinationOperator.CurrentPath,
                            targetName);

                        switch (resolution)
                        {
                            case GenericProblemResolution.Skip:
                                return null;
                            case GenericProblemResolution.Abort:
                                return new AbortedCopyMoveWorkerResult();
                            case GenericProblemResolution.Rename:
                                targetName = FindAvailableName(planFile, input);
                                break;
                            case GenericProblemResolution.Overwrite:
                                {
                                    attributes &= ~FileAttributes.ReadOnly;

                                    if (!input.DestinationOperator.SetFileAttributes(targetName, attributes))
                                    {
                                        var innerResolution = GetResolutionFor(ProcessingProblemKind.FailedToChangeFileAttributes,
                                            input.SourceOperator.CurrentPath,
                                            input.DestinationOperator.CurrentPath,
                                            targetName);

                                        switch (innerResolution)
                                        {
                                            case GenericProblemResolution.Skip:
                                                return null;
                                            case GenericProblemResolution.Abort:
                                                return new AbortedCopyMoveWorkerResult();
                                            default:
                                                throw new InvalidOperationException("Invalid resolution!");
                                        }
                                    }

                                    break;
                                }
                            default:
                                throw new InvalidOperationException("Invalid resolution!");
                        }
                    }
                }

                // Ask about overwriting system file

                if (input.DestinationOperator.FileExists(targetName))
                {
                    var attributes = input.DestinationOperator.GetFileAttributes(targetName);

                    if (attributes.HasFlag(System.IO.FileAttributes.System))
                    {
                        var resolution = GetResolutionFor(ProcessingProblemKind.DestinationFileIsSystem,
                            input.SourceOperator.CurrentPath,
                            input.DestinationOperator.CurrentPath,
                            targetName);

                        switch (resolution)
                        {
                            case GenericProblemResolution.Skip:
                                return null;
                            case GenericProblemResolution.Abort:
                                return new AbortedCopyMoveWorkerResult();
                            case GenericProblemResolution.Rename:
                                targetName = FindAvailableName(planFile, input);
                                break;
                            case GenericProblemResolution.Overwrite:
                                {
                                    attributes &= ~FileAttributes.System;

                                    if (!input.DestinationOperator.SetFileAttributes(targetName, attributes))
                                    {
                                        var innerResolution = GetResolutionFor(ProcessingProblemKind.FailedToChangeFileAttributes,
                                            input.SourceOperator.CurrentPath,
                                            input.DestinationOperator.CurrentPath,
                                            targetName);

                                        switch (innerResolution)
                                        {
                                            case GenericProblemResolution.Skip:
                                                return null;
                                            case GenericProblemResolution.Abort:
                                                return new AbortedCopyMoveWorkerResult();
                                            default:
                                                throw new InvalidOperationException("Invalid resolution!");
                                        }
                                    }

                                    break;
                                }
                            default:
                                throw new InvalidOperationException("Invalid resolution!");
                        }
                    }
                }

                // Get the source stream

                var sourceStream = input.SourceOperator.OpenFileForReading(planFile.Name);
                if (sourceStream == null)
                {
                    var resolution = GetResolutionFor(ProcessingProblemKind.CannotOpenSourceFile,
                        input.SourceOperator.CurrentPath,
                        input.DestinationOperator.CurrentPath,
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

                // Get the destination stream

                var destinationStream = input.DestinationOperator.OpenFileForWriting(targetName);
                if (destinationStream == null)
                {
                    sourceStream.Dispose();

                    var resolution = GetResolutionFor(ProcessingProblemKind.CannotOpenDestinationFile,
                        input.SourceOperator.CurrentPath,
                        input.DestinationOperator.CurrentPath,
                        targetName);

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

                // Start copying

                byte[] buffer = new byte[BUFFER_SIZE];

                try
                {
                    int bytesRead;

                    do
                    {
                        bytesRead = sourceStream.Read(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                            destinationStream.Write(buffer, 0, bytesRead);
                    }
                    while (bytesRead > 0);
                }
                catch
                {
                    destinationStream?.Dispose();
                    destinationStream = null;

                    // Try to delete partially-copied file
                    input.DestinationOperator.DeleteFile(targetName);

                    var resolution = GetResolutionFor(ProcessingProblemKind.FailedToCopyFile,
                        input.SourceOperator.CurrentPath,
                        input.DestinationOperator.CurrentPath,
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
                    sourceStream?.Dispose();
                    sourceStream = null;
                    destinationStream?.Dispose();
                    destinationStream = null;
                }

                if (input.OperationType == DataTransferOperationType.Move)
                {
                    if (!input.SourceOperator.DeleteFile(planFile.Name))
                    {
                        var resolution = GetResolutionFor(ProcessingProblemKind.FailedToDeleteSourceFile,
                            input.SourceOperator.CurrentPath,
                            input.DestinationOperator.CurrentPath,
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
                }

                return null;
            }

            private CopyMoveWorkerResult ProcessFolder(PlanFolder planFolder, CopyMoveWorkerInput input)
            {
                string currentSourcePath, currentDestinationPath;

                // Create folder in remote location
                if (!input.DestinationOperator.CreateFolder(planFolder.Name))
                {
                    var resolution = GetResolutionFor(ProcessingProblemKind.CannotCreateDestinationFolder,
                        input.SourceOperator.CurrentPath,
                        input.DestinationOperator.CurrentPath,
                        planFolder.Name);

                    switch (resolution)
                    {
                        case GenericProblemResolution.Skip:
                            return null;
                        case GenericProblemResolution.Abort:
                            return new AbortedCopyMoveWorkerResult();
                        default:
                            throw new InvalidOperationException("Invalid problem resolution!");
                    }
                }

                // Enter folder in local location
                if (!input.SourceOperator.EnterFolder(planFolder.Name))
                {
                    var resolution = GetResolutionFor(ProcessingProblemKind.CannotEnterSourceFolder,
                        input.SourceOperator.CurrentPath,
                        input.DestinationOperator.CurrentPath,
                        planFolder.Name);

                    switch (resolution)
                    {
                        case GenericProblemResolution.Skip:
                            return null;
                        case GenericProblemResolution.Abort:
                            return new AbortedCopyMoveWorkerResult();
                        default:
                            throw new InvalidOperationException("Invalid problem resolution!");
                    }
                }

                // Enter folder in remote location
                if (!input.DestinationOperator.EnterFolder(planFolder.Name))
                {
                    currentSourcePath = input.SourceOperator.CurrentPath;

                    if (!input.SourceOperator.ExitFolder())
                    {
                        return new CriticalFailureCopyMoveWorkerResult(String.Format(Strings.CopyMove_Critical_CannotExitSourceFolder, currentSourcePath));
                    }

                    var resolution = GetResolutionFor(ProcessingProblemKind.CannotEnterDestinationFolder,
                        input.SourceOperator.CurrentPath,
                        input.DestinationOperator.CurrentPath,
                        planFolder.Name);

                    switch (resolution)
                    {
                        case GenericProblemResolution.Skip:
                            return null;
                        case GenericProblemResolution.Abort:
                            return new AbortedCopyMoveWorkerResult();
                        default:
                            throw new InvalidOperationException("Invalid problem resolution!");
                    }
                }

                ProcessItems(planFolder, input);

                // Return to parent folder in local location
                currentSourcePath = input.SourceOperator.CurrentPath;
                if (!input.SourceOperator.ExitFolder())
                {
                    return new CriticalFailureCopyMoveWorkerResult(String.Format(Strings.CopyMove_Critical_CannotExitSourceFolder, currentSourcePath));
                }

                currentDestinationPath = input.DestinationOperator.CurrentPath;
                if (!input.DestinationOperator.ExitFolder())
                {
                    return new CriticalFailureCopyMoveWorkerResult(String.Format(Strings.CopyMove_Critical_CannotExitDestinationFolder, currentDestinationPath));
                }

                return null;
            }

            private CopyMoveWorkerResult ProcessItems(IReadOnlyList<BasePlanItem> items, CopyMoveWorkerInput input)
            {
                foreach (var item in items)
                {
                    if (item is PlanFolder planFolder)
                    {
                        var result = ProcessFolder(planFolder, input);
                        if (result != null)
                            return result;
                    }
                    else if (item is PlanFile planFile)
                    {
                        var result = ProcessFile(planFile, input);
                        if (result != null)
                            return result;
                    }
                    else
                        throw new InvalidOperationException("Invalid plan item!");
                }

                return null;
            }

            protected override void OnDoWork(DoWorkEventArgs e)
            {
                var input = (CopyMoveWorkerInput)e.Argument;

                // 1. Plan

                var plan = input.SourceOperator.BuildOperationPlanFromSelection(input.SelectedItems, input.Configuration.FileMask);

                // 2. Copying/moving files

                var result = ProcessItems(plan, input);
                if (result != null)
                    e.Result = result;
                else
                    e.Result = new SuccessCopyMoveWorkerResult();
            }

            public CopyMoveWorker()
            {
                userDecisionSemaphore = new SemaphoreSlim(0, 1);
            }

            public SemaphoreSlim UserDecisionSemaphore => userDecisionSemaphore;

            public SingleProblemResolution? UserDecision { get; set; }
        }

        // Private fields -----------------------------------------------------

        private DataTransferOperationType operationType;
        private IFilesystemOperator sourceOperator;
        private IFilesystemOperator destinationOperator;
        private IReadOnlyList<Item> selectedItems;
        private CopyMoveConfigurationModel configuration;

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
        }

        // Public methods -----------------------------------------------------

        public BufferedCopyMoveWithPlanOperationViewModel(IDialogService dialogService,
            DataTransferOperationType operationType,
            IFilesystemOperator sourceOperator,
            IFilesystemOperator destinationOperator,
            CopyMoveConfigurationModel configuration,
            IReadOnlyList<Item> selectedItems)
            : base(dialogService)
        {
            this.operationType = operationType;
            this.sourceOperator = sourceOperator;
            this.destinationOperator = destinationOperator;
            this.configuration = configuration;
            this.selectedItems = selectedItems;

            this.worker = new CopyMoveWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.ProgressChanged += HandleWorkerProgressChanged;
            worker.RunWorkerCompleted += HandleWorkerRunWorkerCompleted;
        }

        private void HandleWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public override void Run()
        {
            var input = new CopyMoveWorkerInput(operationType,
                sourceOperator,
                destinationOperator,
                configuration,
                selectedItems);
            worker.RunWorkerAsync(input);
        }
    }
}

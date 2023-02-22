using BindingEnums;
using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Items;
using File.Manager.API.Filesystem.Models.Plan;
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

        private class CancelledCopyMoveWorkerResult : CopyMoveWorkerResult
        {
            public CancelledCopyMoveWorkerResult()
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

        public class CopyMoveProgress
        {
            public CopyMoveProgress(int overallProgress, string overallDescription, int detailedProgress, string detailedDescription)
            {
                OverallProgress = overallProgress;
                OverallDescription = overallDescription;
                DetailedProgress = detailedProgress;
                DetailedDescription = detailedDescription;
            }

            public int OverallProgress { get; }
            public string OverallDescription { get; }
            public int DetailedProgress { get; }
            public string DetailedDescription { get; }
        }

        // Worker

        private class CopyMoveWorker : BackgroundWorker
        {
            // Private constants ----------------------------------------------

            private const long BUFFER_SIZE = 1024 * 1024;

            // Private types --------------------------------------------------

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
                FailedToDeleteSourceFile,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_CannotGetFileAttributes), typeof(Strings))]
                [AvailableResolutions(SingleProblemResolution.Skip, SingleProblemResolution.SkipAll, SingleProblemResolution.Abort)]
                CannotGetFileAttributes
            }

            private class CopyMoveWorkerContext
            {
                public CopyMoveWorkerContext(long totalSize, int totalFiles)
                {
                    TotalSize = totalSize;
                    TotalFiles = totalFiles;
                }

                public long CopiedSize { get; set; }
                public int CopiedFiles { get; set; }
                public long TotalSize { get; }
                public int TotalFiles { get; }
            }

            // Private fields -------------------------------------------------

            private readonly SemaphoreSlim userDecisionSemaphore;
            private readonly byte[] buffer = new byte[BUFFER_SIZE];
            private Dictionary<ProcessingProblemKind, GenericProblemResolution> problemResolutions = new();

            // Private methods ------------------------------------------------

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

            private string FindAvailableName(PlanFile planFile, IFilesystemOperator destinationOperator)
            {
                string targetName;
                long i = 1;
                string name = System.IO.Path.GetFileNameWithoutExtension(planFile.Name);
                string extension = System.IO.Path.GetExtension(planFile.Name);

                while (destinationOperator.FileExists($"{name} ({i}){extension}"))
                    i++;

                targetName = $"{name} ({i}){extension}";
                return targetName;
            }

            private (bool exit, CopyMoveWorkerResult result) CheckIfSourceFileExists(PlanFile planFile, 
                IFilesystemOperator sourceOperator, 
                IFilesystemOperator destinationOperator)
            {
                if (!sourceOperator.FileExists(planFile.Name))
                {
                    var resolution = GetResolutionFor(ProcessingProblemKind.SourceFileDoesNotExist,
                        sourceOperator.CurrentPath,
                        destinationOperator.CurrentPath,
                        planFile.Name);

                    switch (resolution)
                    {
                        case GenericProblemResolution.Skip:
                            return (true, null);
                        case GenericProblemResolution.Abort:
                            return (true, new AbortedCopyMoveWorkerResult());
                        default:
                            throw new InvalidOperationException("Invalid resolution!");
                    }
                }

                return (false, null);
            }

            private (bool exit, CopyMoveWorkerResult result) CheckIfDestinationFileExists(PlanFile planFile,
                IFilesystemOperator sourceOperator,
                IFilesystemOperator destinationOperator,
                ref string targetName)
            {
                if (destinationOperator.FileExists(planFile.Name))
                {
                    var resolution = GetResolutionFor(ProcessingProblemKind.DestinationFileAlreadyExists,
                        sourceOperator.CurrentPath,
                        destinationOperator.CurrentPath,
                        planFile.Name);

                    switch (resolution)
                    {
                        case GenericProblemResolution.Skip:
                            return (true, null);
                        case GenericProblemResolution.Abort:
                            return (true, new AbortedCopyMoveWorkerResult());
                        case GenericProblemResolution.Overwrite:
                            break;
                        case GenericProblemResolution.Rename:
                            targetName = FindAvailableName(planFile, destinationOperator);
                            break;
                        default:
                            throw new InvalidOperationException("Invalid resolution!");
                    }
                }

                return (false, null);
            }

            private (bool exit, CopyMoveWorkerResult result) CheckForOverwritingReadOnlyFile(PlanFile planFile,
                IFilesystemOperator sourceOperator,
                IFilesystemOperator destinationOperator,
                ref string targetName)
            {
                if (destinationOperator.FileExists(targetName))
                {
                    var attributes = destinationOperator.GetFileAttributes(targetName);

                    if (attributes == null)
                    {
                        var resolution = GetResolutionFor(ProcessingProblemKind.CannotGetFileAttributes,
                            sourceOperator.CurrentPath,
                            destinationOperator.CurrentPath,
                            targetName);

                        switch (resolution)
                        {
                            case GenericProblemResolution.Skip:
                                return (true, null);
                            case GenericProblemResolution.Abort:
                                return (true, new AbortedCopyMoveWorkerResult());
                            default:
                                throw new InvalidOperationException("Invalid resolution!");
                        }
                    }

                    if (attributes.Value.HasFlag(System.IO.FileAttributes.ReadOnly))
                    {
                        var resolution = GetResolutionFor(ProcessingProblemKind.DestinationFileIsReadOnly,
                            sourceOperator.CurrentPath,
                            destinationOperator.CurrentPath,
                            targetName);

                        switch (resolution)
                        {
                            case GenericProblemResolution.Skip:
                                return (true, null);
                            case GenericProblemResolution.Abort:
                                return (true, new AbortedCopyMoveWorkerResult());
                            case GenericProblemResolution.Rename:
                                targetName = FindAvailableName(planFile, destinationOperator);
                                break;
                            case GenericProblemResolution.Overwrite:
                                {
                                    attributes &= ~FileAttributes.ReadOnly;

                                    if (!destinationOperator.SetFileAttributes(targetName, attributes.Value))
                                    {
                                        var innerResolution = GetResolutionFor(ProcessingProblemKind.FailedToChangeFileAttributes,
                                            sourceOperator.CurrentPath,
                                            destinationOperator.CurrentPath,
                                            targetName);

                                        switch (innerResolution)
                                        {
                                            case GenericProblemResolution.Skip:
                                                return (true, null);
                                            case GenericProblemResolution.Abort:
                                                return (true, new AbortedCopyMoveWorkerResult());
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

                return (false, null);
            }

            private (bool exit, CopyMoveWorkerResult result) CheckForOverwritingSystemFile(PlanFile planFile,
                IFilesystemOperator sourceOperator,
                IFilesystemOperator destinationOperator,
                ref string targetName)
            {
                if (destinationOperator.FileExists(targetName))
                {
                    var attributes = destinationOperator.GetFileAttributes(targetName);

                    if (attributes == null)
                    {
                        var resolution = GetResolutionFor(ProcessingProblemKind.CannotGetFileAttributes,
                            sourceOperator.CurrentPath,
                            destinationOperator.CurrentPath,
                            targetName);

                        switch (resolution)
                        {
                            case GenericProblemResolution.Skip:
                                return (true, null);
                            case GenericProblemResolution.Abort:
                                return (true, new AbortedCopyMoveWorkerResult());
                            default:
                                throw new InvalidOperationException("Invalid resolution!");
                        }
                    }

                    if (attributes.Value.HasFlag(System.IO.FileAttributes.System))
                    {
                        var resolution = GetResolutionFor(ProcessingProblemKind.DestinationFileIsSystem,
                            sourceOperator.CurrentPath,
                            destinationOperator.CurrentPath,
                            targetName);

                        switch (resolution)
                        {
                            case GenericProblemResolution.Skip:
                                return (true, null);
                            case GenericProblemResolution.Abort:
                                return (true, new AbortedCopyMoveWorkerResult());
                            case GenericProblemResolution.Rename:
                                targetName = FindAvailableName(planFile, destinationOperator);
                                break;
                            case GenericProblemResolution.Overwrite:
                                {
                                    attributes &= ~FileAttributes.System;

                                    if (!destinationOperator.SetFileAttributes(targetName, attributes.Value))
                                    {
                                        var innerResolution = GetResolutionFor(ProcessingProblemKind.FailedToChangeFileAttributes,
                                            sourceOperator.CurrentPath,
                                            destinationOperator.CurrentPath,
                                            targetName);

                                        switch (innerResolution)
                                        {
                                            case GenericProblemResolution.Skip:
                                                return (true, null);
                                            case GenericProblemResolution.Abort:
                                                return (true, new AbortedCopyMoveWorkerResult());
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

                return (false, null);
            }

            private (bool exit, CopyMoveWorkerResult result) OpenSourceFile(PlanFile planFile,
                IFilesystemOperator sourceOperator,
                IFilesystemOperator destinationOperator,
                ref Stream sourceStream)
            {
                sourceStream = sourceOperator.OpenFileForReading(planFile.Name);
                if (sourceStream == null)
                {
                    var resolution = GetResolutionFor(ProcessingProblemKind.CannotOpenSourceFile,
                        sourceOperator.CurrentPath,
                        destinationOperator.CurrentPath,
                        planFile.Name);

                    switch (resolution)
                    {
                        case GenericProblemResolution.Skip:
                            return (true, null);
                        case GenericProblemResolution.Abort:
                            return (true, new AbortedCopyMoveWorkerResult());
                        default:
                            throw new InvalidOperationException("Invalid resolution!");
                    }
                }

                return (false, null);
            }

            private (bool exit, CopyMoveWorkerResult result) OpenDestinationFile(PlanFile planFile,
                IFilesystemOperator sourceOperator,
                IFilesystemOperator destinationOperator,
                string targetName,
                ref Stream destinationStream)
            {
                destinationStream = destinationOperator.OpenFileForWriting(targetName);
                if (destinationStream == null)
                {
                    var resolution = GetResolutionFor(ProcessingProblemKind.CannotOpenDestinationFile,
                        sourceOperator.CurrentPath,
                        destinationOperator.CurrentPath,
                        targetName);

                    switch (resolution)
                    {
                        case GenericProblemResolution.Skip:
                            return (true, null);
                        case GenericProblemResolution.Abort:
                            return (true, new AbortedCopyMoveWorkerResult());
                        default:
                            throw new InvalidOperationException("Invalid resolution!");
                    }
                }

                return (false, null);
            }

            private (bool exit, CopyMoveWorkerResult result) DeleteSourceFile(PlanFile planFile, IFilesystemOperator sourceOperator, IFilesystemOperator destinationOperator)
            {
                if (!sourceOperator.DeleteFile(planFile.Name))
                {
                    var resolution = GetResolutionFor(ProcessingProblemKind.FailedToDeleteSourceFile,
                        sourceOperator.CurrentPath,
                        destinationOperator.CurrentPath,
                        planFile.Name);

                    switch (resolution)
                    {
                        case GenericProblemResolution.Skip:
                            return (true, null);
                        case GenericProblemResolution.Abort:
                            return (true, new AbortedCopyMoveWorkerResult());
                        default:
                            throw new InvalidOperationException("Invalid resolution!");
                    }
                }

                return (false, null);
            }

            private (bool exit, CopyMoveWorkerResult result) CopyFile(CopyMoveWorkerContext context,
                PlanFile planFile,
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

                    bytesRead = sourceStream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                        destinationStream.Write(buffer, 0, bytesRead);

                    bytesCopied += bytesRead;

                    string totalDescription = String.Format(Strings.CopyMove_Info_TotalDescription,
                        context.CopiedFiles,
                        context.TotalFiles,
                        context.CopiedSize + bytesCopied,
                        context.TotalSize);

                    ReportProgress(0, new CopyMoveProgress((int)((context.CopiedSize + bytesCopied) * 100 / context.TotalSize),
                        totalDescription,
                        100,
                        planFile.Name));
                }
                while (bytesRead > 0);

                return (false, null);
            }

            private (bool exit, CopyMoveWorkerResult result) CreateDestinationFolder(PlanFolder planFolder,
                IFilesystemOperator sourceOperator,
                IFilesystemOperator destinationOperator)
            {
                if (!destinationOperator.CreateFolder(planFolder.Name))
                {
                    var resolution = GetResolutionFor(ProcessingProblemKind.CannotCreateDestinationFolder,
                        sourceOperator.CurrentPath,
                        destinationOperator.CurrentPath,
                        planFolder.Name);

                    switch (resolution)
                    {
                        case GenericProblemResolution.Skip:
                            return (true, null);
                        case GenericProblemResolution.Abort:
                            return (true, new AbortedCopyMoveWorkerResult());
                        default:
                            throw new InvalidOperationException("Invalid problem resolution!");
                    }
                }

                return (false, null);
            }

            private (bool exit, CopyMoveWorkerResult result) EnterSourceFolder(PlanFolder planFolder,
                IFilesystemOperator sourceOperator,
                IFilesystemOperator destinationOperator,
                ref IFilesystemOperator sourceFolderOperator)
            {
                sourceFolderOperator = sourceOperator.EnterFolder(planFolder.Name);

                // Enter folder in local location
                if (sourceFolderOperator == null)
                {
                    var resolution = GetResolutionFor(ProcessingProblemKind.CannotEnterSourceFolder,
                        sourceOperator.CurrentPath,
                        destinationOperator.CurrentPath,
                        planFolder.Name);

                    switch (resolution)
                    {
                        case GenericProblemResolution.Skip:
                            return (true, null);
                        case GenericProblemResolution.Abort:
                            return (true, new AbortedCopyMoveWorkerResult());
                        default:
                            throw new InvalidOperationException("Invalid problem resolution!");
                    }
                }

                return (false, null);
            }

            private (bool exit, CopyMoveWorkerResult result) EnterDestinationFolder(PlanFolder planFolder,
                IFilesystemOperator sourceOperator,
                IFilesystemOperator destinationOperator,
                ref IFilesystemOperator destinationFolderOperator)
            {
                destinationFolderOperator = destinationOperator.EnterFolder(planFolder.Name);

                // Enter folder in remote location
                if (destinationFolderOperator == null)
                {
                    var resolution = GetResolutionFor(ProcessingProblemKind.CannotEnterDestinationFolder,
                        sourceOperator.CurrentPath,
                        destinationOperator.CurrentPath,
                        planFolder.Name);

                    switch (resolution)
                    {
                        case GenericProblemResolution.Skip:
                            return (true, null);
                        case GenericProblemResolution.Abort:
                            return (true, new AbortedCopyMoveWorkerResult());
                        default:
                            throw new InvalidOperationException("Invalid problem resolution!");
                    }
                }

                return (false, null);
            }

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

                    (exit, result) = CheckIfSourceFileExists(planFile, sourceOperator, destinationOperator);
                    if (exit)
                        return result;

                    // Ask about overwriting existing file

                    string targetName = planFile.Name;
                    (exit, result) = CheckIfDestinationFileExists(planFile, sourceOperator, destinationOperator, ref targetName);
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

                    string totalDescription = String.Format(Strings.CopyMove_Info_TotalDescription,
                        context.CopiedFiles,
                        context.TotalFiles,
                        context.CopiedSize,
                        context.TotalSize);

                    ReportProgress(0, new CopyMoveProgress((int)(context.CopiedSize * 100 / context.TotalSize),
                        totalDescription,
                        100,
                        string.Empty));
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

                ProcessItems(context, planFolder, operationType, sourceFolderOperator, destinationFolderOperator);

                return null;
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

            private (long totalSize, int totalFiles) EvaluateTotalsRecursive(IReadOnlyList<BasePlanItem> items)
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
                                (long folderSize, int folderFiles) = EvaluateTotalsRecursive(planFolder);
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

            // Protected methods ----------------------------------------------

            protected override void OnDoWork(DoWorkEventArgs e)
            {
                var input = (CopyMoveWorkerInput)e.Argument;

                // 1. Plan

                var plan = input.SourceOperator.BuildOperationPlanFromSelection(input.SelectedItems, input.Configuration.FileMask);

                // 2. Evaluate totals

                (long totalSize, int totalFiles) = EvaluateTotalsRecursive(plan);
                var context = new CopyMoveWorkerContext(totalSize, totalFiles);

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
            else if (e.UserState is CopyMoveProgress progress)
            {
                OverallProgress = progress.OverallProgress;
                OverallProgressDescription = progress.OverallDescription;
                DetailedProgress = progress.DetailedProgress;
                DetailedProgressDescription = progress.DetailedDescription;
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
            if (e.Result is CriticalFailureCopyMoveWorkerResult critical)
            {
                messagingService.ShowError(critical.LocalizedMessage);
            }

            OnFinished();
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

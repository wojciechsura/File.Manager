using BindingEnums;
using File.Manager.API.Filesystem;
using File.Manager.BusinessLogic.Attributes;
using File.Manager.BusinessLogic.Services.Dialogs;
using File.Manager.BusinessLogic.Services.Messaging;
using File.Manager.BusinessLogic.Types;
using File.Manager.Common.Helpers;
using File.Manager.Resources.Operations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static File.Manager.BusinessLogic.ViewModels.Operations.CopyMove.BufferedCopyMoveWithPlanOperationViewModel;
using System.IO;
using File.Manager.API.Filesystem.Models.Items;
using File.Manager.API.Filesystem.Models.Items.Plan;
using File.Manager.API.Filesystem.Models.Items.Operator;
using File.Manager.API.Filesystem.Models.Items.Listing;
using File.Manager.BusinessLogic.Models.Dialogs.CopyMoveConfiguration;
using System.Windows.Markup;

namespace File.Manager.BusinessLogic.ViewModels.Operations.CopyMove
{
    public abstract class BaseCopyMoveOperationViewModel : BaseOperationViewModel
    {
        // Protected types ----------------------------------------------------

        // Progress

        protected class UserQuestionRequestProgress
        {
            public UserQuestionRequestProgress(SingleProblemResolution[] availableResolutions, string header)
            {
                AvailableResolutions = availableResolutions;
                Header = header;
            }

            public SingleProblemResolution[] AvailableResolutions { get; }
            public string Header { get; }
        }

        protected class CopyMoveProgress
        {
            public CopyMoveProgress(int progress, string description, int fileProgress, string fileDescription)
            {
                Progress = progress;
                Description = description;
                FileProgress = fileProgress;
                FileDescription = fileDescription;
            }

            public int Progress { get; }
            public string Description { get; }
            public int FileProgress { get; }
            public string FileDescription { get; }
        }

        // Results

        protected abstract class CopyMoveWorkerResult
        {

        }

        protected sealed class AbortedCopyMoveWorkerResult : CopyMoveWorkerResult
        {
            public AbortedCopyMoveWorkerResult()
            {

            }
        }

        protected sealed class CancelledCopyMoveWorkerResult : CopyMoveWorkerResult
        {
            public CancelledCopyMoveWorkerResult()
            {

            }
        }

        protected sealed class CriticalFailureCopyMoveWorkerResult : CopyMoveWorkerResult
        {
            public CriticalFailureCopyMoveWorkerResult(string localizedMessage)
            {
                LocalizedMessage = localizedMessage;
            }

            public string LocalizedMessage { get; }
        }

        protected sealed class SuccessCopyMoveWorkerResult : CopyMoveWorkerResult
        {
            public SuccessCopyMoveWorkerResult()
            {

            }
        }

        // Worker

        protected abstract class BaseCopyMoveWorkerContext
        {
            protected BaseCopyMoveWorkerContext(CopyMoveConfigurationModel configuration)
            {
                Configuration = configuration;
            }

            public long CopiedSize { get; set; }
            public int CopiedFiles { get; set; }
            public CopyMoveConfigurationModel Configuration { get; }
        }

        protected abstract class BaseCopyMoveWorker<TContext> : BackgroundWorker
            where TContext : BaseCopyMoveWorkerContext
        {
            // Private constants ----------------------------------------------

            private const long BUFFER_SIZE = 1024 * 1024;

            // Protected types ------------------------------------------------

            protected enum ProcessingProblemKind
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
                [LocalizedDescription(nameof(Strings.CopyMove_Question_CannotGetTargetFileAttributes), typeof(Strings))]
                [AvailableResolutions(SingleProblemResolution.Skip, SingleProblemResolution.SkipAll, SingleProblemResolution.Abort)]
                CannotGetTargetFileAttributes,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_CannotGetSourceFileAttributes), typeof(Strings))]
                [AvailableResolutions(SingleProblemResolution.Skip, SingleProblemResolution.SkipAll, SingleProblemResolution.Abort)]
                CannotGetSourceFileAttributes,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_CannotSetTargetFileAttributes), typeof(Strings))]
                [AvailableResolutions(SingleProblemResolution.Ignore, SingleProblemResolution.IgnoreAll, SingleProblemResolution.Abort)]
                CannotSetTargetFileAttributes,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_CannotListSourceFolder), typeof(Strings))]
                [AvailableResolutions(SingleProblemResolution.Ignore, SingleProblemResolution.IgnoreAll, SingleProblemResolution.Abort)]
                CannotListFolderContents,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_InvalidRenamedFilename), typeof(Strings))]
                [AvailableResolutions(SingleProblemResolution.Skip, SingleProblemResolution.SkipAll, SingleProblemResolution.Ignore, SingleProblemResolution.IgnoreAll, SingleProblemResolution.Abort)]
                InvalidRenamedFilename
            }

            // Private fields -------------------------------------------------

            private readonly SemaphoreSlim userDecisionSemaphore;
            private readonly Dictionary<ProcessingProblemKind, GenericProblemResolution> problemResolutions = new();

            // Protected fields -----------------------------------------------

            protected readonly byte[] buffer = new byte[BUFFER_SIZE];
            protected readonly char[] invalidChars;

            // Protected methods ----------------------------------------------
            
            protected BaseCopyMoveWorker()
            {
                userDecisionSemaphore = new SemaphoreSlim(0, 1);
                invalidChars = System.IO.Path.GetInvalidFileNameChars();
            }

            protected abstract (bool exit, CopyMoveWorkerResult result) CopyFile(TContext context,
                IFileInfo fileInfo,
                Stream sourceStream,
                Stream destinationStream,
                byte[] buffer,
                ref bool cancelled);

            protected abstract (bool exit, CopyMoveWorkerResult result) RetrieveFolderContents(TContext context,
                IFolderInfo folderInfo,
                IFilesystemOperator sourceFolderOperator,
                IFilesystemOperator destinationFolderOperator,
                ref IReadOnlyList<IBaseItemInfo> items);

            protected string FindAvailableName(IFileInfo planFile, IFilesystemOperator destinationOperator)
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

            protected GenericProblemResolution GetResolutionFor(ProcessingProblemKind problemKind,
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
                    case SingleProblemResolution.IgnoreAll:
                        problemResolutions[problemKind] = GenericProblemResolution.Ignore;
                        return GenericProblemResolution.Ignore;
                    case SingleProblemResolution.Ignore:
                        return GenericProblemResolution.Ignore;
                    case SingleProblemResolution.Abort:
                        return GenericProblemResolution.Abort;
                    default:
                        throw new InvalidOperationException("Unsupported problem resolution!");
                }
            }

            protected (bool exit, CopyMoveWorkerResult result) EnsureSourceFileExists(IFileInfo planFile,
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

            protected (bool exit, CopyMoveWorkerResult result) EnsureDestinationFileDoesNotExist(IFileInfo planFile,
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

            protected (bool exit, CopyMoveWorkerResult result) CheckForOverwritingReadOnlyFile(IFileInfo planFile,
                IFilesystemOperator sourceOperator,
                IFilesystemOperator destinationOperator,
                ref string targetName)
            {
                if (destinationOperator.FileExists(targetName))
                {
                    var attributes = destinationOperator.GetFileAttributes(targetName);

                    if (attributes == null)
                    {
                        var resolution = GetResolutionFor(ProcessingProblemKind.CannotGetTargetFileAttributes,
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

            protected (bool exit, CopyMoveWorkerResult result) CheckForOverwritingSystemFile(IFileInfo planFile,
                IFilesystemOperator sourceOperator,
                IFilesystemOperator destinationOperator,
                ref string targetName)
            {
                if (destinationOperator.FileExists(targetName))
                {
                    var attributes = destinationOperator.GetFileAttributes(targetName);

                    if (attributes == null)
                    {
                        var resolution = GetResolutionFor(ProcessingProblemKind.CannotGetTargetFileAttributes,
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

            protected (bool exit, CopyMoveWorkerResult result) CopyAttributes(IFileInfo planFile,
                IFilesystemOperator sourceOperator,
                IFilesystemOperator destinationOperator,
                string targetName)
            {
                var attributes = sourceOperator.GetFileAttributes(planFile.Name);

                if (attributes == null)
                {
                    var resolution = GetResolutionFor(ProcessingProblemKind.CannotGetSourceFileAttributes,
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

                if (!destinationOperator.SetFileAttributes(targetName, attributes.Value))
                {
                    var resolution = GetResolutionFor(ProcessingProblemKind.CannotSetTargetFileAttributes,
                        sourceOperator.CurrentPath,
                        destinationOperator.CurrentPath,
                        targetName);

                    switch (resolution)
                    {
                        case GenericProblemResolution.Ignore:
                            return (false, null);
                        case GenericProblemResolution.Abort:
                            return (true, new AbortedCopyMoveWorkerResult());
                        default:
                            throw new InvalidOperationException("Invalid resolution!");
                    }
                }

                return (false, null);
            }

            protected (bool exit, CopyMoveWorkerResult result) OpenSourceFile(IFileInfo planFile,
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

            protected (bool exit, CopyMoveWorkerResult result) OpenDestinationFile(IFileInfo planFile,
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

            protected (bool exit, CopyMoveWorkerResult result) DeleteSourceFile(IFileInfo planFile,
                IFilesystemOperator sourceOperator,
                IFilesystemOperator destinationOperator)
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

            protected (bool exit, CopyMoveWorkerResult result) CreateDestinationFolder(IFolderInfo planFolder,
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

            protected (bool exit, CopyMoveWorkerResult result) EnterSourceFolder(IFolderInfo planFolder,
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

            protected (bool exit, CopyMoveWorkerResult result) EnterDestinationFolder(IFolderInfo planFolder,
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

            private (bool exit, CopyMoveWorkerResult result) RenameTargetFile(TContext context, 
                IFilesystemOperator sourceOperator, 
                IFilesystemOperator destinationOperator, 
                ref string targetName)
            {
                if (context.Configuration.RenameFrom.IsMatch(targetName))
                {
                    var newName = context.Configuration.RenameFrom.Replace(targetName, context.Configuration.RenameTo);

                    // New name may be invalid                    
                    if (string.IsNullOrEmpty(newName) || newName.Any(c => invalidChars.Contains(c)))
                    {
                        var resolution = GetResolutionFor(ProcessingProblemKind.InvalidRenamedFilename,
                        sourceOperator.CurrentPath,
                        destinationOperator.CurrentPath,
                        newName);

                        switch (resolution)
                        {
                            case GenericProblemResolution.Skip:
                                return (true, null);
                            case GenericProblemResolution.Ignore:
                                // Name won't be changed
                                break;
                            case GenericProblemResolution.Abort:
                                return (true, new AbortedCopyMoveWorkerResult());
                            default:
                                throw new InvalidOperationException("Invalid problem resolution!");
                        }
                    }
                    else
                    {
                        targetName = newName;
                    }                    
                }

                return (false, null);
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

            protected CopyMoveWorkerResult ProcessItems(TContext context,
                IReadOnlyList<IBaseItemInfo> items,
                DataTransferOperationType operationType,
                IFilesystemOperator sourceOperator,
                IFilesystemOperator destinationOperator,
                bool isRoot)
            {
                foreach (var item in items)
                {
                    if (item is IFolderInfo planFolder)
                    {
                        var result = ProcessFolder(context, planFolder, operationType, sourceOperator, destinationOperator, isRoot);
                        if (result != null)
                            return result;
                    }
                    else if (item is IFileInfo planFile)
                    {
                        var result = ProcessFile(context, planFile, operationType, sourceOperator, destinationOperator, isRoot);
                        if (result != null)
                            return result;
                    }
                    else
                        throw new InvalidOperationException("Invalid plan item!");
                }

                return null;
            }

            protected CopyMoveWorkerResult ProcessFile(TContext context,
                IFileInfo operatorFile,
                DataTransferOperationType operationType,
                IFilesystemOperator sourceOperator,
                IFilesystemOperator destinationOperator,
                bool isRoot)
            {
                try
                {
                    bool exit;
                    CopyMoveWorkerResult result;

                    // Check if source file exists

                    (exit, result) = EnsureSourceFileExists(operatorFile, sourceOperator, destinationOperator);
                    if (exit)
                        return result;

                    // Generate new name for the file

                    string targetName = operatorFile.Name;
                    if (context.Configuration.RenameFiles && (isRoot || context.Configuration.RenameRecursive))
                    {
                        (exit, result) = RenameTargetFile(context, sourceOperator, destinationOperator, ref targetName);
                        if (exit)
                            return result;
                    }

                    // Ask about overwriting existing file

                    (exit, result) = EnsureDestinationFileDoesNotExist(operatorFile, sourceOperator, destinationOperator, ref targetName);
                    if (exit)
                        return result;

                    // Ask about overwriting readonly file

                    (exit, result) = CheckForOverwritingReadOnlyFile(operatorFile, sourceOperator, destinationOperator, ref targetName);
                    if (exit)
                        return result;

                    // Ask about overwriting system file

                    (exit, result) = CheckForOverwritingSystemFile(operatorFile, sourceOperator, destinationOperator, ref targetName);

                    // Get the source stream

                    Stream sourceStream = null;
                    (exit, result) = OpenSourceFile(operatorFile, sourceOperator, destinationOperator, ref sourceStream);
                    if (exit)
                        return result;

                    // Get the destination stream

                    Stream destinationStream = null;
                    (exit, result) = OpenDestinationFile(operatorFile, sourceOperator, destinationOperator, targetName, ref destinationStream);
                    if (exit)
                        return result;

                    // Start copying

                    bool cancelled = false;

                    try
                    {
                        (exit, result) = CopyFile(context, operatorFile, sourceStream, destinationStream, buffer, ref cancelled);
                        if (exit)
                            return result;

                        (exit, result) = CopyAttributes(operatorFile, sourceOperator, destinationOperator, targetName);
                        if (exit)
                            return result;

                        if (operationType == DataTransferOperationType.Move)
                        {
                            (exit, result) = DeleteSourceFile(operatorFile, sourceOperator, destinationOperator);
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
                            operatorFile.Name);

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
                    context.CopiedSize += operatorFile.Size;
                    context.CopiedFiles++;
                }

                return null;
            }

            private CopyMoveWorkerResult ProcessFolder(TContext context,
                IFolderInfo folderInfo,
                DataTransferOperationType operationType,
                IFilesystemOperator sourceOperator,
                IFilesystemOperator destinationOperator,
                bool isRoot)
            {
                bool exit;
                CopyMoveWorkerResult result;

                // Create folder in remote location
                (exit, result) = CreateDestinationFolder(folderInfo, sourceOperator, destinationOperator);
                if (exit)
                    return result;

                IFilesystemOperator sourceFolderOperator = null;

                (exit, result) = EnterSourceFolder(folderInfo, sourceOperator, destinationOperator, ref sourceFolderOperator);
                if (exit)
                    return result;

                IFilesystemOperator destinationFolderOperator = null;

                (exit, result) = EnterDestinationFolder(folderInfo, sourceOperator, destinationOperator, ref destinationFolderOperator);
                if (exit)
                    return result;

                IReadOnlyList<IBaseItemInfo> items = null;
                (exit, result) = RetrieveFolderContents(context, folderInfo, sourceFolderOperator, destinationFolderOperator, ref items);
                if (exit)
                    return result;

                return ProcessItems(context, items, operationType, sourceFolderOperator, destinationFolderOperator, false);
            }            

            // Public properties ----------------------------------------------

            public SemaphoreSlim UserDecisionSemaphore => userDecisionSemaphore;

            public SingleProblemResolution? UserDecision { get; set; }
        }

        // Private fields -----------------------------------------------------

        private int fileProgress;
        private string fileProgressDescription;
        private string fromAddress;
        private string toAddress;

        // Protected fields ---------------------------------------------------

        protected readonly IFilesystemOperator sourceOperator;
        protected readonly IFilesystemOperator destinationOperator;
        protected readonly DataTransferOperationType operationType;
        protected readonly IReadOnlyList<Item> selectedItems;
        protected readonly CopyMoveConfigurationModel configuration;

        // Public methods -----------------------------------------------------

        public BaseCopyMoveOperationViewModel(IDialogService dialogService, 
            IMessagingService messagingService,
            IFilesystemOperator sourceOperator,
            IFilesystemOperator destinationOperator,
            IReadOnlyList<Item> selectedItems,
            CopyMoveConfigurationModel configuration,
            DataTransferOperationType operationType)
            : base(dialogService, messagingService)
        {
            this.sourceOperator = sourceOperator;
            this.destinationOperator = destinationOperator;
            this.selectedItems = selectedItems;
            this.configuration = configuration;
            this.operationType = operationType;
        }

        public override void Run()
        {
            if (sourceOperator.CurrentPath == destinationOperator.CurrentPath)
            {
                messagingService.Warn(Strings.CopyMove_Warning_CannotCopyItemsToTheSameLocation);
                return;
            }
        }

        // Public properties --------------------------------------------------

        public int FileProgress
        {
            get => fileProgress;
            set => Set(ref fileProgress, value);
        }

        public string FileProgressDescription
        {
            get => fileProgressDescription;
            set => Set(ref fileProgressDescription, value);
        }

        public string FromAddress
        {
            get => fromAddress; 
            set => Set(ref fromAddress, value);
        }

        public string ToAddress
        {
            get => toAddress;
            set => Set(ref toAddress, value);
        }
    }
}

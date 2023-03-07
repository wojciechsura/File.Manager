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
            public UserQuestionRequestProgress(SingleCopyMoveProblemResolution[] availableResolutions, string header)
            {
                AvailableResolutions = availableResolutions;
                Header = header;
            }

            public SingleCopyMoveProblemResolution[] AvailableResolutions { get; }
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

            protected enum CopyMoveProblemKind
            {
                [LocalizedDescription(nameof(Strings.CopyMove_Question_FailedToCreateDestinationFolder), typeof(Strings))]
                [AvailableCopyMoveResolutions(SingleCopyMoveProblemResolution.Skip, SingleCopyMoveProblemResolution.SkipAll, SingleCopyMoveProblemResolution.Abort)]
                CannotCreateDestinationFolder,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_FailedToEnterDestinationFolder), typeof(Strings))]
                [AvailableCopyMoveResolutions(SingleCopyMoveProblemResolution.Skip, SingleCopyMoveProblemResolution.SkipAll, SingleCopyMoveProblemResolution.Abort)]
                CannotEnterDestinationFolder,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_FailedToEnterSourceFolder), typeof(Strings))]
                [AvailableCopyMoveResolutions(SingleCopyMoveProblemResolution.Skip, SingleCopyMoveProblemResolution.SkipAll, SingleCopyMoveProblemResolution.Abort)]
                CannotEnterSourceFolder,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_SourceFileDoesNotExist), typeof(Strings))]
                [AvailableCopyMoveResolutions(SingleCopyMoveProblemResolution.Skip, SingleCopyMoveProblemResolution.SkipAll, SingleCopyMoveProblemResolution.Abort)]
                SourceFileDoesNotExist,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_DestinationFileAlreadyExists), typeof(Strings))]
                [AvailableCopyMoveResolutions(SingleCopyMoveProblemResolution.Skip, SingleCopyMoveProblemResolution.SkipAll,
                    SingleCopyMoveProblemResolution.Overwrite, SingleCopyMoveProblemResolution.OverwriteAll,
                    SingleCopyMoveProblemResolution.Rename, SingleCopyMoveProblemResolution.RenameAll,
                    SingleCopyMoveProblemResolution.Abort)]
                DestinationFileAlreadyExists,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_DestinationFileIsReadOnly), typeof(Strings))]
                [AvailableCopyMoveResolutions(SingleCopyMoveProblemResolution.Skip, SingleCopyMoveProblemResolution.SkipAll,
                    SingleCopyMoveProblemResolution.Overwrite, SingleCopyMoveProblemResolution.OverwriteAll,
                    SingleCopyMoveProblemResolution.Rename, SingleCopyMoveProblemResolution.RenameAll,
                    SingleCopyMoveProblemResolution.Abort)]
                DestinationFileIsReadOnly,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_DestinationFileIsSystem), typeof(Strings))]
                [AvailableCopyMoveResolutions(SingleCopyMoveProblemResolution.Skip, SingleCopyMoveProblemResolution.SkipAll,
                    SingleCopyMoveProblemResolution.Overwrite, SingleCopyMoveProblemResolution.OverwriteAll,
                    SingleCopyMoveProblemResolution.Rename, SingleCopyMoveProblemResolution.RenameAll,
                    SingleCopyMoveProblemResolution.Abort)]
                DestinationFileIsSystem,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_CannotOpenSourceFile), typeof(Strings))]
                [AvailableCopyMoveResolutions(SingleCopyMoveProblemResolution.Skip, SingleCopyMoveProblemResolution.SkipAll, SingleCopyMoveProblemResolution.Abort)]
                CannotOpenSourceFile,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_CannotOpenDestinationFile), typeof(Strings))]
                [AvailableCopyMoveResolutions(SingleCopyMoveProblemResolution.Skip, SingleCopyMoveProblemResolution.SkipAll, SingleCopyMoveProblemResolution.Abort)]
                CannotOpenDestinationFile,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_FailedToChangeFileAttributes), typeof(Strings))]
                [AvailableCopyMoveResolutions(SingleCopyMoveProblemResolution.Skip, SingleCopyMoveProblemResolution.SkipAll, SingleCopyMoveProblemResolution.Abort)]
                FailedToChangeFileAttributes,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_FailedToCopyFile), typeof(Strings))]
                [AvailableCopyMoveResolutions(SingleCopyMoveProblemResolution.Skip, SingleCopyMoveProblemResolution.SkipAll, SingleCopyMoveProblemResolution.Abort)]
                FailedToCopyFile,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_FailedToRemoveSourceFile), typeof(Strings))]
                [AvailableCopyMoveResolutions(SingleCopyMoveProblemResolution.Skip, SingleCopyMoveProblemResolution.SkipAll, SingleCopyMoveProblemResolution.Abort)]
                FailedToDeleteSourceFile,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_CannotGetTargetFileAttributes), typeof(Strings))]
                [AvailableCopyMoveResolutions(SingleCopyMoveProblemResolution.Skip, SingleCopyMoveProblemResolution.SkipAll, SingleCopyMoveProblemResolution.Abort)]
                CannotGetTargetFileAttributes,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_CannotGetSourceFileAttributes), typeof(Strings))]
                [AvailableCopyMoveResolutions(SingleCopyMoveProblemResolution.Skip, SingleCopyMoveProblemResolution.SkipAll, SingleCopyMoveProblemResolution.Abort)]
                CannotGetSourceFileAttributes,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_CannotSetTargetFileAttributes), typeof(Strings))]
                [AvailableCopyMoveResolutions(SingleCopyMoveProblemResolution.Ignore, SingleCopyMoveProblemResolution.IgnoreAll, SingleCopyMoveProblemResolution.Abort)]
                CannotSetTargetFileAttributes,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_CannotListSourceFolder), typeof(Strings))]
                [AvailableCopyMoveResolutions(SingleCopyMoveProblemResolution.Ignore, SingleCopyMoveProblemResolution.IgnoreAll, SingleCopyMoveProblemResolution.Abort)]
                CannotListFolderContents,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_InvalidRenamedFilename), typeof(Strings))]
                [AvailableCopyMoveResolutions(SingleCopyMoveProblemResolution.Skip, SingleCopyMoveProblemResolution.SkipAll, SingleCopyMoveProblemResolution.Ignore, SingleCopyMoveProblemResolution.IgnoreAll, SingleCopyMoveProblemResolution.Abort)]
                InvalidRenamedFilename,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_FileCopiedIntoItself), typeof(Strings))]
                [AvailableCopyMoveResolutions(SingleCopyMoveProblemResolution.Skip, SingleCopyMoveProblemResolution.SkipAll, SingleCopyMoveProblemResolution.Rename, SingleCopyMoveProblemResolution.RenameAll, SingleCopyMoveProblemResolution.Abort)]
                FileCopiedIntoItself,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_FolderCopiedIntoItself), typeof(Strings))]
                [AvailableCopyMoveResolutions(SingleCopyMoveProblemResolution.Skip, SingleCopyMoveProblemResolution.SkipAll, SingleCopyMoveProblemResolution.Rename, SingleCopyMoveProblemResolution.RenameAll, SingleCopyMoveProblemResolution.Abort)]
                FolderCopiedIntoItself
            }

            // Private fields -------------------------------------------------

            private readonly SemaphoreSlim userDecisionSemaphore;
            private readonly Dictionary<CopyMoveProblemKind, GenericCopyMoveProblemResolution> problemResolutions = new();

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

            protected string FindAvailableName(IBaseItemInfo item, IFilesystemOperator destinationOperator)
            {
                long i = 1;
                string name = System.IO.Path.GetFileNameWithoutExtension(item.Name);
                string extension = System.IO.Path.GetExtension(item.Name);
                string newName;

                do
                {
                    newName = CreateNewName(i, name, extension);
                    i++;
                }
                while (destinationOperator.FileExists(newName) || destinationOperator.FolderExists(newName));
                
                return newName;

                static string CreateNewName(long i, string name, string extension)
                {
                    return $"{name} ({i}){extension}";
                }
            }

            protected GenericCopyMoveProblemResolution GetResolutionFor(CopyMoveProblemKind problemKind,
                string sourceAddress,
                string destinationAddress,
                string name)
            {
                if (problemResolutions.TryGetValue(problemKind, out GenericCopyMoveProblemResolution resolution) && resolution != GenericCopyMoveProblemResolution.Ask)
                    return resolution;

                LocalizedDescriptionAttribute localizedDescription = problemKind.GetAttribute<LocalizedDescriptionAttribute>();
                string header = string.Format(localizedDescription.Description, sourceAddress, destinationAddress, name);

                AvailableCopyMoveResolutionsAttribute availableResolutions = problemKind.GetAttribute<AvailableCopyMoveResolutionsAttribute>();

                var progress = new UserQuestionRequestProgress(availableResolutions.AvailableResolutions, header);
                ReportProgress(0, progress);

                UserDecisionSemaphore.Wait();

                switch (UserDecision)
                {
                    case SingleCopyMoveProblemResolution.Skip:
                        return GenericCopyMoveProblemResolution.Skip;
                    case SingleCopyMoveProblemResolution.SkipAll:
                        problemResolutions[problemKind] = GenericCopyMoveProblemResolution.Skip;
                        return GenericCopyMoveProblemResolution.Skip;
                    case SingleCopyMoveProblemResolution.Overwrite:
                        return GenericCopyMoveProblemResolution.Overwrite;
                    case SingleCopyMoveProblemResolution.OverwriteAll:
                        problemResolutions[problemKind] = GenericCopyMoveProblemResolution.Overwrite;
                        return GenericCopyMoveProblemResolution.Overwrite;
                    case SingleCopyMoveProblemResolution.Rename:
                        return GenericCopyMoveProblemResolution.Rename;
                    case SingleCopyMoveProblemResolution.RenameAll:
                        problemResolutions[problemKind] = GenericCopyMoveProblemResolution.Rename;
                        return GenericCopyMoveProblemResolution.Rename;
                    case SingleCopyMoveProblemResolution.IgnoreAll:
                        problemResolutions[problemKind] = GenericCopyMoveProblemResolution.Ignore;
                        return GenericCopyMoveProblemResolution.Ignore;
                    case SingleCopyMoveProblemResolution.Ignore:
                        return GenericCopyMoveProblemResolution.Ignore;
                    case SingleCopyMoveProblemResolution.Abort:
                        return GenericCopyMoveProblemResolution.Abort;
                    default:
                        throw new InvalidOperationException("Unsupported problem resolution!");
                }
            }

            protected (bool exit, CopyMoveWorkerResult result) EnsureSourceFileExists(IFileInfo fileInfo,
                IFilesystemOperator sourceOperator,
                IFilesystemOperator destinationOperator)
            {
                if (!sourceOperator.FileExists(fileInfo.Name))
                {
                    var resolution = GetResolutionFor(CopyMoveProblemKind.SourceFileDoesNotExist,
                        sourceOperator.CurrentPath,
                        destinationOperator.CurrentPath,
                        fileInfo.Name);

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

            protected (bool exit, CopyMoveWorkerResult result) EnsureDestinationFileDoesNotExist(IFileInfo fileInfo,
                IFilesystemOperator sourceOperator,
                IFilesystemOperator destinationOperator,
                ref string targetName)
            {
                if (destinationOperator.FileExists(fileInfo.Name))
                {
                    var resolution = GetResolutionFor(CopyMoveProblemKind.DestinationFileAlreadyExists,
                        sourceOperator.CurrentPath,
                        destinationOperator.CurrentPath,
                        fileInfo.Name);

                    switch (resolution)
                    {
                        case GenericCopyMoveProblemResolution.Skip:
                            return (true, null);
                        case GenericCopyMoveProblemResolution.Abort:
                            return (true, new AbortedCopyMoveWorkerResult());
                        case GenericCopyMoveProblemResolution.Overwrite:
                            break;
                        case GenericCopyMoveProblemResolution.Rename:
                            targetName = FindAvailableName(fileInfo, destinationOperator);
                            break;
                        default:
                            throw new InvalidOperationException("Invalid resolution!");
                    }
                }

                return (false, null);
            }

            protected (bool exit, CopyMoveWorkerResult result) CheckForOverwritingReadOnlyFile(IFileInfo fileInfo,
                IFilesystemOperator sourceOperator,
                IFilesystemOperator destinationOperator,
                ref string targetName)
            {
                if (destinationOperator.FileExists(targetName))
                {
                    var attributes = destinationOperator.GetFileAttributes(targetName);

                    if (attributes == null)
                    {
                        var resolution = GetResolutionFor(CopyMoveProblemKind.CannotGetTargetFileAttributes,
                            sourceOperator.CurrentPath,
                            destinationOperator.CurrentPath,
                            targetName);

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

                    if (attributes.Value.HasFlag(System.IO.FileAttributes.ReadOnly))
                    {
                        var resolution = GetResolutionFor(CopyMoveProblemKind.DestinationFileIsReadOnly,
                            sourceOperator.CurrentPath,
                            destinationOperator.CurrentPath,
                            targetName);

                        switch (resolution)
                        {
                            case GenericCopyMoveProblemResolution.Skip:
                                return (true, null);
                            case GenericCopyMoveProblemResolution.Abort:
                                return (true, new AbortedCopyMoveWorkerResult());
                            case GenericCopyMoveProblemResolution.Rename:
                                targetName = FindAvailableName(fileInfo, destinationOperator);
                                break;
                            case GenericCopyMoveProblemResolution.Overwrite:
                                {
                                    attributes &= ~FileAttributes.ReadOnly;

                                    if (!destinationOperator.SetFileAttributes(targetName, attributes.Value))
                                    {
                                        var innerResolution = GetResolutionFor(CopyMoveProblemKind.FailedToChangeFileAttributes,
                                            sourceOperator.CurrentPath,
                                            destinationOperator.CurrentPath,
                                            targetName);

                                        switch (innerResolution)
                                        {
                                            case GenericCopyMoveProblemResolution.Skip:
                                                return (true, null);
                                            case GenericCopyMoveProblemResolution.Abort:
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

            protected (bool exit, CopyMoveWorkerResult result) CheckForOverwritingSystemFile(IFileInfo fileInfo,
                IFilesystemOperator sourceOperator,
                IFilesystemOperator destinationOperator,
                ref string targetName)
            {
                if (destinationOperator.FileExists(targetName))
                {
                    var attributes = destinationOperator.GetFileAttributes(targetName);

                    if (attributes == null)
                    {
                        var resolution = GetResolutionFor(CopyMoveProblemKind.CannotGetTargetFileAttributes,
                            sourceOperator.CurrentPath,
                            destinationOperator.CurrentPath,
                            targetName);

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

                    if (attributes.Value.HasFlag(System.IO.FileAttributes.System))
                    {
                        var resolution = GetResolutionFor(CopyMoveProblemKind.DestinationFileIsSystem,
                            sourceOperator.CurrentPath,
                            destinationOperator.CurrentPath,
                            targetName);

                        switch (resolution)
                        {
                            case GenericCopyMoveProblemResolution.Skip:
                                return (true, null);
                            case GenericCopyMoveProblemResolution.Abort:
                                return (true, new AbortedCopyMoveWorkerResult());
                            case GenericCopyMoveProblemResolution.Rename:
                                targetName = FindAvailableName(fileInfo, destinationOperator);
                                break;
                            case GenericCopyMoveProblemResolution.Overwrite:
                                {
                                    attributes &= ~FileAttributes.System;

                                    if (!destinationOperator.SetFileAttributes(targetName, attributes.Value))
                                    {
                                        var innerResolution = GetResolutionFor(CopyMoveProblemKind.FailedToChangeFileAttributes,
                                            sourceOperator.CurrentPath,
                                            destinationOperator.CurrentPath,
                                            targetName);

                                        switch (innerResolution)
                                        {
                                            case GenericCopyMoveProblemResolution.Skip:
                                                return (true, null);
                                            case GenericCopyMoveProblemResolution.Abort:
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

            protected (bool exit, CopyMoveWorkerResult result) CopyAttributes(IFileInfo fileInfo,
                IFilesystemOperator sourceOperator,
                IFilesystemOperator destinationOperator,
                string targetName)
            {
                var attributes = sourceOperator.GetFileAttributes(fileInfo.Name);

                if (attributes == null)
                {
                    var resolution = GetResolutionFor(CopyMoveProblemKind.CannotGetSourceFileAttributes,
                        sourceOperator.CurrentPath,
                        destinationOperator.CurrentPath,
                        fileInfo.Name);

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

                if (!destinationOperator.SetFileAttributes(targetName, attributes.Value))
                {
                    var resolution = GetResolutionFor(CopyMoveProblemKind.CannotSetTargetFileAttributes,
                        sourceOperator.CurrentPath,
                        destinationOperator.CurrentPath,
                        targetName);

                    switch (resolution)
                    {
                        case GenericCopyMoveProblemResolution.Ignore:
                            return (false, null);
                        case GenericCopyMoveProblemResolution.Abort:
                            return (true, new AbortedCopyMoveWorkerResult());
                        default:
                            throw new InvalidOperationException("Invalid resolution!");
                    }
                }

                return (false, null);
            }

            protected (bool exit, CopyMoveWorkerResult result) OpenSourceFile(IFileInfo fileInfo,
                IFilesystemOperator sourceOperator,
                IFilesystemOperator destinationOperator,
                ref Stream sourceStream)
            {
                sourceStream = sourceOperator.OpenFileForReading(fileInfo.Name);
                if (sourceStream == null)
                {
                    var resolution = GetResolutionFor(CopyMoveProblemKind.CannotOpenSourceFile,
                        sourceOperator.CurrentPath,
                        destinationOperator.CurrentPath,
                        fileInfo.Name);

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

            protected (bool exit, CopyMoveWorkerResult result) OpenDestinationFile(IFileInfo fileInfo,
                IFilesystemOperator sourceOperator,
                IFilesystemOperator destinationOperator,
                string targetName,
                ref Stream destinationStream)
            {
                destinationStream = destinationOperator.OpenFileForWriting(targetName);
                if (destinationStream == null)
                {
                    var resolution = GetResolutionFor(CopyMoveProblemKind.CannotOpenDestinationFile,
                        sourceOperator.CurrentPath,
                        destinationOperator.CurrentPath,
                        targetName);

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

            protected (bool exit, CopyMoveWorkerResult result) DeleteSourceFile(IFileInfo fileInfo,
                IFilesystemOperator sourceOperator,
                IFilesystemOperator destinationOperator)
            {
                if (!sourceOperator.DeleteFile(fileInfo.Name))
                {
                    var resolution = GetResolutionFor(CopyMoveProblemKind.FailedToDeleteSourceFile,
                        sourceOperator.CurrentPath,
                        destinationOperator.CurrentPath,
                        fileInfo.Name);

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

            protected (bool exit, CopyMoveWorkerResult result) CreateDestinationFolder(IFolderInfo folderInfo,
                IFilesystemOperator sourceOperator,
                IFilesystemOperator destinationOperator,
                string targetName)
            {
                if (!destinationOperator.CreateFolder(targetName))
                {
                    var resolution = GetResolutionFor(CopyMoveProblemKind.CannotCreateDestinationFolder,
                        sourceOperator.CurrentPath,
                        destinationOperator.CurrentPath,
                        targetName);

                    switch (resolution)
                    {
                        case GenericCopyMoveProblemResolution.Skip:
                            return (true, null);
                        case GenericCopyMoveProblemResolution.Abort:
                            return (true, new AbortedCopyMoveWorkerResult());
                        default:
                            throw new InvalidOperationException("Invalid problem resolution!");
                    }
                }

                return (false, null);
            }

            protected (bool exit, CopyMoveWorkerResult result) EnterSourceFolder(IFolderInfo folderInfo,
                IFilesystemOperator sourceOperator,
                IFilesystemOperator destinationOperator,
                ref IFilesystemOperator sourceFolderOperator)
            {
                sourceFolderOperator = sourceOperator.EnterFolder(folderInfo.Name);

                // Enter folder in local location
                if (sourceFolderOperator == null)
                {
                    var resolution = GetResolutionFor(CopyMoveProblemKind.CannotEnterSourceFolder,
                        sourceOperator.CurrentPath,
                        destinationOperator.CurrentPath,
                        folderInfo.Name);

                    switch (resolution)
                    {
                        case GenericCopyMoveProblemResolution.Skip:
                            return (true, null);
                        case GenericCopyMoveProblemResolution.Abort:
                            return (true, new AbortedCopyMoveWorkerResult());
                        default:
                            throw new InvalidOperationException("Invalid problem resolution!");
                    }
                }

                return (false, null);
            }

            protected (bool exit, CopyMoveWorkerResult result) EnterDestinationFolder(IFolderInfo folderInfo,
                IFilesystemOperator sourceOperator,
                IFilesystemOperator destinationOperator,
                string targetName,
                ref IFilesystemOperator destinationFolderOperator)
            {
                destinationFolderOperator = destinationOperator.EnterFolder(targetName);

                // Enter folder in remote location
                if (destinationFolderOperator == null)
                {
                    var resolution = GetResolutionFor(CopyMoveProblemKind.CannotEnterDestinationFolder,
                        sourceOperator.CurrentPath,
                        destinationOperator.CurrentPath,
                        targetName);

                    switch (resolution)
                    {
                        case GenericCopyMoveProblemResolution.Skip:
                            return (true, null);
                        case GenericCopyMoveProblemResolution.Abort:
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
                        var resolution = GetResolutionFor(CopyMoveProblemKind.InvalidRenamedFilename,
                            sourceOperator.CurrentPath,
                            destinationOperator.CurrentPath,
                            newName);

                        switch (resolution)
                        {
                            case GenericCopyMoveProblemResolution.Skip:
                                return (true, null);
                            case GenericCopyMoveProblemResolution.Ignore:
                                // Name won't be changed
                                break;
                            case GenericCopyMoveProblemResolution.Abort:
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

            private (bool exit, CopyMoveWorkerResult result) EnsureFileNotCopiedOnItself(IFileInfo operatorFile, 
                IFilesystemOperator sourceOperator, 
                IFilesystemOperator destinationOperator, 
                ref string targetName)
            {
                if (sourceOperator.CurrentPath == destinationOperator.CurrentPath &&
                    operatorFile.Name == targetName) 
                {
                    var resolution = GetResolutionFor(CopyMoveProblemKind.FileCopiedIntoItself,
                            sourceOperator.CurrentPath,
                            destinationOperator.CurrentPath,
                            targetName);

                    switch (resolution)
                    {
                        case GenericCopyMoveProblemResolution.Skip:
                            return (true, null);
                        case GenericCopyMoveProblemResolution.Rename:
                            targetName = FindAvailableName(operatorFile, destinationOperator);
                            return (false, null);
                            break;
                        case GenericCopyMoveProblemResolution.Abort:
                            return (true, new AbortedCopyMoveWorkerResult());
                        default:
                            throw new InvalidOperationException("Invalid problem resolution!");
                    }
                }

                return (false, null);
            }

            private (bool exit, CopyMoveWorkerResult result) EnsureFolderNotCopiedOnItself(IFolderInfo folderInfo, 
                IFilesystemOperator sourceOperator, 
                IFilesystemOperator destinationOperator, 
                ref string targetName)
            {
                if (sourceOperator.CurrentPath == destinationOperator.CurrentPath &&
                    folderInfo.Name == targetName)
                {
                    var resolution = GetResolutionFor(CopyMoveProblemKind.FolderCopiedIntoItself,
                            sourceOperator.CurrentPath,
                            destinationOperator.CurrentPath,
                            targetName);

                    switch (resolution)
                    {
                        case GenericCopyMoveProblemResolution.Skip:
                            return (true, null);
                        case GenericCopyMoveProblemResolution.Rename:
                            targetName = FindAvailableName(folderInfo, destinationOperator);
                            return (false, null);
                        case GenericCopyMoveProblemResolution.Abort:
                            return (true, new AbortedCopyMoveWorkerResult());
                        default:
                            throw new InvalidOperationException("Invalid problem resolution!");
                    }
                }

                return (false, null);
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

                    // Check if file is not being copied into itself

                    (exit, result) = EnsureFileNotCopiedOnItself(operatorFile, sourceOperator, destinationOperator, ref targetName);
                    if (exit) 
                        return result;

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
                    if (exit)
                        return result;

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
                    }
                    catch
                    {
                        destinationStream?.Dispose();
                        destinationStream = null;

                        // Try to delete partially-copied file
                        destinationOperator.DeleteFile(targetName);

                        var resolution = GetResolutionFor(CopyMoveProblemKind.FailedToCopyFile,
                            sourceOperator.CurrentPath,
                            destinationOperator.CurrentPath,
                            operatorFile.Name);

                        switch (resolution)
                        {
                            case GenericCopyMoveProblemResolution.Skip:
                                return null;
                            case GenericCopyMoveProblemResolution.Abort:
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

                        if (cancelled)
                        {
                            // Try to remove file, which was not copied
                            destinationOperator.DeleteFile(targetName);
                        }
                    }

                    if (operationType == DataTransferOperationType.Move)
                    {
                        (exit, result) = DeleteSourceFile(operatorFile, sourceOperator, destinationOperator);
                        if (exit)
                            return result;
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

                string targetName = folderInfo.Name;

                (exit, result) = EnsureFolderNotCopiedOnItself(folderInfo, sourceOperator, destinationOperator, ref targetName);
                if (exit) 
                    return result;

                // Create folder in remote location
                (exit, result) = CreateDestinationFolder(folderInfo, sourceOperator, destinationOperator, targetName);
                if (exit)
                    return result;

                IFilesystemOperator sourceFolderOperator = null;

                (exit, result) = EnterSourceFolder(folderInfo, sourceOperator, destinationOperator, ref sourceFolderOperator);
                if (exit)
                    return result;

                IFilesystemOperator destinationFolderOperator = null;

                (exit, result) = EnterDestinationFolder(folderInfo, sourceOperator, destinationOperator, targetName, ref destinationFolderOperator);
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

            public SingleCopyMoveProblemResolution? UserDecision { get; set; }
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

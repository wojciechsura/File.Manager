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
using System.Reflection;
using System.Windows.Controls;

namespace File.Manager.BusinessLogic.ViewModels.Operations.CopyMove
{
    public abstract class BaseCopyMoveOperationViewModel : BaseOperationViewModel
    {
        // Protected types ----------------------------------------------------

        // Input

        protected sealed class CopyMoveWorkerInput
        {
            public CopyMoveWorkerInput(DataTransferOperationType operationType, FilesystemOperator sourceOperator, FilesystemOperator destinationOperator, CopyMoveConfigurationModel configuration, IReadOnlyList<Item> selectedItems)
            {
                OperationType = operationType;
                SourceOperator = sourceOperator;
                DestinationOperator = destinationOperator;
                Configuration = configuration;
                SelectedItems = selectedItems;
            }

            public DataTransferOperationType OperationType { get; }
            public FilesystemOperator SourceOperator { get; }
            public FilesystemOperator DestinationOperator { get; }
            public CopyMoveConfigurationModel Configuration { get; }
            public IReadOnlyList<Item> SelectedItems { get; }
        }

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

        protected abstract class BaseCopyMoveWorker<TContext> : BaseWorker, ICopyMoveUserDecisionWorker
            where TContext : BaseCopyMoveWorkerContext
        {
            // Private constants ----------------------------------------------

            private const long BUFFER_SIZE = 1024 * 1024;

            // Protected types ------------------------------------------------

            protected enum CopyMoveProblemKind
            {
                [LocalizedDescription(nameof(Strings.CopyMove_Question_CannotCreateDestinationFolder), typeof(Strings))]
                [AvailableCopyMoveResolutions(SingleCopyMoveProblemResolution.Skip, SingleCopyMoveProblemResolution.SkipAll, SingleCopyMoveProblemResolution.Abort)]
                CannotCreateDestinationFolder,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_CannotEnterDestinationFolder), typeof(Strings))]
                [AvailableCopyMoveResolutions(SingleCopyMoveProblemResolution.Skip, SingleCopyMoveProblemResolution.SkipAll, SingleCopyMoveProblemResolution.Abort)]
                CannotEnterDestinationFolder,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_CannotEnterSourceFolder), typeof(Strings))]
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
                [LocalizedDescription(nameof(Strings.CopyMove_Question_CannotChangeFileAttributes), typeof(Strings))]
                [AvailableCopyMoveResolutions(SingleCopyMoveProblemResolution.Skip, SingleCopyMoveProblemResolution.SkipAll, SingleCopyMoveProblemResolution.Abort)]
                CannotChangeFileAttributes,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_CannotCopyFile), typeof(Strings))]
                [AvailableCopyMoveResolutions(SingleCopyMoveProblemResolution.Skip, SingleCopyMoveProblemResolution.SkipAll, SingleCopyMoveProblemResolution.Abort)]
                CannotCopyFile,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_CannotRemoveSourceFile), typeof(Strings))]
                [AvailableCopyMoveResolutions(SingleCopyMoveProblemResolution.Skip, SingleCopyMoveProblemResolution.SkipAll, SingleCopyMoveProblemResolution.Abort)]
                CannotDeleteSourceFile,
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
                FolderCopiedIntoItself,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_CannotDeleteEmptyFolder), typeof(Strings))]
                [AvailableCopyMoveResolutions(SingleCopyMoveProblemResolution.Skip, SingleCopyMoveProblemResolution.SkipAll, SingleCopyMoveProblemResolution.Abort)]
                CannotDeleteEmptyFolder,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_CannotCheckIfSubfolderIsEmpty), typeof(Strings))]
                [AvailableCopyMoveResolutions(SingleCopyMoveProblemResolution.Skip, SingleCopyMoveProblemResolution.SkipAll, SingleCopyMoveProblemResolution.Abort)]
                CannotCheckIfSubfolderIsEmpty,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_CannotCheckIfSourceFileExists), typeof(Strings))]
                [AvailableCopyMoveResolutions(SingleCopyMoveProblemResolution.Skip, SingleCopyMoveProblemResolution.SkipAll, SingleCopyMoveProblemResolution.Abort)]
                CannotCheckIfSourceFileExists,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_CannotCheckIfSourceFolderExists), typeof(Strings))]
                [AvailableCopyMoveResolutions(SingleCopyMoveProblemResolution.Skip, SingleCopyMoveProblemResolution.SkipAll, SingleCopyMoveProblemResolution.Abort)]
                CannotCheckIfSourceFolderExists,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_CannotCheckIfDestinationFileExists), typeof(Strings))]
                [AvailableCopyMoveResolutions(SingleCopyMoveProblemResolution.Skip, SingleCopyMoveProblemResolution.SkipAll, SingleCopyMoveProblemResolution.Abort)]
                CannotCheckIfDestinationFileExists,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_CannotCheckIfDestinationFolderExists), typeof(Strings))]
                [AvailableCopyMoveResolutions(SingleCopyMoveProblemResolution.Skip, SingleCopyMoveProblemResolution.SkipAll, SingleCopyMoveProblemResolution.Abort)]
                CannotCheckIfDestinationFolderExists,
                [LocalizedDescription(nameof(Strings.CopyMove_Question_DestinationFolderAlreadyExists), typeof(Strings))]
                [AvailableCopyMoveResolutions(SingleCopyMoveProblemResolution.Skip, SingleCopyMoveProblemResolution.SkipAll, SingleCopyMoveProblemResolution.Abort)]
                DestinationFolderAlreadyExists
            }

            // Private fields -------------------------------------------------

            private readonly SemaphoreSlim userDecisionSemaphore;
            private readonly Dictionary<CopyMoveProblemKind, GenericCopyMoveProblemResolution> problemResolutions = new();

            // Private methods ------------------------------------------------

            private (bool exit, CopyMoveWorkerResult result) HandleSkipAbort(CopyMoveProblemKind problemKind,
                FilesystemOperator sourceOperator,
                FilesystemOperator destinationOperator,
                string itemName)
            {
                var resolution = GetResolutionFor(problemKind,
                                        sourceOperator.CurrentPath,
                                        destinationOperator.CurrentPath,
                                        itemName);

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

            private (bool exit, CopyMoveWorkerResult result) HandleSkipRenameAbort(CopyMoveProblemKind fileCopiedIntoItself,
                IBaseItemInfo fileInfo,
                FilesystemOperator sourceOperator,
                FilesystemOperator destinationOperator,
                ref string targetName)
            {
                var resolution = GetResolutionFor(fileCopiedIntoItself,
                                            sourceOperator.CurrentPath,
                                            destinationOperator.CurrentPath,
                                            targetName);

                switch (resolution)
                {
                    case GenericCopyMoveProblemResolution.Skip:
                        return (true, null);
                    case GenericCopyMoveProblemResolution.Rename:
                        {
                            (bool exit, CopyMoveWorkerResult result) = FindAvailableDestinationName(fileInfo, sourceOperator, destinationOperator, ref targetName);
                            if (exit)
                                return (true, result);

                            return (false, null);
                        }
                    case GenericCopyMoveProblemResolution.Abort:
                        return (true, new AbortedCopyMoveWorkerResult());
                    default:
                        throw new InvalidOperationException("Invalid problem resolution!");
                }
            }

            private (bool exit, CopyMoveWorkerResult result) HandleSkipIgnoreAbort(CopyMoveProblemKind problemKind,
                FilesystemOperator sourceOperator,
                FilesystemOperator destinationOperator,
                string itemName)
            {
                var resolution = GetResolutionFor(problemKind,
                            sourceOperator.CurrentPath,
                            destinationOperator.CurrentPath,
                            itemName);

                switch (resolution)
                {
                    case GenericCopyMoveProblemResolution.Skip:
                        return (true, null);
                    case GenericCopyMoveProblemResolution.Ignore:
                        // Name won't be changed
                        return (false, null);
                    case GenericCopyMoveProblemResolution.Abort:
                        return (true, new AbortedCopyMoveWorkerResult());
                    default:
                        throw new InvalidOperationException("Invalid problem resolution!");
                }
            }

            private (bool exit, CopyMoveWorkerResult result) HandleSkipOverwriteRenameAbort(CopyMoveProblemKind destinationFileAlreadyExists,
                IFileInfo fileInfo,
                FilesystemOperator sourceOperator,
                FilesystemOperator destinationOperator,
                ref string targetName)
            {
                var resolution = GetResolutionFor(destinationFileAlreadyExists,
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
                        return (false, null);
                    case GenericCopyMoveProblemResolution.Rename:
                        {
                            (bool exit, CopyMoveWorkerResult result) = FindAvailableDestinationName(fileInfo, sourceOperator, destinationOperator, ref targetName);
                            if (exit)
                                return (true, result);

                            return (false, null);
                        }
                    default:
                        throw new InvalidOperationException("Invalid resolution!");
                }
            }

            private (bool exit, CopyMoveWorkerResult result) HandleSkipRenameOverwriteWithAttributeChangeAbort(CopyMoveProblemKind problemKind,
                IFileInfo fileInfo,
                FilesystemOperator sourceOperator,
                FilesystemOperator destinationOperator,
                FileAttributes attribute,
                ref string targetName,
                ref FileAttributes? attributes)
            {
                var resolution = GetResolutionFor(problemKind,
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
                        {
                            (bool exit, CopyMoveWorkerResult result) = FindAvailableDestinationName(fileInfo, sourceOperator, destinationOperator, ref targetName);
                            if (exit)
                                return (true, result);

                            return (false, null);
                        }
                    case GenericCopyMoveProblemResolution.Overwrite:
                        {
                            attributes &= ~attribute;

                            if (!destinationOperator.SetFileAttributes(targetName, attributes.Value))
                            {
                                return HandleSkipAbort(CopyMoveProblemKind.CannotChangeFileAttributes,
                                    sourceOperator,
                                    destinationOperator,
                                    targetName);
                            }

                            return (false, null);
                        }
                    default:
                        throw new InvalidOperationException("Invalid resolution!");
                }
            }

            private (bool exit, CopyMoveWorkerResult result) HandleIgnoreAbort(CopyMoveProblemKind problemKind,
                FilesystemOperator sourceOperator,
                FilesystemOperator destinationOperator,
                string targetName)
            {
                var resolution = GetResolutionFor(problemKind,
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

            private (bool exit, CopyMoveWorkerResult result) RenameTargetFile(TContext context,
                            FilesystemOperator sourceOperator,
                            FilesystemOperator destinationOperator,
                            ref string targetName)
            {
                if (context.Configuration.RenameFrom.IsMatch(targetName))
                {
                    var newName = context.Configuration.RenameFrom.Replace(targetName, context.Configuration.RenameTo);

                    // New name may be invalid                    
                    if (string.IsNullOrEmpty(newName) || newName.Any(c => invalidChars.Contains(c)))
                    {
                        return HandleSkipIgnoreAbort(CopyMoveProblemKind.InvalidRenamedFilename,
                            sourceOperator,
                            destinationOperator,
                            newName);
                    }
                    else
                    {
                        targetName = newName;
                    }
                }

                return (false, null);
            }

            private (bool exit, CopyMoveWorkerResult result) EnsureFileNotCopiedOnItself(IFileInfo fileInfo,
                FilesystemOperator sourceOperator,
                FilesystemOperator destinationOperator,
                ref string targetName)
            {
                if (sourceOperator.CurrentPath == destinationOperator.CurrentPath &&
                    fileInfo.Name == targetName)
                {
                    return HandleSkipRenameAbort(CopyMoveProblemKind.FileCopiedIntoItself,
                        fileInfo,
                        sourceOperator,
                        destinationOperator,
                        ref targetName);
                }

                return (false, null);
            }

            private (bool exit, CopyMoveWorkerResult result) EnsureFolderNotCopiedOnItself(IFolderInfo folderInfo,
                FilesystemOperator sourceOperator,
                FilesystemOperator destinationOperator,
                ref string targetName)
            {
                if (sourceOperator.CurrentPath == destinationOperator.CurrentPath &&
                    folderInfo.Name == targetName)
                {
                    return HandleSkipRenameAbort(CopyMoveProblemKind.FolderCopiedIntoItself,
                        folderInfo,
                        sourceOperator,
                        destinationOperator,
                        ref targetName);
                }

                return (false, null);
            }

            private (bool exit, CopyMoveWorkerResult result) DeleteEmptySourceFolder(IFolderInfo folderInfo,
                FilesystemOperator sourceOperator,
                FilesystemOperator destinationOperator)
            {
                if (!sourceOperator.DeleteEmptyFolder(folderInfo.Name))
                {
                    return HandleSkipAbort(CopyMoveProblemKind.CannotDeleteEmptyFolder,
                            sourceOperator,
                            destinationOperator,
                            folderInfo.Name);
                }

                return (false, null);
            }

            private (bool exit, CopyMoveWorkerResult result) CheckIfSourceSubfolderEmpty(IFolderInfo folderInfo,
                FilesystemOperator sourceOperator,
                FilesystemOperator destinationOperator,
                ref bool folderEmpty)
            {
                var subfolderEmpty = sourceOperator.CheckIsSubfolderEmpty(folderInfo.Name);

                if (subfolderEmpty == null)
                {
                    return HandleSkipAbort(CopyMoveProblemKind.CannotCheckIfSubfolderIsEmpty,
                            sourceOperator,
                            destinationOperator,
                            folderInfo.Name);
                }

                folderEmpty = subfolderEmpty.Value;
                return (false, null);
            }

            private (bool exit, CopyMoveWorkerResult result) FindAvailableDestinationName(IBaseItemInfo item,
                FilesystemOperator sourceOperator,
                FilesystemOperator destinationOperator,
                ref string newName)
            {
                long i = 1;
                string name = System.IO.Path.GetFileNameWithoutExtension(item.Name);
                string extension = System.IO.Path.GetExtension(item.Name);

                bool? fileExists;
                bool? folderExists;

                do
                {
                    newName = CreateNewName(i, name, extension);
                    i++;

                    fileExists = destinationOperator.FileExists(newName);
                    if (fileExists == null)
                    {
                        return HandleSkipAbort(CopyMoveProblemKind.CannotCheckIfDestinationFileExists,
                            sourceOperator,
                            destinationOperator,
                            newName);
                    }

                    folderExists = destinationOperator.FolderExists(newName);
                    if (folderExists == null)
                    {
                        return HandleSkipAbort(CopyMoveProblemKind.CannotCheckIfDestinationFolderExists,
                            sourceOperator,
                            destinationOperator,
                            newName);
                    }
                }
                while (fileExists == true || folderExists == true);

                return (false, null);

                static string CreateNewName(long i, string name, string extension)
                {
                    return $"{name} ({i}){extension}";
                }
            }

            private (bool exit, CopyMoveWorkerResult result) EnsureSourceFileExists(IFileInfo fileInfo,
                FilesystemOperator sourceOperator,
                FilesystemOperator destinationOperator)
            {
                var fileExists = sourceOperator.FileExists(fileInfo.Name);

                if (fileExists == null)
                {
                    return HandleSkipAbort(CopyMoveProblemKind.CannotCheckIfSourceFileExists,
                            sourceOperator,
                            destinationOperator,
                            fileInfo.Name);
                }
                else if (fileExists == false)
                {
                    return HandleSkipAbort(CopyMoveProblemKind.SourceFileDoesNotExist,
                        sourceOperator,
                        destinationOperator,
                        fileInfo.Name);
                }

                return (false, null);
            }

            private (bool exit, CopyMoveWorkerResult result) EnsureDestinationFileDoesNotExist(IFileInfo fileInfo,
                FilesystemOperator sourceOperator,
                FilesystemOperator destinationOperator,
                ref string targetName)
            {
                var fileExists = destinationOperator.FileExists(targetName);

                if (fileExists == null)
                {
                    return HandleSkipAbort(CopyMoveProblemKind.CannotCheckIfDestinationFileExists,
                            sourceOperator,
                            destinationOperator,
                            targetName);
                }

                if (fileExists == true)
                {
                    return HandleSkipOverwriteRenameAbort(CopyMoveProblemKind.DestinationFileAlreadyExists, 
                        fileInfo, 
                        sourceOperator, 
                        destinationOperator, 
                        ref targetName);
                }

                return (false, null);
            }

            private (bool exit, CopyMoveWorkerResult result) EnsureDestinationFolderDoesNotExist(IFileInfo fileInfo,
                FilesystemOperator sourceOperator,
                FilesystemOperator destinationOperator,
                ref string targetName)
            {
                var folderExists = destinationOperator.FolderExists(targetName);

                if (folderExists == null)
                {
                    return HandleSkipAbort(CopyMoveProblemKind.CannotCheckIfDestinationFolderExists,
                            sourceOperator,
                            destinationOperator,
                            targetName);
                }

                if (folderExists == true)
                {
                    return HandleSkipOverwriteRenameAbort(CopyMoveProblemKind.DestinationFolderAlreadyExists,
                        fileInfo,
                        sourceOperator,
                        destinationOperator,
                        ref targetName);
                }

                return (false, null);
            }

            private (bool exit, CopyMoveWorkerResult result) CheckForOverwritingSpecialFile(IFileInfo fileInfo,
                FilesystemOperator sourceOperator,
                FilesystemOperator destinationOperator,
                FileAttributes attribute,
                CopyMoveProblemKind attributeProblemKind,
                ref string targetName)
            {
                var fileExists = destinationOperator.FileExists(targetName);

                if (fileExists == null)
                {
                    return HandleSkipAbort(CopyMoveProblemKind.CannotCheckIfDestinationFileExists,
                        sourceOperator,
                        destinationOperator,
                        targetName);
                }

                if (fileExists == true)
                {
                    var attributes = destinationOperator.GetFileAttributes(targetName);

                    if (attributes == null)
                    {
                        return HandleSkipAbort(CopyMoveProblemKind.CannotGetTargetFileAttributes,
                            sourceOperator,
                            destinationOperator,
                            targetName);
                    }

                    if (attributes.Value.HasFlag(attribute))
                    {
                        return HandleSkipRenameOverwriteWithAttributeChangeAbort(attributeProblemKind,
                            fileInfo,
                            sourceOperator,
                            destinationOperator,
                            attribute,
                            ref targetName,
                            ref attributes);
                    }
                }

                return (false, null);
            }

            private (bool exit, CopyMoveWorkerResult result) CopyAttributes(IFileInfo fileInfo,
                FilesystemOperator sourceOperator,
                FilesystemOperator destinationOperator,
                string targetName)
            {
                var attributes = sourceOperator.GetFileAttributes(fileInfo.Name);

                if (attributes == null)
                {
                    return HandleSkipAbort(CopyMoveProblemKind.CannotGetSourceFileAttributes,
                        sourceOperator,
                        destinationOperator,
                        fileInfo.Name);
                }

                if (!destinationOperator.SetFileAttributes(targetName, attributes.Value))
                {
                    return HandleIgnoreAbort(CopyMoveProblemKind.CannotSetTargetFileAttributes,
                        sourceOperator,
                        destinationOperator,
                        targetName);
                }

                return (false, null);
            }

            private (bool exit, CopyMoveWorkerResult result) OpenSourceFile(IFileInfo fileInfo,
                FilesystemOperator sourceOperator,
                FilesystemOperator destinationOperator,
                ref Stream sourceStream)
            {
                sourceStream = sourceOperator.OpenFileForReading(fileInfo.Name);
                if (sourceStream == null)
                {
                    return HandleSkipAbort(CopyMoveProblemKind.CannotOpenSourceFile,
                        sourceOperator,
                        destinationOperator,
                        fileInfo.Name);
                }

                return (false, null);
            }

            private (bool exit, CopyMoveWorkerResult result) OpenDestinationFile(IFileInfo fileInfo,
                FilesystemOperator sourceOperator,
                FilesystemOperator destinationOperator,
                string targetName,
                ref Stream destinationStream)
            {
                destinationStream = destinationOperator.OpenFileForWriting(targetName);
                if (destinationStream == null)
                {
                    return HandleSkipAbort(CopyMoveProblemKind.CannotOpenDestinationFile,
                        sourceOperator,
                        destinationOperator,
                        targetName);
                }

                return (false, null);
            }

            private (bool exit, CopyMoveWorkerResult result) DeleteSourceFile(IFileInfo fileInfo,
                FilesystemOperator sourceOperator,
                FilesystemOperator destinationOperator)
            {
                if (!sourceOperator.DeleteFile(fileInfo.Name))
                {
                    return HandleSkipAbort(CopyMoveProblemKind.CannotDeleteSourceFile,
                        sourceOperator,
                        destinationOperator,
                        fileInfo.Name);
                }

                return (false, null);
            }

            private (bool exit, CopyMoveWorkerResult result) CreateDestinationFolder(IFolderInfo folderInfo,
                FilesystemOperator sourceOperator,
                FilesystemOperator destinationOperator,
                string targetName)
            {
                if (!destinationOperator.CreateFolder(targetName))
                {
                    return HandleSkipAbort(CopyMoveProblemKind.CannotCreateDestinationFolder,
                        sourceOperator,
                        destinationOperator,
                        targetName);
                }

                return (false, null);
            }

            private (bool exit, CopyMoveWorkerResult result) EnterSourceFolder(IFolderInfo folderInfo,
                FilesystemOperator sourceOperator,
                FilesystemOperator destinationOperator,
                ref FilesystemOperator sourceFolderOperator)
            {
                sourceFolderOperator = sourceOperator.EnterFolder(folderInfo.Name);

                // Enter folder in local location
                if (sourceFolderOperator == null)
                {
                    return HandleSkipAbort(CopyMoveProblemKind.CannotEnterSourceFolder,
                        sourceOperator,
                        destinationOperator,
                        folderInfo.Name);
                }

                return (false, null);
            }

            private (bool exit, CopyMoveWorkerResult result) EnterDestinationFolder(IFolderInfo folderInfo,
                FilesystemOperator sourceOperator,
                FilesystemOperator destinationOperator,
                string targetName,
                ref FilesystemOperator destinationFolderOperator)
            {
                destinationFolderOperator = destinationOperator.EnterFolder(targetName);

                // Enter folder in remote location
                if (destinationFolderOperator == null)
                {
                    return HandleSkipAbort(CopyMoveProblemKind.CannotEnterDestinationFolder,
                        sourceOperator,
                        destinationOperator,
                        targetName);
                }

                return (false, null);
            }

            private CopyMoveWorkerResult ProcessFile(TContext context,
                IFileInfo fileInfo,
                DataTransferOperationType operationType,
                FilesystemOperator sourceOperator,
                FilesystemOperator destinationOperator,
                bool isRoot)
            {
                try
                {
                    bool exit;
                    CopyMoveWorkerResult result;

                    // Check if source file exists

                    (exit, result) = EnsureSourceFileExists(fileInfo, sourceOperator, destinationOperator);
                    if (exit)
                        return result;

                    // Generate new name for the file

                    string targetName = fileInfo.Name;
                    if (context.Configuration.Rename &&
                        context.Configuration.RenameFiles &&
                        (isRoot || context.Configuration.RenameRecursive))
                    {
                        (exit, result) = RenameTargetFile(context, sourceOperator, destinationOperator, ref targetName);
                        if (exit)
                            return result;
                    }

                    // Check if file is not being copied into itself

                    (exit, result) = EnsureFileNotCopiedOnItself(fileInfo, sourceOperator, destinationOperator, ref targetName);
                    if (exit)
                        return result;

                    // Check if destination does not contain folder with the same name
                    (exit, result) = EnsureDestinationFolderDoesNotExist(fileInfo, sourceOperator, destinationOperator, ref targetName);
                    if (exit)
                        return result;

                    // Ask about overwriting existing file

                    (exit, result) = EnsureDestinationFileDoesNotExist(fileInfo, sourceOperator, destinationOperator, ref targetName);
                    if (exit)
                        return result;

                    // Ask about overwriting readonly file

                    (exit, result) = CheckForOverwritingSpecialFile(fileInfo, sourceOperator, destinationOperator, FileAttributes.ReadOnly, CopyMoveProblemKind.DestinationFileIsReadOnly, ref targetName);
                    if (exit)
                        return result;

                    // Ask about overwriting system file

                    (exit, result) = CheckForOverwritingSpecialFile(fileInfo, sourceOperator, destinationOperator, FileAttributes.System, CopyMoveProblemKind.DestinationFileIsSystem, ref targetName);
                    if (exit)
                        return result;

                    // Get the source stream

                    Stream sourceStream = null;
                    (exit, result) = OpenSourceFile(fileInfo, sourceOperator, destinationOperator, ref sourceStream);
                    if (exit)
                        return result;

                    // Get the destination stream

                    Stream destinationStream = null;
                    (exit, result) = OpenDestinationFile(fileInfo, sourceOperator, destinationOperator, targetName, ref destinationStream);
                    if (exit)
                        return result;

                    // Start copying

                    bool cancelled = false;

                    try
                    {
                        (exit, result) = CopyFile(context, fileInfo, sourceStream, destinationStream, buffer, ref cancelled);
                        if (exit)
                            return result;

                        (exit, result) = CopyAttributes(fileInfo, sourceOperator, destinationOperator, targetName);
                        if (exit)
                            return result;
                    }
                    catch
                    {
                        destinationStream?.Dispose();
                        destinationStream = null;

                        // Try to delete partially-copied file
                        destinationOperator.DeleteFile(targetName);

                        var resolution = GetResolutionFor(CopyMoveProblemKind.CannotCopyFile,
                            sourceOperator.CurrentPath,
                            destinationOperator.CurrentPath,
                            fileInfo.Name);

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
                        sourceOperator.CloseReadFile(sourceStream, fileInfo.Name);
                        destinationOperator.CloseWrittenFile(destinationStream, fileInfo.Name);

                        if (cancelled)
                        {
                            // Try to remove file, which was not copied
                            destinationOperator.DeleteFile(targetName);
                        }
                    }

                    if (operationType == DataTransferOperationType.Move)
                    {
                        (exit, result) = DeleteSourceFile(fileInfo, sourceOperator, destinationOperator);
                        if (exit)
                            return result;
                    }
                }
                finally
                {
                    context.CopiedSize += fileInfo.Size;
                    context.CopiedFiles++;
                }

                return null;
            }

            private CopyMoveWorkerResult ProcessFolder(TContext context,
                IFolderInfo folderInfo,
                DataTransferOperationType operationType,
                FilesystemOperator sourceOperator,
                FilesystemOperator destinationOperator,
                bool isRoot)
            {
                bool exit;
                CopyMoveWorkerResult result;

                string targetName = folderInfo.Name;
                if (context.Configuration.Rename &&
                    context.Configuration.RenameFolders &&
                    (isRoot || context.Configuration.RenameRecursive))
                {
                    (exit, result) = RenameTargetFile(context, sourceOperator, destinationOperator, ref targetName);
                    if (exit)
                        return result;
                }

                if (!context.Configuration.Rename)
                {
                    // Copying folder on itself makes sense only if rename is in place.

                    (exit, result) = EnsureFolderNotCopiedOnItself(folderInfo, sourceOperator, destinationOperator, ref targetName);
                    if (exit)
                        return result;
                }

                (exit, result) = CreateDestinationFolder(folderInfo, sourceOperator, destinationOperator, targetName);
                if (exit)
                    return result;

                FilesystemOperator sourceFolderOperator = null;

                (exit, result) = EnterSourceFolder(folderInfo, sourceOperator, destinationOperator, ref sourceFolderOperator);
                if (exit)
                    return result;

                FilesystemOperator destinationFolderOperator = null;

                (exit, result) = EnterDestinationFolder(folderInfo, sourceOperator, destinationOperator, targetName, ref destinationFolderOperator);
                if (exit)
                    return result;

                IReadOnlyList<IBaseItemInfo> items = null;
                (exit, result) = RetrieveFolderContents(context, folderInfo, sourceFolderOperator, destinationFolderOperator, ref items);
                if (exit)
                    return result;

                result = ProcessItems(context, items, operationType, sourceFolderOperator, destinationFolderOperator, false);
                if (result != null)
                    return result;

                // In move operation, if all files were deleted, delete the empty folder as well

                if (operationType == DataTransferOperationType.Move)
                {
                    bool folderEmpty = false;
                    (exit, result) = CheckIfSourceSubfolderEmpty(folderInfo, sourceFolderOperator, destinationFolderOperator, ref folderEmpty);
                    if (exit)
                        return result;

                    if (folderEmpty)
                    {
                        (exit, result) = DeleteEmptySourceFolder(folderInfo, sourceFolderOperator, destinationFolderOperator);
                        if (exit)
                            return result;
                    }
                }

                return null;
            }

            // Protected fields -----------------------------------------------

            protected readonly byte[] buffer = new byte[BUFFER_SIZE];
            protected readonly char[] invalidChars;

            // Protected methods ----------------------------------------------

            protected BaseCopyMoveWorker()
            {
                userDecisionSemaphore = new SemaphoreSlim(0, 1);
                invalidChars = System.IO.Path.GetInvalidFileNameChars();
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

            protected CopyMoveWorkerResult ProcessItems(TContext context,
                IReadOnlyList<IBaseItemInfo> items,
                DataTransferOperationType operationType,
                FilesystemOperator sourceOperator,
                FilesystemOperator destinationOperator,
                bool isRoot)
            {
                foreach (var item in items)
                {
                    if (item is IFolderInfo folderInfo)
                    {
                        var result = ProcessFolder(context, folderInfo, operationType, sourceOperator, destinationOperator, isRoot);
                        if (result != null)
                            return result;
                    }
                    else if (item is IFileInfo fileInfo)
                    {
                        var result = ProcessFile(context, fileInfo, operationType, sourceOperator, destinationOperator, isRoot);
                        if (result != null)
                            return result;
                    }
                    else
                        throw new InvalidOperationException("Invalid plan item!");
                }

                return null;
            }

            protected abstract (bool exit, CopyMoveWorkerResult result) CopyFile(TContext context,
                IFileInfo fileInfo,
                Stream sourceStream,
                Stream destinationStream,
                byte[] buffer,
                ref bool cancelled);

            protected abstract (bool exit, CopyMoveWorkerResult result) RetrieveFolderContents(TContext context,
                IFolderInfo folderInfo,
                FilesystemOperator sourceFolderOperator,
                FilesystemOperator destinationFolderOperator,
                ref IReadOnlyList<IBaseItemInfo> items);

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

        protected readonly FilesystemOperator sourceOperator;
        protected readonly FilesystemOperator destinationOperator;
        protected readonly DataTransferOperationType operationType;
        protected readonly IReadOnlyList<Item> selectedItems;
        protected readonly CopyMoveConfigurationModel configuration;

        // Protected methods --------------------------------------------------

        protected virtual void HandleWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var worker = (ICopyMoveUserDecisionWorker)sender;

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

        protected virtual void HandleWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is CriticalFailureCopyMoveWorkerResult critical)
            {
                messagingService.ShowError(critical.LocalizedMessage);
            }

            OnFinished();
        }

        // Public methods -----------------------------------------------------

        public BaseCopyMoveOperationViewModel(IDialogService dialogService,
            IMessagingService messagingService,
            FilesystemOperator sourceOperator,
            FilesystemOperator destinationOperator,
            IReadOnlyList<Item> selectedItems,
            CopyMoveConfigurationModel configuration,
            DataTransferOperationType operationType)
            : base(dialogService, messagingService)
        {
            this.Title = operationType switch
            {
                DataTransferOperationType.Move => Strings.CopyMove_Title_MovingFiles,
                DataTransferOperationType.Copy => Strings.CopyMove_Title_CopyingFiles,
                _ => throw new InvalidOperationException("Unsupported opertion type!")
            };

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

        // Private methods ----------------------------------------------------        
    }
}
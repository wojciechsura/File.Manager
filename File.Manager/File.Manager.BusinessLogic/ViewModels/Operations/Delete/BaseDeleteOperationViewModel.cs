using BindingEnums;
using File.Manager.API.Filesystem.Models.Items.Listing;
using File.Manager.API.Filesystem.Models.Items.Plan;
using File.Manager.API.Filesystem.Models.Items;
using File.Manager.API.Filesystem;
using File.Manager.BusinessLogic.Attributes;
using File.Manager.BusinessLogic.Models.Dialogs.DeleteConfiguration;
using File.Manager.BusinessLogic.Services.Dialogs;
using File.Manager.BusinessLogic.Services.Messaging;
using File.Manager.BusinessLogic.Types;
using File.Manager.Common.Helpers;
using File.Manager.Resources.Operations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Xml.Linq;
using System.Reflection;
using System.Drawing.Printing;
using System.Windows.Controls;

namespace File.Manager.BusinessLogic.ViewModels.Operations.Delete
{
    public abstract class BaseDeleteOperationViewModel : BaseOperationViewModel
    {
        // Protected types ----------------------------------------------------

        // Input

        protected sealed class DeleteWorkerInput
        {
            public DeleteWorkerInput(IFilesystemOperator filesystemOperator,
                DeleteConfigurationModel configuration, 
                IReadOnlyList<Item> selectedItems)
            {
                FilesystemOperator = filesystemOperator;
                Configuration = configuration;
                SelectedItems = selectedItems;
            }

            public IFilesystemOperator FilesystemOperator { get; }
            public DeleteConfigurationModel Configuration { get; }
            public IReadOnlyList<Item> SelectedItems { get; }
        }

        // Progress

        protected class UserQuestionRequestProgress
        {
            public UserQuestionRequestProgress(SingleDeleteProblemResolution[] availableResolutions, string header)
            {
                AvailableResolutions = availableResolutions;
                Header = header;
            }

            public SingleDeleteProblemResolution[] AvailableResolutions { get; }
            public string Header { get; }
        }

        protected class DeleteProgress
        {
            public DeleteProgress(int progress, string description)
            {
                Progress = progress;
                Description = description;
            }

            public int Progress { get; }
            public string Description { get; }
        }

        // Results

        protected abstract class DeleteWorkerResult
        {

        }

        protected sealed class AbortedDeleteWorkerResult : DeleteWorkerResult
        {
            public AbortedDeleteWorkerResult()
            {

            }
        }

        protected sealed class CancelledDeleteWorkerResult : DeleteWorkerResult
        {
            public CancelledDeleteWorkerResult()
            {

            }
        }

        protected sealed class CriticalFailureDeleteWorkerResult : DeleteWorkerResult
        {
            public CriticalFailureDeleteWorkerResult(string localizedMessage)
            {
                LocalizedMessage = localizedMessage;
            }

            public string LocalizedMessage { get; }
        }

        protected sealed class SuccessDeleteWorkerResult : DeleteWorkerResult
        {
            public SuccessDeleteWorkerResult()
            {

            }
        }

        // Worker

        protected abstract class BaseDeleteWorkerContext
        {
            protected BaseDeleteWorkerContext(DeleteConfigurationModel configuration)
            {
                Configuration = configuration;
            }

            public long DeletedSize { get; set; }
            public int DeletedFiles { get; set; }
            public DeleteConfigurationModel Configuration { get; }
        }

        protected abstract class BaseDeleteWorker<TContext> : BaseWorker, IDeleteUserDecisionWorker
            where TContext : BaseDeleteWorkerContext
        {
            // Protected types ------------------------------------------------

            protected enum DeleteProblemKind
            {
                [LocalizedDescription(nameof(Strings.Delete_Question_CannotGetFileAttributes), typeof(Strings))]
                [AvailableDeleteResolutions(SingleDeleteProblemResolution.Skip, SingleDeleteProblemResolution.SkipAll, SingleDeleteProblemResolution.Abort)]
                CannotGetFileAttributes,
                [LocalizedDescription(nameof(Strings.Delete_Question_DeletedFileIsReadOnly), typeof(Strings))]
                [AvailableDeleteResolutions(SingleDeleteProblemResolution.Delete, SingleDeleteProblemResolution.DeleteAll, SingleDeleteProblemResolution.Skip, SingleDeleteProblemResolution.SkipAll, SingleDeleteProblemResolution.Abort)]
                DeletedFileIsReadOnly,
                [LocalizedDescription(nameof(Strings.Delete_Question_DeletedFileIsHidden), typeof(Strings))]
                [AvailableDeleteResolutions(SingleDeleteProblemResolution.Delete, SingleDeleteProblemResolution.DeleteAll, SingleDeleteProblemResolution.Skip, SingleDeleteProblemResolution.SkipAll, SingleDeleteProblemResolution.Abort)]
                DeletedFileIsHidden,
                [LocalizedDescription(nameof(Strings.Delete_Question_DeletedFileIsSystem), typeof(Strings))]
                [AvailableDeleteResolutions(SingleDeleteProblemResolution.Delete, SingleDeleteProblemResolution.DeleteAll, SingleDeleteProblemResolution.Skip, SingleDeleteProblemResolution.SkipAll, SingleDeleteProblemResolution.Abort)]
                DeletedFileIsSystem,
                [LocalizedDescription(nameof(Strings.Delete_Question_CannotSetFileAttributes), typeof(Strings))]
                [AvailableDeleteResolutions(SingleDeleteProblemResolution.Delete, SingleDeleteProblemResolution.DeleteAll, SingleDeleteProblemResolution.Skip, SingleDeleteProblemResolution.SkipAll, SingleDeleteProblemResolution.Abort)]
                CannotSetFileAttributes,
                [LocalizedDescription(nameof(Strings.Delete_Question_CannotDeleteFile), typeof(Strings))]
                [AvailableDeleteResolutions(SingleDeleteProblemResolution.Skip, SingleDeleteProblemResolution.SkipAll, SingleDeleteProblemResolution.Abort)]
                CannotDeleteFile,
                [LocalizedDescription(nameof(Strings.Delete_Question_CannotEnterFolder), typeof(Strings))]
                [AvailableDeleteResolutions(SingleDeleteProblemResolution.Skip, SingleDeleteProblemResolution.SkipAll, SingleDeleteProblemResolution.Abort)]
                CannotEnterFolder,
                [LocalizedDescription(nameof(Strings.Delete_Question_CannotDeleteFolder), typeof(Strings))]
                [AvailableDeleteResolutions(SingleDeleteProblemResolution.Skip, SingleDeleteProblemResolution.SkipAll, SingleDeleteProblemResolution.Abort)]
                CannotDeleteFolder,
                [LocalizedDescription(nameof(Strings.Delete_Question_FileDoesNotExist), typeof(Strings))]
                [AvailableDeleteResolutions(SingleDeleteProblemResolution.Skip, SingleDeleteProblemResolution.SkipAll, SingleDeleteProblemResolution.Abort)]
                FileDoesNotExist,
                [LocalizedDescription(nameof(Strings.Delete_Question_CannotCheckIfSubfolderIsEmpty), typeof(Strings))]
                [AvailableDeleteResolutions(SingleDeleteProblemResolution.Skip, SingleDeleteProblemResolution.SkipAll, SingleDeleteProblemResolution.Abort)]
                CannotCheckIfSubfolderIsEmpty,
                [LocalizedDescription(nameof(Strings.Delete_Question_CannotListFolderContents), typeof(Strings))]
                [AvailableDeleteResolutions(SingleDeleteProblemResolution.Skip, SingleDeleteProblemResolution.SkipAll, SingleDeleteProblemResolution.Abort)]
                CannotListFolderContents,
                [LocalizedDescription(nameof(Strings.Delete_Question_CannotCheckIfFileExists), typeof(Strings))]
                [AvailableDeleteResolutions(SingleDeleteProblemResolution.Skip, SingleDeleteProblemResolution.SkipAll, SingleDeleteProblemResolution.Abort)]
                CannotCheckIfFileExists
            }

            // Private fields -------------------------------------------------

            private readonly SemaphoreSlim userDecisionSemaphore;
            private readonly Dictionary<DeleteProblemKind, GenericDeleteProblemResolution> problemResolutions = new();

            // Private methods ------------------------------------------------

            private (bool exit, DeleteWorkerResult result) HandleSkipAbort(DeleteProblemKind problemKind,
                string itemName,
                IFilesystemOperator filesystemOperator)
            {
                var resolution = GetResolutionFor(problemKind,
                    filesystemOperator.CurrentPath,
                    itemName);

                switch (resolution)
                {
                    case GenericDeleteProblemResolution.Skip:
                        return (true, null);
                    case GenericDeleteProblemResolution.Abort:
                        return (true, new AbortedDeleteWorkerResult());
                    default:
                        throw new InvalidOperationException("Invalid problem resolution!");
                }
            }

            private (bool exit, DeleteWorkerResult reuslt) HandleSkipDeleteWithAttributeChangeAbort(DeleteProblemKind problemKind,
                            string itemName,
                            IFilesystemOperator filesystemOperator,
                            ref FileAttributes fileAttributes,
                            FileAttributes attribute)
            {
                var resolution = GetResolutionFor(DeleteProblemKind.DeletedFileIsReadOnly,
                    filesystemOperator.CurrentPath,
                    itemName);

                switch (resolution)
                {
                    case GenericDeleteProblemResolution.Skip:
                        return (true, null);
                    case GenericDeleteProblemResolution.Abort:
                        return (true, new AbortedDeleteWorkerResult());
                    case GenericDeleteProblemResolution.Delete:
                        {
                            fileAttributes &= ~attribute;

                            if (!filesystemOperator.SetFileAttributes(itemName, fileAttributes))
                            {
                                return HandleSkipAbort(problemKind,
                                    itemName,
                                    filesystemOperator);
                            }

                            return (false, null);
                        }
                    default:
                        throw new InvalidOperationException("Invalid resolution!");
                }
            }

            private (bool exit, DeleteWorkerResult result) CheckIfSubfolderEmpty(IFolderInfo folderInfo,
                IFilesystemOperator filesystemOperator,
                ref bool folderEmpty)
            {
                var subfolderEmpty = filesystemOperator.CheckIsSubfolderEmpty(folderInfo.Name);

                if (subfolderEmpty == null)
                {
                    return HandleSkipAbort(DeleteProblemKind.CannotCheckIfSubfolderIsEmpty, 
                        folderInfo.Name, 
                        filesystemOperator);
                }

                folderEmpty = subfolderEmpty.Value;
                return (false, null);
            }

            private (bool exit, DeleteWorkerResult result) EnsureFileExists(IFileInfo fileInfo,
                IFilesystemOperator filesystemOperator)
            {
                var fileExists = filesystemOperator.FileExists(fileInfo.Name);
                if (fileExists == null)
                {
                    return HandleSkipAbort(DeleteProblemKind.CannotCheckIfFileExists,
                        fileInfo.Name,
                        filesystemOperator);                    
                }

                if (fileExists == false)
                {
                    var resolution = GetResolutionFor(DeleteProblemKind.FileDoesNotExist,
                        filesystemOperator.CurrentPath,
                        fileInfo.Name);

                    switch (resolution)
                    {
                        case GenericDeleteProblemResolution.Skip:
                            return (true, null);
                        case GenericDeleteProblemResolution.Abort:
                            return (true, new AbortedDeleteWorkerResult());
                        default:
                            throw new InvalidOperationException("Invalid resolution!");
                    }
                }

                return (false, null);
            }

            private (bool exit, DeleteWorkerResult result) CheckForDeletingSpecialFile(IFileInfo fileInfo,
                IFilesystemOperator filesystemOperator)
            {
                (bool exit, DeleteWorkerResult result) ValidateAttribute(System.IO.FileAttributes fileAttributes, System.IO.FileAttributes attribute, DeleteProblemKind problemKind)
                {
                    if (fileAttributes.HasFlag(attribute))
                    {
                        return HandleSkipDeleteWithAttributeChangeAbort(problemKind,
                            fileInfo.Name,
                            filesystemOperator,
                            ref fileAttributes,
                            attribute);
                    }

                    return (false, null);
                }

                var fileExists = filesystemOperator.FileExists(fileInfo.Name);
                if (fileExists == null)
                {
                    return HandleSkipAbort(DeleteProblemKind.CannotCheckIfFileExists,
                        fileInfo.Name,
                        filesystemOperator);
                }

                if (fileExists == true)
                {
                    var attributes = filesystemOperator.GetFileAttributes(fileInfo.Name);

                    if (attributes == null)
                    {
                        return HandleSkipAbort(DeleteProblemKind.CannotGetFileAttributes,
                            fileInfo.Name, 
                            filesystemOperator);                        
                    }

                    bool exit;
                    DeleteWorkerResult result;

                    (exit, result) = ValidateAttribute(attributes.Value, FileAttributes.ReadOnly, DeleteProblemKind.DeletedFileIsReadOnly);
                    if (exit)
                        return (true, result);

                    (exit, result) = ValidateAttribute(attributes.Value, FileAttributes.Hidden, DeleteProblemKind.DeletedFileIsHidden);
                    if (exit)
                        return (true, result);

                    (exit, result) = ValidateAttribute(attributes.Value, FileAttributes.System, DeleteProblemKind.DeletedFileIsSystem);
                    if (exit)
                        return (true, result);
                }

                return (false, null);
            }

            private (bool exit, DeleteWorkerResult) DeleteEmptyFolder(IFolderInfo folderInfo,
                IFilesystemOperator filesystemOperator)
            {
                if (!filesystemOperator.DeleteEmptyFolder(folderInfo.Name))
                {
                    return HandleSkipAbort(DeleteProblemKind.CannotDeleteFolder,
                        folderInfo.Name,
                        filesystemOperator);                    
                }

                return (false, null);
            }

            private (bool exit, DeleteWorkerResult result) EnterFolder(IFolderInfo folderInfo,
                IFilesystemOperator filesystemOperator,
                ref IFilesystemOperator filesystemFolderOperator)
            {
                filesystemFolderOperator = filesystemOperator.EnterFolder(folderInfo.Name);

                // Enter folder in local location
                if (filesystemFolderOperator == null)
                {
                    return HandleSkipAbort(DeleteProblemKind.CannotEnterFolder,
                        folderInfo.Name,
                        filesystemOperator);
                }

                return (false, null);
            }

            private DeleteWorkerResult ProcessFile(TContext context,
                IFileInfo fileInfo,
                IFilesystemOperator filesystemOperator)
            {
                bool exit;
                DeleteWorkerResult result;

                (exit, result) = EnsureFileExists(fileInfo, filesystemOperator);
                if (exit)
                    return result;

                (exit, result) = CheckForDeletingSpecialFile(fileInfo, filesystemOperator);
                if (exit)
                    return result;

                (exit, result) = DeleteFile(context, fileInfo, filesystemOperator);
                if (exit)
                    return result;

                return null;
            }

            private DeleteWorkerResult ProcessFolder(TContext context,
                IFolderInfo folderInfo,
                IFilesystemOperator filesystemOperator)
            {
                bool exit;
                DeleteWorkerResult result;

                IFilesystemOperator folderOperator = null;

                (exit, result) = EnterFolder(folderInfo, filesystemOperator, ref folderOperator);
                if (exit)
                    return result;

                IReadOnlyList<IBaseItemInfo> items = null;
                (exit, result) = RetrieveFolderContents(context, folderInfo, folderOperator, ref items);
                if (exit)
                    return result;

                result = ProcessItems(context, items, folderOperator);
                if (result != null)
                    return result;

                bool folderEmpty = false;
                (exit, result) = CheckIfSubfolderEmpty(folderInfo, filesystemOperator, ref folderEmpty);
                if (exit) 
                    return result;

                if (folderEmpty)
                {
                    (exit, result) = DeleteEmptyFolder(folderInfo, filesystemOperator);
                    if (exit)
                        return result;
                }

                return null;
            }


            // Protected methods ----------------------------------------------

            protected BaseDeleteWorker()
            {
                userDecisionSemaphore = new SemaphoreSlim(0, 1);
            }

            protected DeleteWorkerResult ProcessItems(TContext context,
                IReadOnlyList<IBaseItemInfo> items,
                IFilesystemOperator filesystemOperator)
            {
                foreach (var item in items)
                {
                    if (item is IFolderInfo planFolder)
                    {
                        var result = ProcessFolder(context, planFolder, filesystemOperator);
                        if (result != null)
                            return result;
                    }
                    else if (item is IFileInfo planFile)
                    {
                        var result = ProcessFile(context, planFile, filesystemOperator);
                        if (result != null)
                            return result;
                    }
                    else
                        throw new InvalidOperationException("Invalid plan item!");
                }

                return null;
            }

            protected abstract (bool exit, DeleteWorkerResult result) RetrieveFolderContents(TContext context,
                IFolderInfo folderInfo,
                IFilesystemOperator filesystemOperator,
                ref IReadOnlyList<IBaseItemInfo> items);

            protected GenericDeleteProblemResolution GetResolutionFor(DeleteProblemKind problemKind,
                string address,
                string name)
            {
                if (problemResolutions.TryGetValue(problemKind, out GenericDeleteProblemResolution resolution) && resolution != GenericDeleteProblemResolution.Ask)
                    return resolution;

                LocalizedDescriptionAttribute localizedDescription = problemKind.GetAttribute<LocalizedDescriptionAttribute>();
                string header = string.Format(localizedDescription.Description, address, name);

                AvailableDeleteResolutionsAttribute availableResolutions = problemKind.GetAttribute<AvailableDeleteResolutionsAttribute>();

                var progress = new UserQuestionRequestProgress(availableResolutions.AvailableResolutions, header);
                ReportProgress(0, progress);

                UserDecisionSemaphore.Wait();

                switch (UserDecision)
                {
                    case SingleDeleteProblemResolution.Skip:
                        return GenericDeleteProblemResolution.Skip;
                    case SingleDeleteProblemResolution.SkipAll:
                        problemResolutions[problemKind] = GenericDeleteProblemResolution.Skip;
                        return GenericDeleteProblemResolution.Skip;
                    case SingleDeleteProblemResolution.Delete:
                        return GenericDeleteProblemResolution.Delete;
                    case SingleDeleteProblemResolution.DeleteAll:
                        problemResolutions[problemKind] = GenericDeleteProblemResolution.Delete;
                        return GenericDeleteProblemResolution.Delete;
                    case SingleDeleteProblemResolution.Abort:
                        return GenericDeleteProblemResolution.Abort;
                    default:
                        throw new InvalidOperationException("Unsupported problem resolution!");
                }
            }

            protected virtual (bool exit, DeleteWorkerResult result) DeleteFile(TContext context,
                IFileInfo fileInfo,
                IFilesystemOperator filesystemOperator)
            {
                if (!filesystemOperator.DeleteFile(fileInfo.Name))
                {
                    var resolution = GetResolutionFor(DeleteProblemKind.CannotDeleteFile,
                        filesystemOperator.CurrentPath,
                        fileInfo.Name);

                    switch (resolution)
                    {
                        case GenericDeleteProblemResolution.Skip:
                            return (true, null);
                        case GenericDeleteProblemResolution.Abort:
                            return (true, new AbortedDeleteWorkerResult());
                        default:
                            throw new InvalidOperationException("Invalid resolution!");
                    }
                }

                return (false, null);
            }

            // Public properties ----------------------------------------------

            public SemaphoreSlim UserDecisionSemaphore => userDecisionSemaphore;

            public SingleDeleteProblemResolution? UserDecision { get; set; }
        }

        // Private fields -----------------------------------------------------

        private string address;

        // Protected fields ---------------------------------------------------

        protected readonly IFilesystemOperator filesystemOperator;
        protected readonly IReadOnlyList<Item> selectedItems;
        protected readonly DeleteConfigurationModel configuration;

        // Protected methods --------------------------------------------------

        protected void HandleWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var worker = (IDeleteUserDecisionWorker)sender;

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

        protected void HandleWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is CriticalFailureDeleteWorkerResult critical)
            {
                messagingService.ShowError(critical.LocalizedMessage);
            }

            OnFinished();
        }

        // Public methods -----------------------------------------------------

        public BaseDeleteOperationViewModel(IDialogService dialogService,
            IMessagingService messagingService,
            IFilesystemOperator filesystemOperator,
            IReadOnlyList<Item> selectedItems,
            DeleteConfigurationModel configuration)
            : base(dialogService, messagingService)
        {
            this.filesystemOperator = filesystemOperator;
            this.selectedItems = selectedItems;
            this.configuration = configuration;
        }

        // Public properties --------------------------------------------------

        public string Address
        {
            get => address;
            set => Set(ref address, value);
        }
    }
}

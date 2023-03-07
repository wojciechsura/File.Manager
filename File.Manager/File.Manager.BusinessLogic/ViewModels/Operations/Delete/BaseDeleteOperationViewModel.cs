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

namespace File.Manager.BusinessLogic.ViewModels.Operations.Delete
{
    public abstract class BaseDeleteOperationViewModel : BaseOperationViewModel
    {
        // Protected types ----------------------------------------------------

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

        protected abstract class BaseDeleteWorker<TContext> : BackgroundWorker
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
                CannotDeleteFolder
            }

            // Private fields -------------------------------------------------

            private readonly SemaphoreSlim userDecisionSemaphore;
            private readonly Dictionary<DeleteProblemKind, GenericDeleteProblemResolution> problemResolutions = new();

            // Protected methods ----------------------------------------------

            protected BaseDeleteWorker()
            {
                userDecisionSemaphore = new SemaphoreSlim(0, 1);
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

            protected (bool exit, DeleteWorkerResult result) CheckForDeletingSpecialFile(IFileInfo fileInfo,
                IFilesystemOperator filesystemOperator)
            {
                (bool exit, DeleteWorkerResult result) ValidateAttribute(System.IO.FileAttributes fileAttributes, System.IO.FileAttributes attribute, DeleteProblemKind problemKind)
                {
                    if (fileAttributes.HasFlag(attribute))
                    {
                        var resolution = GetResolutionFor(DeleteProblemKind.DeletedFileIsReadOnly,
                            filesystemOperator.CurrentPath,
                            fileInfo.Name);

                        switch (resolution)
                        {
                            case GenericDeleteProblemResolution.Skip:
                                return (true, null);
                            case GenericDeleteProblemResolution.Abort:
                                return (true, new AbortedDeleteWorkerResult());
                            case GenericDeleteProblemResolution.Delete:
                                {
                                    fileAttributes &= ~attribute;

                                    if (!filesystemOperator.SetFileAttributes(fileInfo.Name, fileAttributes))
                                    {
                                        var innerResolution = GetResolutionFor(problemKind,
                                            filesystemOperator.CurrentPath,
                                            fileInfo.Name);

                                        switch (innerResolution)
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
                            default:
                                throw new InvalidOperationException("Invalid resolution!");
                        }

                    }

                    return (false, null);
                }

                if (filesystemOperator.FileExists(fileInfo.Name))
                {
                    var attributes = filesystemOperator.GetFileAttributes(fileInfo.Name);

                    if (attributes == null)
                    {
                        var resolution = GetResolutionFor(DeleteProblemKind.CannotGetFileAttributes,                            
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
           
            protected (bool exit, DeleteWorkerResult result) DeleteFile(IFileInfo fileInfo,
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

            protected (bool exit, DeleteWorkerResult) DeleteEmptyFolder(IFolderInfo folderInfo,
                IFilesystemOperator filesystemOperator)
            {
                if (!filesystemOperator.DeleteFolder(folderInfo.Name, true))
                {
                    var resolution = GetResolutionFor(DeleteProblemKind.CannotDeleteFolder,
                        filesystemOperator.CurrentPath,
                        folderInfo.Name);

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

            protected (bool exit, DeleteWorkerResult result) EnterFolder(IFolderInfo folderInfo,
                IFilesystemOperator filesystemOperator,
                ref IFilesystemOperator filesystemFolderOperator)
            {
                filesystemFolderOperator = filesystemOperator.EnterFolder(folderInfo.Name);

                // Enter folder in local location
                if (filesystemFolderOperator == null)
                {
                    var resolution = GetResolutionFor(DeleteProblemKind.CannotEnterFolder,
                        filesystemOperator.CurrentPath,
                        folderInfo.Name);

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

                return (false, null);
            }

            protected (DeleteWorkerResult result, bool allDeleted) ProcessItems(TContext context,
                IReadOnlyList<IBaseItemInfo> items,
                IFilesystemOperator filesystemOperator)
            {
                bool allItemsDeleted = true;

                foreach (var item in items)
                {
                    if (item is IFolderInfo planFolder)
                    {
                        (var result, bool deleted) = ProcessFolder(context, planFolder, filesystemOperator);
                        allItemsDeleted &= deleted;
                        if (result != null)
                            return (result, false);
                    }
                    else if (item is IFileInfo planFile)
                    {
                        (var result, bool deleted) = ProcessFile(context, planFile, filesystemOperator);
                        allItemsDeleted &= deleted;
                        if (result != null)
                            return (result, false);
                    }
                    else
                        throw new InvalidOperationException("Invalid plan item!");
                }

                return (null, allItemsDeleted);
            }

            protected (DeleteWorkerResult result, bool deleted) ProcessFile(TContext context,
                IFileInfo operatorFile,
                IFilesystemOperator filesystemOperator)
            {
                throw new NotImplementedException();
            }

            private (DeleteWorkerResult result, bool deleted) ProcessFolder(TContext context,
                IFolderInfo folderInfo,                
                IFilesystemOperator filesystemOperator)
            {
                throw new NotImplementedException();
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

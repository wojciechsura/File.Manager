using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Items;
using File.Manager.API.Filesystem.Models.Items.Listing;
using File.Manager.BusinessLogic.Models.Dialogs.DeleteConfiguration;
using File.Manager.BusinessLogic.Services.Dialogs;
using File.Manager.BusinessLogic.Services.Messaging;
using File.Manager.BusinessLogic.Types;
using File.Manager.Resources.Operations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.ViewModels.Operations.Delete
{
    public class DeleteOperationViewModel : BaseDeleteOperationViewModel
    {
        // Private types ------------------------------------------------------

        private sealed class DeleteWorkerContext : BaseDeleteWorkerContext
        {
            public DeleteWorkerContext(DeleteConfigurationModel configuration) 
                : base(configuration)
            {

            }
        }

        private sealed class DeleteWorker : BaseDeleteWorker<DeleteWorkerContext>
        {
            // Private fields -------------------------------------------------

            private DateTime startTime;

            // Protected methods ----------------------------------------------

            protected override (bool exit, DeleteWorkerResult result) DeleteFile(DeleteWorkerContext context, IFileInfo fileInfo, IFilesystemOperator filesystemOperator)
            {
                if (CancellationPending)
                    return (true, new CancelledDeleteWorkerResult());

                (bool exit, DeleteWorkerResult result) = base.DeleteFile(context, fileInfo, filesystemOperator);

                context.DeletedFiles++;
                context.DeletedSize += fileInfo.Size;

                (_, string elapsedString) = EvalElapsed(startTime);
                var partialDescription = string.Format(Strings.Delete_Info_PartialDescription, context.DeletedFiles, context.DeletedSize, elapsedString);

                ReportProgress(0, new DeleteProgress(0, partialDescription));

                return (exit, result);
            }

            protected override (bool exit, DeleteWorkerResult result) RetrieveFolderContents(DeleteWorkerContext context, 
                IFolderInfo folderInfo, 
                IFilesystemOperator filesystemOperator, 
                ref IReadOnlyList<IBaseItemInfo> items)
            {
                items = filesystemOperator.List(null, context.Configuration.FileMask);
                if (items == null)
                {
                    var resolution = GetResolutionFor(DeleteProblemKind.CannotListFolderContents,
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

            protected override void OnDoWork(DoWorkEventArgs e)
            {
                startTime = DateTime.Now;

                var input = (DeleteWorkerInput)e.Argument;

                var items = input.FilesystemOperator.List(input.SelectedItems, input.Configuration.FileMask);

                var context = new DeleteWorkerContext(input.Configuration);

                var result = ProcessItems(context, items, input.FilesystemOperator);

                if (result != null)
                    e.Result = result;
                else
                    e.Result = new SuccessDeleteWorkerResult();
            }

            // Public methods -------------------------------------------------

            public DeleteWorker()
            {

            }
        }

        // Private fields -----------------------------------------------------

        private readonly DeleteWorker worker;

        // Public methods -----------------------------------------------------

        public DeleteOperationViewModel(IDialogService dialogService,
            IMessagingService messagingService,
            IFilesystemOperator filesystemOperator,
            DeleteConfigurationModel configuration,
            IReadOnlyList<Item> selectedItems)
            : base(dialogService,
                  messagingService,
                  filesystemOperator,
                  selectedItems,
                  configuration)
        {
            this.worker = new DeleteWorker();
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
            Address = filesystemOperator.CurrentPath;

            ProgressIndeterminate = true;

            var input = new DeleteWorkerInput(filesystemOperator,
                configuration,
                selectedItems);

            worker.RunWorkerAsync(input);
        }
    }
}

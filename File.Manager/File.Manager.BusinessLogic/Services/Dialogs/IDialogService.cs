﻿using File.Manager.BusinessLogic.Models.Configuration.Ftp;
using File.Manager.BusinessLogic.Models.Dialogs.CopyMoveConfiguration;
using File.Manager.BusinessLogic.Models.Dialogs.DeleteConfiguration;
using File.Manager.BusinessLogic.Models.Dialogs.FtpCredentials;
using File.Manager.BusinessLogic.Models.Dialogs.NewFolderConfiguration;
using File.Manager.BusinessLogic.Models.Dialogs.Selection;
using File.Manager.BusinessLogic.Types;
using File.Manager.BusinessLogic.ViewModels.Operations;
using File.Manager.BusinessLogic.ViewModels.Operations.CopyMove;
using File.Manager.BusinessLogic.ViewModels.Operations.Delete;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Services.Dialogs
{
    public interface IDialogService
    {
        void ShowExceptionDialog(Exception e);
        (bool result, string path) ShowOpenDialog(string filter = null, string title = null, string filename = null);
        (bool result, string path) ShowSaveDialog(string filter = null, string title = null, string filename = null);
        (bool result, T resolution) ShowUserDecisionDialog<T>(T[] availableResolutions, string header);
        (bool result, CopyMoveConfigurationModel model) ShowCopyMoveConfigurationDialog(CopyMoveConfigurationInputModel input);
        void ShowCopyMoveProgress(BaseCopyMoveOperationViewModel operation);
        (bool result, DeleteConfigurationModel model) ShowDeleteConfigurationDialog(DeleteConfigurationInputModel input);
        void ShowDeleteProgress(BaseDeleteOperationViewModel operation);
        (bool result, NewFolderConfigurationModel model) ShowNewFolderConfigurationDialog();
        (bool result, SelectionResultModel model) ShowSelectionDialog(SelectionOperationKind operationKind);
        void ShowViewWindow(Stream stream, string filename);
        (bool result, FtpSession model) ShowFtpSessionEditorDialog(FtpSession editedSession);
        (bool result, FtpCredentialsModel model) ShowFtpCredentialsDialog(string username);
    }
}

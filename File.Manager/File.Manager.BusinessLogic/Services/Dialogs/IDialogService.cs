﻿using File.Manager.BusinessLogic.Models.Dialogs.CopyMoveConfiguration;
using File.Manager.BusinessLogic.Types;
using File.Manager.BusinessLogic.ViewModels.Operations;
using File.Manager.BusinessLogic.ViewModels.Operations.CopyMove;
using System;
using System.Collections.Generic;
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
        (bool result, CopyMoveConfigurationModel model) ShowCopyMoveConfigurationDialog(CopyMoveConfigurationInputModel input);
        void ShowCopyMoveProgress(BaseCopyMoveOperationViewModel operation);
        (bool result, SingleCopyMoveProblemResolution resolution) ShowUserDecisionDialog(SingleCopyMoveProblemResolution[] availableResolutions, string header);
    }
}

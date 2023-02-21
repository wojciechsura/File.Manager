using File.Manager.BusinessLogic.Models.Dialogs.CopyMoveConfiguration;
using File.Manager.BusinessLogic.Services.Dialogs;
using File.Manager.BusinessLogic.Types;
using File.Manager.BusinessLogic.ViewModels.Operations;
using File.Manager.Resources;
using File.Manager.Windows;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.Services.DialogService
{
    internal class DialogService : IDialogService
    {
        public (bool result, string path) ShowOpenDialog(string filter = null, string title = null, string filename = null)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (filename != null)
                dialog.FileName = filename;

            if (filter != null)
                dialog.Filter = filter;
            else
                dialog.Filter = Resources.Common.Strings.DefaultFilter;

            if (title != null)
                dialog.Title = title;
            else
                dialog.Title = Resources.Common.Strings.DefaultDialogTitle;

            if (dialog.ShowDialog() == true)
                return (true, dialog.FileName);
            else
                return (false, null);
        }

        public (bool result, string path) ShowSaveDialog(string filter = null, string title = null, string filename = null)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            if (filename != null)
                dialog.FileName = filename;

            if (filter != null)
                dialog.Filter = filter;
            else
                dialog.Filter = Resources.Common.Strings.DefaultFilter;

            if (title != null)
                dialog.Title = title;
            else
                dialog.Title = Resources.Common.Strings.DefaultDialogTitle;

            if (dialog.ShowDialog() == true)
                return (true, dialog.FileName);
            else
                return (false, null);
        }

        public void ShowExceptionDialog(Exception e)
        {
            var dialog = new ExceptionWindow(e);
            dialog.ShowDialog();
        }

        public (bool result, CopyMoveConfigurationModel model) ShowCopyMoveConfigurationDialog(CopyMoveConfigurationInputModel input)
        {
            var dialog = new CopyMoveConfigurationWindow(input);
            if (dialog.ShowDialog() == true)
                return (true, dialog.Result);
            else
                return (false, null);
        }

        public void ShowOperation(BaseOperationViewModel operation)
        {
            var dialog = new OperationRunnerWindow(operation);
            dialog.ShowDialog();
        }

        public (bool result, SingleProblemResolution resolution) ShowUserDecisionDialog(SingleProblemResolution[] availableResolutions, string header)
        {
            var dialog = new UserDecisionDialog(availableResolutions, header);
            if (dialog.ShowDialog() == true)
                return (true, dialog.Result);
            else
                return (false, (SingleProblemResolution)0);
        }
    }
}

using File.Manager.BusinessLogic.Models.Dialogs.CopyMoveConfiguration;
using File.Manager.BusinessLogic.Models.Dialogs.DeleteConfiguration;
using File.Manager.BusinessLogic.Services.Dialogs;
using File.Manager.BusinessLogic.Types;
using File.Manager.BusinessLogic.ViewModels.Operations;
using File.Manager.BusinessLogic.ViewModels.Operations.CopyMove;
using File.Manager.BusinessLogic.ViewModels.Operations.Delete;
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

        public (bool result, T resolution) ShowUserDecisionDialog<T>(T[] availableResolutions, string header)
        {
            var dialog = new UserDecisionDialog(availableResolutions.Cast<object>().ToArray(), header);
            if (dialog.ShowDialog() == true)
                return (true, (T)dialog.Result);
            else
                return (false, default(T));
        }

        public (bool result, CopyMoveConfigurationModel model) ShowCopyMoveConfigurationDialog(CopyMoveConfigurationInputModel input)
        {
            var dialog = new CopyMoveConfigurationWindow(input);
            if (dialog.ShowDialog() == true)
                return (true, dialog.Result);
            else
                return (false, null);
        }

        public void ShowCopyMoveProgress(BaseCopyMoveOperationViewModel operation)
        {
            var dialog = new CopyMoveProgressWindow(operation);
            dialog.ShowDialog();
        }

        public (bool result, DeleteConfigurationModel model) ShowDeleteConfigurationDialog(DeleteConfigurationInputModel input)
        {
            var dialog = new DeleteConfigurationWindow(input);
            if (dialog.ShowDialog() == true)
                return (true, dialog.Result);
            else
                return (false, null);
        }

        public void ShowDeleteProgress(BaseDeleteOperationViewModel operation)
        {
            var dialog = new DeleteProgressWindow(operation);
            dialog.ShowDialog();
        }
    }
}

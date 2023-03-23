using File.Manager.BusinessLogic.Models.Configuration.Ftp;
using File.Manager.BusinessLogic.Models.Dialogs.CopyMoveConfiguration;
using File.Manager.BusinessLogic.Models.Dialogs.DeleteConfiguration;
using File.Manager.BusinessLogic.Models.Dialogs.NewFolderConfiguration;
using File.Manager.BusinessLogic.Models.Dialogs.Selection;
using File.Manager.BusinessLogic.Services.Dialogs;
using File.Manager.BusinessLogic.Types;
using File.Manager.BusinessLogic.ViewModels.NewFolderConfiguration;
using File.Manager.BusinessLogic.ViewModels.Operations;
using File.Manager.BusinessLogic.ViewModels.Operations.CopyMove;
using File.Manager.BusinessLogic.ViewModels.Operations.Delete;
using File.Manager.Resources;
using File.Manager.Windows;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace File.Manager.Services.DialogService
{
    internal class DialogService : IDialogService
    {
        private readonly Stack<Window> dialogWindows = new();

        private void ActivateLastDialog()
        {
            if (dialogWindows.Any())
                dialogWindows.Peek().Activate();
        }

        private void PopDialog(Window dialog)
        {
            if (dialogWindows.Peek() != dialog)
                throw new InvalidOperationException("Broken dialog window stack mechanism!");

            dialogWindows.Pop();
        }

        private Window GetOwnerWindow()
        {
            return dialogWindows.Any() ? dialogWindows.Peek() : Application.Current.MainWindow;
        }

        public (bool result, string path) ShowOpenDialog(string filter = null, string title = null, string filename = null)
        {
            try
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
            finally
            {
                ActivateLastDialog();
            }
        }

        public (bool result, string path) ShowSaveDialog(string filter = null, string title = null, string filename = null)
        {
            try
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
            finally
            {
                ActivateLastDialog();
            }
        }

        public void ShowExceptionDialog(Exception e)
        {
            var dialog = new ExceptionWindow(e);
            dialog.Owner = GetOwnerWindow();
            dialogWindows.Push(dialog);

            try
            {
                dialog.ShowDialog();
            }
            finally
            {
                PopDialog(dialog);
                ActivateLastDialog();
            }
        }

        public (bool result, T resolution) ShowUserDecisionDialog<T>(T[] availableResolutions, string header)
        {
            var dialog = new UserDecisionDialog(availableResolutions.Cast<object>().ToArray(), header);
            dialog.Owner = GetOwnerWindow();
            dialogWindows.Push(dialog);

            try
            {
                if (dialog.ShowDialog() == true)
                    return (true, (T)dialog.Result);
                else
                    return (false, default(T));
            }
            finally
            {
                PopDialog(dialog);
                ActivateLastDialog();
            }
        }

        public (bool result, NewFolderConfigurationModel model) ShowNewFolderConfigurationDialog()
        {
            var dialog = new NewFolderConfigurationWindow();
            dialog.Owner = GetOwnerWindow();
            dialogWindows.Push(dialog);

            try
            {
                if (dialog.ShowDialog() == true)
                    return (true, dialog.Result);
                else
                    return (false, null);
            }
            finally
            {
                PopDialog(dialog);
                ActivateLastDialog();
            }
        }

        public (bool result, CopyMoveConfigurationModel model) ShowCopyMoveConfigurationDialog(CopyMoveConfigurationInputModel input)
        {
            var dialog = new CopyMoveConfigurationWindow(input);
            dialog.Owner = GetOwnerWindow();
            dialogWindows.Push(dialog);

            try
            {
                if (dialog.ShowDialog() == true)
                    return (true, dialog.Result);
                else
                    return (false, null);
            }
            finally
            {
                PopDialog(dialog);
                ActivateLastDialog();
            }
        }

        public void ShowCopyMoveProgress(BaseCopyMoveOperationViewModel operation)
        {
            var dialog = new CopyMoveProgressWindow(operation);
            dialog.Owner = GetOwnerWindow();
            dialogWindows.Push(dialog);

            try
            {
                dialog.ShowDialog();
            }
            finally
            {
                PopDialog(dialog);
                ActivateLastDialog();
            }
        }

        public (bool result, DeleteConfigurationModel model) ShowDeleteConfigurationDialog(DeleteConfigurationInputModel input)
        {
            var dialog = new DeleteConfigurationWindow(input);
            dialog.Owner = GetOwnerWindow();
            dialogWindows.Push(dialog);

            try
            {
                if (dialog.ShowDialog() == true)
                    return (true, dialog.Result);
                else
                    return (false, null);
            }
            finally
            {
                PopDialog(dialog);
                ActivateLastDialog();
            }
        }

        public void ShowDeleteProgress(BaseDeleteOperationViewModel operation)
        {
            var dialog = new DeleteProgressWindow(operation);
            dialog.Owner = GetOwnerWindow();
            dialogWindows.Push(dialog);

            try
            {
                dialog.ShowDialog();
            }
            finally
            {
                PopDialog(dialog);
                ActivateLastDialog();
            }
        }

        public (bool result, SelectionResultModel model) ShowSelectionDialog(SelectionOperationKind operationKind)
        {
            var dialog = new SelectionWindow(operationKind);
            dialog.Owner = GetOwnerWindow();
            dialogWindows.Push(dialog);

            try
            {
                if (dialog.ShowDialog() == true)
                    return (true, dialog.Result);
                else
                    return (false, null);
            }
            finally
            {
                PopDialog(dialog);
                ActivateLastDialog();
            }
        }

        public void ShowViewWindow(Stream stream, string filename)
        {
            // Note: view window is not modal and therefore is not
            // owner-managed, as other dialogs

            var dialog = new ViewWindow(stream, filename);
            dialog.Owner = Application.Current.MainWindow;
            dialog.Show();
        }

        public (bool result, FtpSession model) ShowFtpSessionEditorDialog(FtpSession editedSession)
        {
            var dialog = new FtpSessionEditorWindow(editedSession);
            dialog.Owner = GetOwnerWindow();
            dialogWindows.Push(dialog);

            try
            {
                if (dialog.ShowDialog() == true)
                    return (true, dialog.Result);
                else
                    return (false, null);
            }
            finally
            {
                PopDialog(dialog);
                ActivateLastDialog();
            }
        }
    }
}

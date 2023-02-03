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
    }
}

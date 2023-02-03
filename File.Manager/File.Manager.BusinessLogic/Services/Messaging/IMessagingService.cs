using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Services.Messaging
{
    public interface IMessagingService
    {
        bool AskYesNo(string message, string title = null);
        bool? AskYesNoCancel(string message, string title = null);
        void Warn(string message, string title = null);
        bool WarnYesNo(string message, string title = null);
        void ShowError(string message, string title = null);
        void Inform(string message, string title = null);
        void Beep();
    }
}

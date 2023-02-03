using File.Manager.BusinessLogic.Services.Messaging;
using File.Manager.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace File.Manager.Services.Messaging
{
    internal class MessagingService : IMessagingService
    {
        public bool AskYesNo(string message, string title = null)
        {
            if (title == null)
                title = Strings.DefaultMessageboxTitle;

            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
                return true;
            else if (result == MessageBoxResult.No)
                return false;
            else
                throw new InvalidOperationException("Invalid result!");
        }

        public bool? AskYesNoCancel(string message, string title = null)
        {
            if (title == null)
                title = Strings.DefaultMessageboxTitle;

            var result = MessageBox.Show(message, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
                return true;
            else if (result == MessageBoxResult.No)
                return false;
            else if (result == MessageBoxResult.Cancel)
                return null;
            else
                throw new InvalidOperationException("Invalid result!");
        }

        public bool WarnYesNo(string message, string title = null)
        {
            if (title == null)
                title = Strings.DefaultMessageboxTitle;

            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
                return true;
            else if (result == MessageBoxResult.No)
                return false;
            else
                throw new InvalidOperationException("Invalid result!");
        }

        public void Warn(string message, string title = null)
        {
            if (title == null)
                title = Strings.DefaultMessageboxTitle;

            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public void ShowError(string message, string title = null)
        {
            if (title == null)
                title = Strings.DefaultMessageboxTitle;

            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void Inform(string message, string title = null)
        {
            if (title == null)
                title = Strings.DefaultMessageboxTitle;

            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void Beep()
        {
            SystemSounds.Beep.Play();
        }
    }
}

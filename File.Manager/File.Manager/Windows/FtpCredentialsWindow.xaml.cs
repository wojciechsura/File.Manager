using Autofac;
using File.Manager.BusinessLogic.Models.Dialogs.FtpCredentials;
using File.Manager.BusinessLogic.ViewModels.FtpCredentialsWindow;
using File.Manager.BusinessLogic.ViewModels.FtpSessionEditorWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace File.Manager.Windows
{
    /// <summary>
    /// Logika interakcji dla klasy FtpCredentialsWindow.xaml
    /// </summary>
    public partial class FtpCredentialsWindow : Window, IFtpCredentialsWindowAccess
    {
        private readonly FtpCredentialsWindowViewModel viewModel;

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            viewModel.SetPassword(((PasswordBox)sender).SecurePassword);
        }

        public FtpCredentialsWindow(string username)
        {
            InitializeComponent();

            viewModel = Dependencies.Container.Instance.Resolve<FtpCredentialsWindowViewModel>(
                new NamedParameter("access", this), 
                new NamedParameter("username", username));

            DataContext = viewModel;
        }

        void IFtpCredentialsWindowAccess.Close(bool result)
        {
            DialogResult = result;
            Close();
        }

        public FtpCredentialsModel Result => viewModel.Result;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (string.IsNullOrEmpty(viewModel.Username))
                {
                    tbUsername.Focus();
                }
                else
                {
                    tbPassword.Focus();
                }
            }, System.Windows.Threading.DispatcherPriority.DataBind);
        }
    }
}

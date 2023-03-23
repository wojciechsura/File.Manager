using Autofac;
using File.Manager.BusinessLogic.Models.Configuration.Ftp;
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
    /// Logika interakcji dla klasy FtpSessionEditorWindow.xaml
    /// </summary>
    public partial class FtpSessionEditorWindow : Window, IFtpSessionEditorWindowAccess
    {
        private readonly FtpSessionEditorWindowViewModel viewModel;

        void IFtpSessionEditorWindowAccess.Close(bool result)
        {
            DialogResult = result;
            Close();
        }

        public FtpSessionEditorWindow(FtpSession session)
        {
            InitializeComponent();

            viewModel = Dependencies.Container.Instance.Resolve<FtpSessionEditorWindowViewModel>(new NamedParameter("model", session),
                new NamedParameter("access", this));
            DataContext = viewModel;
        }

        public FtpSession Result => viewModel.Result;
    }
}

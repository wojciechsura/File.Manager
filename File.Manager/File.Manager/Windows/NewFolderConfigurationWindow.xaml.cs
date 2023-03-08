using Autofac;
using File.Manager.BusinessLogic.Models.Dialogs.NewFolderConfiguration;
using File.Manager.BusinessLogic.ViewModels.NewFolderConfiguration;
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
    /// Logika interakcji dla klasy NewFolderConfigurationWindow.xaml
    /// </summary>
    public partial class NewFolderConfigurationWindow : Window, INewFolderConfigurationWindowAccess
    {
        private NewFolderConfigurationWindowViewModel viewModel;

        public NewFolderConfigurationWindow()
        {
            InitializeComponent();

            viewModel = Dependencies.Container.Instance.Resolve<NewFolderConfigurationWindowViewModel>(new NamedParameter("access", this));
            DataContext = viewModel;
        }

        public void Close(bool reason)
        {
            DialogResult = reason;
            Close();
        }

        public NewFolderConfigurationModel Result => viewModel.Result;
    }
}

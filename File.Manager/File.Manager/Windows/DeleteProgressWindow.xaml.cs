using Autofac;
using File.Manager.BusinessLogic.ViewModels.Operations;
using File.Manager.BusinessLogic.ViewModels.Operations.Delete;
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
    /// Logika interakcji dla klasy OperationRunnerWindows.xaml
    /// </summary>
    public partial class DeleteProgressWindow : Window, IDeleteProgressWindowAccess
    {
        private readonly DeleteProgressWindowViewModel viewModel;

        void IDeleteProgressWindowAccess.Close()
        {
            Close();
        }

        public DeleteProgressWindow(BaseDeleteOperationViewModel operation)
        {
            InitializeComponent();

            viewModel = Dependencies.Container.Instance.Resolve<DeleteProgressWindowViewModel>(new NamedParameter("operation", operation),
                new NamedParameter("access", this));
            DataContext = viewModel;
        }

        private void HandleWindowLoaded(object sender, RoutedEventArgs e)
        {
            viewModel.NotifyLoaded();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!viewModel.CanClose)
            {
                viewModel.NotifyUserRequestedClose();
                e.Cancel = true;
            }
        }
    }
}

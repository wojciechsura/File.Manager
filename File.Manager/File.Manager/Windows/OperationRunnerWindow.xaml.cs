using Autofac;
using File.Manager.BusinessLogic.ViewModels.Operations;
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
    public partial class OperationRunnerWindow : Window, IOperationRunnerWindowAccess
    {
        private readonly OperationRunnerViewModel viewModel;

        void IOperationRunnerWindowAccess.Close()
        {
            Close();
        }

        public OperationRunnerWindow(BaseOperationViewModel operation)
        {
            InitializeComponent();

            viewModel = Dependencies.Container.Instance.Resolve<OperationRunnerViewModel>(new NamedParameter("operation", operation),
                new NamedParameter("access", this));
            DataContext = viewModel;
        }

        private void HandleWindowLoaded(object sender, RoutedEventArgs e)
        {
            viewModel.NotifyLoaded();
        }
    }
}

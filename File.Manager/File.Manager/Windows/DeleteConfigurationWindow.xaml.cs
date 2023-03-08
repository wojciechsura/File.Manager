using Autofac;
using Autofac.Core;
using File.Manager.BusinessLogic.Models.Dialogs.DeleteConfiguration;
using File.Manager.BusinessLogic.ViewModels.DeleteConfiguration;
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
    /// Interaction logic for DeleteConfigurationWindow.xaml
    /// </summary>
    public partial class DeleteConfigurationWindow : Window, IDeleteConfigurationWindowAccess
    {
        private readonly DeleteConfigurationWindowViewModel viewModel;

        public DeleteConfigurationWindow(DeleteConfigurationInputModel input)
        {
            InitializeComponent();

            var viewModel = Dependencies.Container.Instance.Resolve<DeleteConfigurationWindowViewModel>(new NamedParameter("input", input),
                new NamedParameter("access", this));

            this.viewModel = viewModel;
            DataContext = viewModel;
        }

        public void Close(bool result)
        {
            DialogResult = result;
            Close();
        }

        public DeleteConfigurationModel Result => viewModel.Result;
    }
}

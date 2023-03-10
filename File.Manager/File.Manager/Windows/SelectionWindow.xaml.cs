using Autofac;
using File.Manager.BusinessLogic.Models.Dialogs.Selection;
using File.Manager.BusinessLogic.Types;
using File.Manager.BusinessLogic.ViewModels.Selection;
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
    /// Logika interakcji dla klasy SelectionWindow.xaml
    /// </summary>
    public partial class SelectionWindow : Window, ISelectionWindowAccess
    {
        private readonly SelectionWindowViewModel viewModel;

        public SelectionWindow(SelectionOperationKind operationKind)
        {
            InitializeComponent();

            viewModel = Dependencies.Container.Instance.Resolve<SelectionWindowViewModel>(new NamedParameter("operationKind", operationKind),
                new NamedParameter("access", this));
            DataContext = viewModel;
        }

        public void Close(bool result)
        {
            DialogResult = result;
            Close();
        }

        public SelectionResultModel Result => viewModel.Result;
    }
}

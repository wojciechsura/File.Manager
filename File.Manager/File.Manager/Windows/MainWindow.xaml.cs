using Autofac;
using Autofac.Core;
using File.Manager.BusinessLogic.ViewModels.Main;
using Fluent;
using Spooksoft.VisualStateManager.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace File.Manager.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow, IMainWindowAccess
    {
        private MainWindowViewModel viewModel;

        private void DoSwitchPanes()
        {
            var leftCol = (int)pLeft.GetValue(Grid.ColumnProperty);
            var rightCol = (int)pRight.GetValue(Grid.ColumnProperty);

            pLeft.SetValue(Grid.ColumnProperty, leftCol == 0 ? 2 : 0);
            pRight.SetValue(Grid.ColumnProperty, rightCol == 0 ? 2 : 0);
        }

        public MainWindow()
        {
            SwitchPanesCommand = new AppCommand(obj => DoSwitchPanes());

            InitializeComponent();

            viewModel = Dependencies.Container.Instance.Resolve<MainWindowViewModel>(new NamedParameter("access", this));
            DataContext = viewModel;

            pLeft.Focus();
        }

        public ICommand SwitchPanesCommand { get; }
    }
}

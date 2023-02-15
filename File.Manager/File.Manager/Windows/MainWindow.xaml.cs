using Autofac;
using Autofac.Core;
using File.Manager.API.Filesystem.Models.Items;
using File.Manager.BusinessLogic.Models.Files;
using File.Manager.BusinessLogic.Types;
using File.Manager.BusinessLogic.ViewModels.Main;
using File.Manager.BusinessLogic.ViewModels.Pane;
using File.Manager.Controls;
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
            throw new NotImplementedException();
        }

        public MainWindow()
        {
            SwitchPanesCommand = new AppCommand(obj => DoSwitchPanes());

            InitializeComponent();

            viewModel = Dependencies.Container.Instance.Resolve<MainWindowViewModel>(new NamedParameter("access", this));
            DataContext = viewModel;
        }

        public ICommand SwitchPanesCommand { get; }

        private void paneGotFocus(object sender, RoutedEventArgs e)
        {
            viewModel.NotifyActivated((sender as Pane)?.DataContext as PaneViewModel);
        }
    }
}

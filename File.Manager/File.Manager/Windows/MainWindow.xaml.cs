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

        void IMainWindowAccess.FocusActivePane()
        {
            if ((pLeft.DataContext as PaneViewModel)?.Active ?? false)
                pLeft.Focus();
            else if ((pRight.DataContext as PaneViewModel)?.Active ?? false)
                pRight.Focus();
        }

        public MainWindow()
        {
            InitializeComponent();

            viewModel = Dependencies.Container.Instance.Resolve<MainWindowViewModel>(new NamedParameter("access", this));
            DataContext = viewModel;
        }

        private void HandlePaneGotFocus(object sender, RoutedEventArgs e)
        {
            viewModel.NotifyActivated((sender as Pane)?.DataContext as PaneViewModel);
        }

        private void HandleBorderPreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void HandleWindowLoaded(object sender, RoutedEventArgs e)
        {
            Uri iconUri = new Uri("pack://application:,,,/Resources/Images/icon.ico", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);

            Dispatcher.BeginInvoke(() =>
            {
                pLeft.Focus();
            }, DispatcherPriority.ApplicationIdle);            
        }

        private void HandleWindowClosing(object sender, CancelEventArgs e)
        {
            bool cancel = e.Cancel;
            viewModel.NotifyClosing(ref cancel);
            e.Cancel= cancel;
        }
    }
}

using File.Manager.BusinessLogic.ViewModels.Pane;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace File.Manager.Controls
{
    /// <summary>
    /// Logika interakcji dla klasy Pane.xaml
    /// </summary>
    public partial class Pane : UserControl, IPaneAccess
    {
        private PaneViewModel paneViewModel;

        public Pane()
        {
            InitializeComponent();
            DataContextChanged += HandleDataContextChanged;
        }

        public void FocusItem(ItemViewModel selectedItem)
        {
            Dispatcher.BeginInvoke(() =>
            {
                var listBoxItem = (ListBoxItem)lbItems.ItemContainerGenerator.ContainerFromItem(selectedItem);
                listBoxItem?.Focus();
                lbItems.UpdateLayout();
                lbItems.ScrollIntoView(listBoxItem);
            }, DispatcherPriority.Render);
        }

        private void HandleDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (paneViewModel != null)
                paneViewModel.Access = null;

            paneViewModel = e.NewValue as PaneViewModel;

            if (paneViewModel != null)
                paneViewModel.Access = this;
        }

        private void ItemDisplayPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                (DataContext as PaneViewModel)?.ExecuteCurrentItem();
            }
        }
    }
}

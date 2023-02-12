using File.Manager.API.Filesystem.Models.Items;
using File.Manager.BusinessLogic.ViewModels.Pane;
using File.Manager.Tools;
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
        // Private fields -----------------------------------------------------

        private readonly CollectionViewSource collectionViewSource;
        private PaneViewModel viewModel;

        // Private methods ----------------------------------------------------

        private void FocusSelectedItem(ListView listView)
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (listView.SelectedItem != null)
                {
                    var listViewItem = (ListViewItem)listView.ItemContainerGenerator.ContainerFromItem(listView.SelectedItem);

                    if (listViewItem != null && !listViewItem.IsFocused)
                    {
                        listViewItem.Focus();
                        listView.UpdateLayout();
                    }
                }
            }, DispatcherPriority.Render);
        }

        private void HandleDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (viewModel != null)
                viewModel.Access = null;

            viewModel = e.NewValue as PaneViewModel;

            if (viewModel != null)
                viewModel.Access = this;
        }

        private void HandleListViewGotFocus(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is not ListViewItem)
                FocusSelectedItem(sender as ListView);
        }

        private void HandleListViewItemSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = sender as ListView;

            FocusSelectedItem(listView);
            listView.ScrollIntoView(listView.SelectedItem);
        }

        private void HandleListViewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListViewItem listViewItem = VisualTreeTools.VisualUpwardSearch<ListViewItem>(e.OriginalSource as DependencyObject);

            if (listViewItem != null)
            {
                viewModel.ExecuteCurrentItem();
            }
        }

        private void HandlePaneGotFocus(object sender, RoutedEventArgs e)
        {
            viewModel?.NotifyGotFocus();
        }

        private void HandlePaneLostFocus(object sender, RoutedEventArgs e)
        {
            viewModel.NotifyLostFocus();
        }

        private void HandlePanePreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                viewModel.NotifySpacePressed();
                e.Handled = true;
            }
            else if (e.Key == Key.Insert)
            {
                viewModel.NotifyInsertPressed();
                e.Handled = true;
            }
            else if (e.Key == Key.Tab)
            {
                viewModel.NotifyTabPressed();
                e.Handled = true;
            }
        }

        private void ItemDisplayPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                (DataContext as PaneViewModel)?.ExecuteCurrentItem();
            }
        }

        // IPaneAccess implementation -----------------------------------------

        void IPaneAccess.SetNextItem()
        {
            collectionViewSource.View.MoveCurrentToNext();
        }

        // Public methods -----------------------------------------------------

        public Pane()
        {
            InitializeComponent();

            collectionViewSource = (CollectionViewSource)FindResource("cvsItems");

            DataContextChanged += HandleDataContextChanged;

            SetDefaultColumns(lbItems);
        }

        public void FocusList()
        {
            lbItems.Focus();
        }

        public void SetDefaultColumns(ListView listView)
        {
            var gridView = listView.View as GridView;
            if (gridView == null)
                return;

            gridView.Columns.Clear();

            // Add name column
            var nameColumn = new GridViewColumn();
            nameColumn.Header = File.Manager.Resources.Controls.Pane.Strings.Column_Header_Name;
            nameColumn.Width = 300;
            nameColumn.CellTemplate = (DataTemplate)FindResource("NameColumnTemplate");
            gridView.Columns.Add(nameColumn);

            // Add size column
            var sizeColumn = new GridViewColumn();
            sizeColumn.Header = File.Manager.Resources.Controls.Pane.Strings.Column_Header_Size;
            sizeColumn.Width = 100;
            sizeColumn.DisplayMemberBinding = new Binding() { Path = new PropertyPath(nameof(Item.SizeDisplay)) };
            gridView.Columns.Add(sizeColumn);

            // Add modified column
            var modifiedColumn = new GridViewColumn();
            modifiedColumn.Header = File.Manager.Resources.Controls.Pane.Strings.Column_Header_Modified;
            modifiedColumn.Width = 150;
            modifiedColumn.DisplayMemberBinding = new Binding() { Path = new PropertyPath(nameof(Item.Modified)) };
            gridView.Columns.Add(modifiedColumn);

            // Add attributes column
            var attributesColumn = new GridViewColumn();
            attributesColumn.Header = File.Manager.Resources.Controls.Pane.Strings.Column_Header_Attributes;
            attributesColumn.Width = 100;
            attributesColumn.DisplayMemberBinding = new Binding() { Path = new PropertyPath(nameof(Item.Attributes)) };
            gridView.Columns.Add(attributesColumn);
        }
    }
}

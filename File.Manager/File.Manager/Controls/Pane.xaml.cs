using File.Manager.API.Filesystem.Models.Items;
using File.Manager.BusinessLogic.Models.Files;
using File.Manager.BusinessLogic.Types;
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

        private PaneViewModel viewModel;

        // Private methods ----------------------------------------------------

        private void HandleDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (viewModel != null)
                viewModel.Access = null;

            viewModel = e.NewValue as PaneViewModel;

            if (viewModel != null)
                viewModel.Access = this;
        }

        private void HandlePanePreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                viewModel.NotifyTabPressed();
                e.Handled = true;
            }
        }

        // IPaneAccess implementation -----------------------------------------

        // Public methods -----------------------------------------------------

        public Pane()
        {
            InitializeComponent();
            DataContextChanged += HandleDataContextChanged;
            SetDefaultColumns();
        }

        public void SetDefaultColumns()
        {
            var columns = new FileListColumnCollection
            {
                new FileListFilenameColumn(File.Manager.Resources.Modules.Filesystem.Common.Strings.Header_Filename) { Width = 1, WidthKind = FileListColumnWidthKind.Star },
                new FileListKeyColumn(File.Manager.Resources.Modules.Filesystem.Common.Strings.Header_Size, Item.SizeDisplayKey) { Width = 80 },
                new FileListKeyColumn(File.Manager.Resources.Modules.Filesystem.Common.Strings.Header_Modified, Item.ModifiedKey) { Width = 120 },
                new FileListKeyColumn(File.Manager.Resources.Modules.Filesystem.Common.Strings.Header_Attributes, Item.AttributesKey) { Width = 80 }
            };

            flList.Columns = columns;
        }

        private void HandlePaneGotFocus(object sender, RoutedEventArgs e)
        {
            if (flList.IsFocused)
                flList.Focus();
        }
    }
}

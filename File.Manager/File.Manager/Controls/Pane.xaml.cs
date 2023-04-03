using File.Manager.API.Filesystem.Models.Items.Listing;
using File.Manager.BusinessLogic.Models.Files;
using File.Manager.BusinessLogic.Types;
using File.Manager.BusinessLogic.ViewModels.Pane;
using File.Manager.Common.Helpers;
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
            if (e.Key == Key.Escape)
            {
                viewModel.HideQuickSearch();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                viewModel.HideQuickSearch();
                // Let the event pass to the file list
            }
        }

        private void HandlePaneTextInput(object sender, TextCompositionEventArgs e)
        {
            if (viewModel.QuickSearchVisible)
            {
                viewModel.QuickSearchText = (viewModel.QuickSearchText + e.Text).ApplyControlChars();
            }
        }

        // Protected methods --------------------------------------------------

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                viewModel.NotifyTabPressed();
                e.Handled = true;
            }
            else if (Keyboard.IsKeyDown(Key.LeftAlt) && e.IsDown && (e.Key >= Key.A && e.Key <= Key.Z))
            {

            }
            else
            {
                base.OnPreviewKeyDown(e);
            }
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            if (!flList.IsFocused)
                flList.Focus();
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
    }
}

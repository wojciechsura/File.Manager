using File.Manager.BusinessLogic.Models.Files;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace File.Manager.Controls.Files
{
    internal abstract class FileListRenderer
    {
        // Private fields -----------------------------------------------------

        private FileListColumnCollection columns;

        // Private methods ----------------------------------------------------

        private void SetColumns(FileListColumnCollection value)
        {
            if (columns != null)
            {
                columns.CollectionChanged -= HandleColumnsCollectionChanged;
                columns.ColumnWidthsChanged -= HandleColumnsWidthsChanged;
            }

            columns = value;

            if (columns != null)
            {
                columns.CollectionChanged += HandleColumnsCollectionChanged;
                columns.ColumnWidthsChanged += HandleColumnsWidthsChanged;
            }

            OnColumnsChanged();
        }

        // Protected fields ---------------------------------------------------

        protected readonly IFileListRendererHost host;

        // Protected methods --------------------------------------------------

        protected FileListRenderer(IFileListRendererHost host)
        {
            this.host = host;
        }

        protected abstract void HandleColumnsWidthsChanged(object sender, EventArgs e);

        protected abstract void HandleColumnsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e);

        protected virtual void OnColumnsChanged()
        {

        }

        // Public methods -----------------------------------------------------

        public abstract void Render(DrawingContext drawingContext);

        public virtual void NotifyMetricsChanged()
        {
            host.RequestInvalidateVisual();
        }

        // Public properties --------------------------------------------------

        public IReadOnlyList<FileListColumn> Columns
        {
            get => columns;
            set => SetColumns(columns);
        }
    }
}

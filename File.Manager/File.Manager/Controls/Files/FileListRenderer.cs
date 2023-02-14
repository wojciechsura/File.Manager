using File.Manager.BusinessLogic.Models.Files;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
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
        private ICollectionView filesSource;

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

        private void SetFilesSource(ICollectionView value)
        {
            if (filesSource != null)
            {
                filesSource.CollectionChanged -= HandleFilesSourceCollectionChanged;
                filesSource.CurrentChanged -= HandleFilesSourceCurrentChanged;
            }

            filesSource = value;

            if (filesSource != null)
            {
                filesSource.CollectionChanged += HandleFilesSourceCollectionChanged;
                filesSource.CurrentChanged += HandleFilesSourceCurrentChanged;
            }

            OnFilesSourceChanged();
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

        protected abstract void HandleFilesSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e);

        protected abstract void HandleFilesSourceCurrentChanged(object sender, EventArgs e);

        protected virtual void OnColumnsChanged()
        {

        }

        protected virtual void OnFilesSourceChanged()
        {

        }

        // Public methods -----------------------------------------------------

        public FileListRenderer()
        {
            
        }

        public abstract void Render(DrawingContext drawingContext);

        public abstract void UpdateScrollData();

        public virtual void NotifyMetricsChanged()
        {
            host.RequestInvalidateVisual();
        }

        public virtual void NotifyScrollPositionChanged()
        {
            
        }

        // Public properties --------------------------------------------------

        public FileListColumnCollection Columns
        {
            get => columns;
            set => SetColumns(value);
        }
        
        public ICollectionView FilesSource 
        {
            get => filesSource;
            set => SetFilesSource(value); 
        }
    }
}

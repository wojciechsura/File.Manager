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

        protected virtual void OnBoundsChanged(Rect newBounds)
        {

        }

        protected virtual void OnDpiChanged(double newPixelsPerDip)
        {

        }

        protected virtual void OnFontChanged(string fontFamily, double fontSize)
        {
            
        }

        // Public methods -----------------------------------------------------

        public abstract void Render(DrawingContext drawingContext);

        public void NotifyColumnsChanged() 
        {
            OnColumnsChanged();
        }

        public void NotifyBoundsChanged(Rect newBounds)
        {
            OnBoundsChanged(newBounds);
            host.RequestInvalidateVisual();
        }

        public void NotifyDpiChanged(double newPixelsPerDip)
        {
            OnDpiChanged(newPixelsPerDip);
            host.RequestInvalidateVisual();
        }

        public void NotifyFontChanged(string fontFamily, double fontSize)
        {
            OnFontChanged(fontFamily, fontSize);
            host.RequestInvalidateVisual();
        }
    }
}

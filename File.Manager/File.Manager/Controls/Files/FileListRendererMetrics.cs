using File.Manager.BusinessLogic.Models.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace File.Manager.Controls.Files
{
    internal abstract class FileListRendererMetrics
    {
        // Private fields -----------------------------------------------------

        private IReadOnlyList<FileListColumn> columns;

        // Private methods ----------------------------------------------------

        private void SetColumns(IReadOnlyList<FileListColumn> newColumns)
        {
            if (columns != newColumns)
            {
                columns = newColumns;
                Invalidate();
            }
        }

        // Protected fields ---------------------------------------------------

        protected readonly IFileListRendererHost host;

        // Protected methods --------------------------------------------------

        protected double PxToDip(double pixels) => pixels / host.PixelsPerDip;

        protected double DipToPx(double dip) => dip * host.PixelsPerDip;

        // Public methods -----------------------------------------------------

        public FileListRendererMetrics(IFileListRendererHost host)
        {
            this.host = host;
        }

        public abstract void Invalidate();

        public abstract void Validate();

        // Public properties --------------------------------------------------

        public IReadOnlyList<FileListColumn> Columns
        {
            get => columns;
            set => SetColumns(value);
        }

        public abstract bool Valid { get; }
    }
}

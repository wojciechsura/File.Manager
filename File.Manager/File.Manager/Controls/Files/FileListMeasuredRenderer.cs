using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace File.Manager.Controls.Files
{
    internal abstract class FileListMeasuredRenderer<TMetrics> : FileListRenderer
        where TMetrics : FileListRendererMetrics
    {
        // Protected fields ---------------------------------------------------

        protected TMetrics metrics;

        // Protected methods --------------------------------------------------

        protected FileListMeasuredRenderer(IFileListRendererHost host, TMetrics metrics)
            : base(host)
        {
            this.metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        }

        protected void ValidateMetrics()
        {
            if (!metrics.Valid)
            {
                metrics.Validate();
            }
        }

        protected void InvalidateMetrics()
        {
            metrics.Invalidate();
        }

        protected override void OnColumnsChanged()
        {
            metrics.Columns = Columns;
            host.RequestInvalidateVisual();
        }

        // Public methods -----------------------------------------------------

        public override void NotifyMetricsChanged()
        {
            InvalidateMetrics();
            base.NotifyMetricsChanged();
        }

        public override void Render(DrawingContext drawingContext)
        {
            ValidateMetrics();
        }
    }
}

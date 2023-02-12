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
        where TMetrics : FileListRendererMetrics, new()
    {
        // Protected fields ---------------------------------------------------

        protected readonly TMetrics metrics;

        // Protected methods --------------------------------------------------

        protected FileListMeasuredRenderer(IFileListRendererHost host)
            : base(host)
        {
            this.metrics = new TMetrics();
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

        protected override void OnBoundsChanged(Rect newBounds)
        {
            metrics.Bounds = newBounds;
        }

        protected override void OnDpiChanged(double newPixelsPerDip)
        {
            metrics.PixelsPerDip = newPixelsPerDip;
        }

        protected override void OnColumnsChanged()
        {
            metrics.Columns = host.Columns;
            host.RequestInvalidateVisual();
        }

        protected override void OnFontChanged(string fontFamily, double fontSize)
        {
            metrics.FontFamily = fontFamily;
            metrics.FontSize = fontSize;
        }


        // Public methods -----------------------------------------------------

        public override void Render(DrawingContext drawingContext)
        {
            ValidateMetrics();
        }
    }
}

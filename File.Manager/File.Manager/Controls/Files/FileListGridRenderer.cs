using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace File.Manager.Controls.Files
{
    internal class FileListGridRenderer : FileListMeasuredRenderer<FileListGridRendererMetrics>
    {
        protected override void HandleColumnsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            InvalidateMetrics();
            host.RequestInvalidateVisual();
        }

        protected override void HandleColumnsWidthsChanged(object sender, EventArgs e)
        {
            InvalidateMetrics();
            host.RequestInvalidateVisual();
        }

        public override void Render(DrawingContext drawingContext)
        {
            base.Render(drawingContext);

            var typeface = new Typeface(host.FontFamily.Source);

            drawingContext.DrawRectangle(host.Appearance.HeaderBackgroundBrush, null, metrics.Header.HeaderBounds.ToBrushRect());

            if (Columns == null || Columns.Count == 0)
                return;

            var columnHeaderSeparatorPen = new Pen(host.Appearance.HeaderSeparatorBrush, host.PixelsPerDip * 1.0);

            for (int i = 0; i < Columns.Count; i++)
            {
                drawingContext.PushClip(new RectangleGeometry(metrics.Column.Columns[i].HeaderBounds.ToBrushRect()));
                try
                {
                    if (i > 0)
                    {
                        Rect headerRect = metrics.Column.Columns[i].HeaderBounds.ToPenRect(columnHeaderSeparatorPen.Thickness);

                        drawingContext.DrawLine(columnHeaderSeparatorPen,
                            new Point(headerRect.Left, headerRect.Top),
                            new Point(headerRect.Left, headerRect.Bottom));
                    }

                    var text = new FormattedText(Columns[i].Header,
                        CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight,
                        typeface,
                        host.FontSize,
                        host.Appearance.HeaderForegroundBrush,
                        host.PixelsPerDip);

                    drawingContext.DrawText(text, metrics.Column.Columns[i].HeaderTitle.ToPoint());
                }
                finally
                {
                    drawingContext.Pop();
                }
            }
        }

        public FileListGridRenderer(IFileListRendererHost host)
            : base(host, new FileListGridRendererMetrics(host))
        {

        }
    }
}

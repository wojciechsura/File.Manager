using File.Manager.BusinessLogic.Models.Files;
using File.Manager.Types;
using Fluent;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace File.Manager.Controls.Files
{
    internal class FileListGridRenderer : FileListMeasuredRenderer<FileListGridRendererMetrics>
    {
        // Private methods ----------------------------------------------------

        private void DrawHeader(DrawingContext drawingContext, Typeface typeface)
        {
            drawingContext.DrawRectangle(host.Appearance.HeaderBackgroundBrush, null, metrics.Header.HeaderBounds.ToBrushRect());

            if (Columns == null || Columns.Count == 0)
                return;

            var columnHeaderSeparatorPen = new Pen(host.Appearance.HeaderSeparatorBrush, host.PixelsPerDip * 1.0);

            for (int i = 0; i < Columns.Count; i++)
            {
                drawingContext.PushClip(new RectangleGeometry(metrics.Column.Columns[i].TitleBounds.ToBrushRect()));
                try
                {
                    if (i > 0)
                    {
                        Rect headerRect = metrics.Column.Columns[i].TitleBounds.ToPenRect(columnHeaderSeparatorPen.Thickness);

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

                    drawingContext.DrawText(text, metrics.Column.Columns[i].TitlePosition.ToPoint());
                }
                finally
                {
                    drawingContext.Pop();
                }
            }
        }

        private void DrawKeyCell(DrawingContext drawingContext, Typeface typeface, IFileListItem fileItem, PixelRectangle itemRect, string key)
        {
            var valueToDisplay = fileItem[key];

            string sValue = valueToDisplay switch
            {
                string str => str,
                int i => i.ToString(),
                DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
                object other => other.ToString(),
                null => string.Empty
            };

            PixelPoint valuePosition = new PixelPoint(itemRect.Left + metrics.Column.HorizontalMarginPx,
                itemRect.Top + (itemRect.Height - metrics.Character.CharHeight) / 2);

            var valueText = new FormattedText(sValue, CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                typeface,
                host.FontSize,
                host.Appearance.ItemForegroundBrush,
                host.PixelsPerDip);

            drawingContext.DrawText(valueText, valuePosition.ToPoint());
        }

        private void DrawCell(DrawingContext drawingContext, Typeface typeface, IFileListItem fileItem, int col, PixelRectangle itemRect)
        {
            drawingContext.PushClip(new RectangleGeometry(itemRect.ToRegionRect()));

            try
            {
                switch (Columns[col])
                {
                    case FileListFilenameColumn filenameColumn:
                        {
                            DrawFilenameCell(drawingContext, typeface, fileItem, itemRect);

                            break;
                        }
                    case FileListKeyColumn keyColumn:
                        {
                            DrawKeyCell(drawingContext, typeface, fileItem, itemRect, keyColumn.Key);

                            break;
                        }
                    default:
                        throw new InvalidOperationException("Unsupported column type!");
                }
            }
            finally
            {
                drawingContext.Pop();
            }
        }

        private void DrawFilenameCell(DrawingContext drawingContext, Typeface typeface, IFileListItem fileItem, PixelRectangle itemRect)
        {
            PixelRectangle iconRect = new PixelRectangle(itemRect.Left + metrics.Column.HorizontalMarginPx,
                                        itemRect.Top + (itemRect.Height - metrics.Item.IconSize) / 2,
                                        metrics.Item.IconSize,
                                        metrics.Item.IconSize);

            drawingContext.DrawImage(fileItem.SmallIcon, iconRect.ToBrushRect());

            PixelPoint filenamePosition = new PixelPoint(iconRect.Right + metrics.Item.ItemSpacing,
                itemRect.Top + (itemRect.Height - metrics.Character.CharHeight) / 2);

            var filenameText = new FormattedText(fileItem.Name,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                typeface,
                host.FontSize,
                host.Appearance.ItemForegroundBrush,
                host.PixelsPerDip);

            drawingContext.DrawText(filenameText, filenamePosition.ToPoint());
        }

        private void DrawItem(DrawingContext drawingContext, Typeface typeface, int itemIndex, IFileListItem fileItem)
        {
            int itemY = metrics.Item.ItemArea.Top + itemIndex * metrics.Item.ItemHeight - host.ScrollPosition;

            if (FilesSource.CurrentPosition == itemIndex)
            {
                Brush border, background;

                if (host.IsFocused)
                {
                    border = host.Appearance.FocusActiveFocusedBorderBrush;
                    background = host.Appearance.FocusActiveFocusedBackgroundBrush;
                }
                else
                {
                    if (host.IsActive)
                    {
                        border = host.Appearance.FocusActiveUnfocusedBorderBrush;
                        background = host.Appearance.FocusActiveUnfocusedBackgroundBrush;
                    }
                    else
                    {
                        border = host.Appearance.FocusInactiveBorderBrush;
                        background = host.Appearance.FocusInactiveBackgroundBrush;
                    }
                }

                // Focus bar

                PixelRectangle focusRect = new PixelRectangle(metrics.Item.ItemArea.Left + metrics.Item.SelectionHorizontalMargin,
                    itemY + metrics.Item.SelectionVerticalMargin,
                    metrics.Item.ItemArea.Width - 2 * metrics.Item.SelectionHorizontalMargin,
                    metrics.Item.ItemHeight - 2 * metrics.Item.SelectionVerticalMargin);

                drawingContext.DrawRoundedRectangle(background,
                    new Pen(border, metrics.Item.SelectionLineThickness),
                    focusRect.ToPenRect(metrics.Item.SelectionLineThickness),
                    metrics.Item.SelectionCornerRadius,
                    metrics.Item.SelectionCornerRadius);
            }

            for (int col = 0; col < Columns.Count; col++)
            {
                var itemRect = new PixelRectangle(metrics.Column.Columns[col].TitleBounds.Left,
                    itemY,
                    metrics.Column.Columns[col].TitleBounds.Width,
                    metrics.Item.ItemHeight);

                DrawCell(drawingContext, typeface, fileItem, col, itemRect);
            }
        }

        private void DrawItems(DrawingContext drawingContext, Typeface typeface)
        {
            drawingContext.PushClip(new RectangleGeometry(metrics.Item.ItemArea.ToRegionRect()));
            try
            {
                int itemIndex = host.ScrollPosition / metrics.Item.ItemHeight;

                var itemsToShow = FilesSource.Cast<IFileListItem>().Skip(itemIndex).Take(metrics.Item.ItemsInView);
                foreach (var fileItem in itemsToShow)
                {
                    DrawItem(drawingContext, typeface, itemIndex, fileItem);

                    itemIndex++;
                }
            }
            finally
            {
                drawingContext.Pop();
            }
        }

        private void EnsureFocusedItemVisible()
        {
            int selectedItemTopY = FilesSource.CurrentPosition * metrics.Item.ItemHeight;
            int selectedItemBottomY = selectedItemTopY + metrics.Item.ItemHeight - 1;
            int topY = host.ScrollPosition;
            int bottomY = host.ScrollPosition + metrics.Item.ItemArea.Height - 1;

            int topDifference = selectedItemTopY - topY;
            int bottomDifference = selectedItemBottomY - bottomY;

            if (topDifference < 0)
            {
                // Selected item is above current view
                host.ScrollPosition = Math.Max(0, Math.Min(host.ScrollMaximum, selectedItemTopY));
                host.RequestInvalidateVisual();
            }
            else if (bottomDifference > 0)
            {
                // Selected item is below current view
                host.ScrollPosition = Math.Max(0, Math.Min(host.ScrollMaximum, selectedItemBottomY - metrics.Item.ItemArea.Height + 1));
                host.RequestInvalidateVisual();
            }

            // Else item is already in current view
        }

        // Protected methods --------------------------------------------------

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

        protected override void HandleFilesSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            InvalidateMetrics();
            UpdateScrollData();
            host.RequestInvalidateVisual();
        }

        protected override void HandleFilesSourceCurrentChanged(object sender, EventArgs e)
        {
            EnsureFocusedItemVisible();
            host.RequestInvalidateVisual();
        }

        // Public methods -----------------------------------------------------

        public FileListGridRenderer(IFileListRendererHost host)
            : base(host, new FileListGridRendererMetrics(host))
        {
            
        }

        public override void NotifyMetricsChanged()
        {
            InvalidateMetrics();
            UpdateScrollData();
            host.RequestInvalidateVisual();
        }

        protected override void OnFilesSourceChanged()
        {
            base.OnFilesSourceChanged();
            InvalidateMetrics();
            UpdateScrollData();
            host.RequestInvalidateVisual();
        }

        public override void NotifyScrollPositionChanged()
        {
            host.RequestInvalidateVisual();
        }

        protected override void OnColumnsChanged()
        {
            base.OnColumnsChanged();
            InvalidateMetrics();
            host.RequestInvalidateVisual();
        }

        public override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Up)
            {
                if (FilesSource != null)
                {
                    if (FilesSource.CurrentItem == null)
                    {
                        FilesSource.MoveCurrentToLast();
                        EnsureFocusedItemVisible();
                        host.RequestInvalidateVisual();
                    }
                    else if (FilesSource.CurrentPosition > 0)
                    {
                        FilesSource.MoveCurrentToPrevious();
                        EnsureFocusedItemVisible();
                        host.RequestInvalidateVisual();
                    }
                }

                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                if (FilesSource != null)
                {
                    if (FilesSource.CurrentItem == null)
                    {
                        FilesSource.MoveCurrentToFirst();
                        EnsureFocusedItemVisible();
                        host.RequestInvalidateVisual();
                    }
                    else if (FilesSource.CurrentPosition < FilesSource.Cast<object>().Count() - 1)
                    {
                        FilesSource.MoveCurrentToNext();
                        EnsureFocusedItemVisible();
                        host.RequestInvalidateVisual();
                    }
                }

                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                if (FilesSource != null && FilesSource.CurrentItem != null) 
                {
                    host.RequestExecuteCurrentItem();
                }
            }
        }

        public override void UpdateScrollData()
        {
            ValidateMetrics();

            host.ScrollMaximum = metrics.Item.ScrollMaximum;
            host.ScrollSmallChange = metrics.Item.ScrollSmallChange;
            host.ScrollLargeChange = metrics.Item.ScrollLargeChange;
        }

        public override void Render(DrawingContext drawingContext)
        {
            base.Render(drawingContext);

            var typeface = new Typeface(host.FontFamily, host.FontStyle, host.FontWeight, host.FontStretch);

            DrawHeader(drawingContext, typeface);

            // Items

            if (FilesSource != null)
                DrawItems(drawingContext, typeface);
        }        
    }
}

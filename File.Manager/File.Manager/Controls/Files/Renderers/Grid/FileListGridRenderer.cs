using File.Manager.API.Filesystem.Models.Items.Listing;
using File.Manager.BusinessLogic.Models.Files;
using File.Manager.Types;
using Fluent;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace File.Manager.Controls.Files.Renderers.Grid
{
    internal partial class FileListGridRenderer : FileListStateRenderer<FileListGridRenderer>
    {
        // Private types ------------------------------------------------------

        private abstract class MouseHoverInfo
        {

        }

        private class ColumnHoverInfo : MouseHoverInfo
        {
            public ColumnHoverInfo(int columnIndex)
            {
                ColumnIndex = columnIndex;
            }

            public int ColumnIndex { get; }
        }

        // Private fields -----------------------------------------------------

        private readonly FileListGridRendererMetrics metrics;
        private MouseHoverInfo mouseHover;

        // Private methods ----------------------------------------------------

        private void ValidateMetrics()
        {
            if (!metrics.Valid)
            {
                metrics.Validate();
            }
        }

        private void InvalidateMetrics()
        {
            metrics.Invalidate();
        }

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
                    if (mouseHover is ColumnHoverInfo columnHover && columnHover.ColumnIndex == i)
                    {
                        drawingContext.DrawRectangle(host.Appearance.HeaderFocusedBackgroundBrush,
                            null,
                            metrics.Column.Columns[i].TitleBounds.ToBrushRect());
                    }

                    if (i > 0)
                    {
                        Rect headerRect = metrics.Column.Columns[i].TitleBounds.ToPenRect(columnHeaderSeparatorPen.Thickness);

                        drawingContext.DrawLine(columnHeaderSeparatorPen,
                            new Point(headerRect.Left, headerRect.Top),
                            new Point(headerRect.Left, headerRect.Bottom));
                    }

                    DrawText(drawingContext,
                        host.Appearance.HeaderForegroundBrush,
                        Columns[i].Header,
                        metrics.Column.Columns[i].TitlePosition.X,
                        metrics.Column.Columns[i].TitlePosition.Y);
                }
                finally
                {
                    drawingContext.Pop();
                }
            }
        }

        private void DrawKeyCell(DrawingContext drawingContext, Typeface typeface, IFileListItem fileItem, PixelRectangle itemRect, Brush foregroundBrush, string key)
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

            PixelPoint valuePosition = new(itemRect.Left + metrics.Column.HorizontalMarginPx,
                itemRect.Top + (itemRect.Height - metrics.Character.CharHeight) / 2);
            DrawText(drawingContext,
                foregroundBrush,
                sValue,
                valuePosition.X,
                valuePosition.Y);
        }

        private void DrawCell(DrawingContext drawingContext, Typeface typeface, IFileListItem fileItem, int col, PixelRectangle itemRect)
        {
            drawingContext.PushClip(new RectangleGeometry(itemRect.ToRegionRect()));

            Brush brush;
            if (fileItem.IsHidden)
                brush = fileItem.IsSelected ? host.Appearance.HiddenSelectedItemForegroundBrush : host.Appearance.HiddenItemForegroundBrush;
            else
                brush = fileItem.IsSelected ? host.Appearance.SelectedItemForegroundBrush : host.Appearance.ItemForegroundBrush;

            try
            {
                switch (Columns[col])
                {
                    case FileListFilenameColumn filenameColumn:
                        {
                            DrawFilenameCell(drawingContext, typeface, fileItem, itemRect, brush);

                            break;
                        }
                    case FileListKeyColumn keyColumn:
                        {
                            DrawKeyCell(drawingContext, typeface, fileItem, itemRect, brush, keyColumn.Key);

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

        private void DrawFilenameCell(DrawingContext drawingContext, Typeface typeface, IFileListItem fileItem, PixelRectangle itemRect, Brush foregroundBrush)
        {
            PixelRectangle iconRect = new PixelRectangle(itemRect.Left + metrics.Column.HorizontalMarginPx,
                                        itemRect.Top + (itemRect.Height - metrics.Item.IconSize) / 2,
                                        metrics.Item.IconSize,
                                        metrics.Item.IconSize);

            drawingContext.DrawImage(fileItem.SmallIcon, iconRect.ToBrushRect());

            PixelPoint filenamePosition = new PixelPoint(iconRect.Right + metrics.Item.ItemSpacing,
                itemRect.Top + (itemRect.Height - metrics.Character.CharHeight) / 2);

            DrawText(drawingContext,
                foregroundBrush,
                fileItem.Name,
                filenamePosition.X,
                filenamePosition.Y);
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
            int firstItemInViewIndex = host.ScrollPosition / metrics.Item.ItemHeight;

            var itemIndex = firstItemInViewIndex;
            var itemsToShow = FilesSource.Cast<IFileListItem>().Skip(firstItemInViewIndex).Take(metrics.Item.ItemsInView);
            foreach (var fileItem in itemsToShow)
            {
                DrawItem(drawingContext, typeface, itemIndex, fileItem);

                itemIndex++;
            }
        }

        private void DrawSelectionBoxes(DrawingContext drawingContext)
        {
            void DrawSelectionBox(int firstUnselectedItemIndex, int firstItemIndex)
            {
                int firstItemY = firstItemIndex * metrics.Item.ItemHeight - host.ScrollPosition;
                int currentSelectedItemsCount = firstUnselectedItemIndex - (int)firstItemIndex;

                PixelRectangle selectionRect = new PixelRectangle(metrics.Item.ItemArea.Left + metrics.Item.SelectionHorizontalMargin,
                    metrics.Item.ItemArea.Top + firstItemY + metrics.Item.SelectionVerticalMargin,
                    metrics.Item.ItemArea.Width - 2 * metrics.Item.SelectionHorizontalMargin,
                    currentSelectedItemsCount * metrics.Item.ItemHeight - 2 * metrics.Item.SelectionVerticalMargin);

                drawingContext.DrawRoundedRectangle(host.Appearance.SelectionBackgroundBrush,
                    new Pen(host.Appearance.SelectionBorderBrush, metrics.Item.SelectionLineThickness),
                    selectionRect.ToPenRect(metrics.Item.SelectionLineThickness),
                    metrics.Item.SelectionCornerRadius,
                    metrics.Item.SelectionCornerRadius);
            }

            int firstItemInViewIndex = host.ScrollPosition / metrics.Item.ItemHeight;
            int firstItemCheckedIndex = Math.Max(0, firstItemInViewIndex - 1);
            int checkedItemCount = metrics.Item.ItemsInView + 2;

            int itemIndex = firstItemCheckedIndex;
            
            int? currentSelectionBoxFirstItemIndex = null;

            var itemsToCheck = FilesSource
                .Cast<IFileListItem>()
                .Skip(firstItemCheckedIndex)
                .Take(checkedItemCount);

            foreach (var fileItem in itemsToCheck)
            {
                if (fileItem.IsSelected && currentSelectionBoxFirstItemIndex == null)
                {
                    currentSelectionBoxFirstItemIndex = itemIndex;
                }
                else if ((!fileItem.IsSelected || itemIndex == firstItemCheckedIndex + checkedItemCount - 1) && currentSelectionBoxFirstItemIndex != null)
                {
                    DrawSelectionBox(itemIndex, currentSelectionBoxFirstItemIndex.Value);
                    currentSelectionBoxFirstItemIndex = null;
                }

                itemIndex++;
            }

            // This happens if last item in series is in view and is selected
            if (currentSelectionBoxFirstItemIndex != null)
                DrawSelectionBox(itemIndex, currentSelectionBoxFirstItemIndex.Value);
        }

        private void DrawItemArea(DrawingContext drawingContext, Typeface typeface)
        {
            drawingContext.PushClip(new RectangleGeometry(metrics.Item.ItemArea.ToRegionRect()));
            try
            {
                // Draw selection boxes first

                DrawSelectionBoxes(drawingContext);

                // Draw items

                DrawItems(drawingContext, typeface);
            }
            finally
            {
                drawingContext.Pop();
            }
        }

        private void EnsureFocusedItemVisible()
        {
            ValidateMetrics();

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
            base.HandleFilesSourceCollectionChanged(sender, e);

            InvalidateMetrics();
            UpdateScrollData();
            host.RequestInvalidateVisual();
        }

        protected override void HandleFilesSourceCurrentChanged(object sender, EventArgs e)
        {
            EnsureFocusedItemVisible();
            host.RequestInvalidateVisual();
        }

        protected override void HandleItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.HandleItemPropertyChanged(sender, e);

            InvalidateMetrics();
            host.RequestInvalidateVisual();
        }

        // Public methods -----------------------------------------------------

        public FileListGridRenderer(IFileListRendererHost host)
            : base(host, renderer => new IdleState(renderer))
        {
            metrics = new FileListGridRendererMetrics(host);
        }

        public override void NotifyMetricsChanged()
        {
            InvalidateMetrics();
            UpdateScrollData();
            host.RequestInvalidateVisual();
        }

        protected override void OnFilesSourceChanged()
        {
            metrics.FilesSource = FilesSource;

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
            metrics.Columns = Columns;
            InvalidateMetrics();
            host.RequestInvalidateVisual();
        }

        public override void UpdateScrollData()
        {
            ValidateMetrics();

            host.ScrollMaximum = metrics.Item.ScrollMaximum;
            host.ScrollSmallChange = metrics.Item.ScrollSmallChange;
            host.ScrollLargeChange = metrics.Item.ScrollLargeChange;
            host.ScrollViewportSize = metrics.Item.ScrollViewportSize;
        }

        public override void Render(DrawingContext drawingContext)
        {
            ValidateMetrics();

            var typeface = new Typeface(host.FontFamily, host.FontStyle, host.FontWeight, host.FontStretch);

            DrawHeader(drawingContext, typeface);

            // Items

            if (FilesSource != null)
                DrawItemArea(drawingContext, typeface);
        }        
    }
}

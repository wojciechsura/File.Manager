using File.Manager.BusinessLogic.Types;
using File.Manager.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace File.Manager.Controls.Files
{
    internal class FileListGridRendererMetrics : FileListRendererMetrics
    {
        // Private constants --------------------------------------------------

        private const int MIN_COLUMN_WIDTH_DIP = 10;
        private const double COLUMN_HEADER_HEIGHT_EM = 2;
        private const double COLUMN_HORIZONTAL_MARGIN_EM = 0.5;
        private const double ROW_VERTICAL_MARGIN_EM = 0.1;
        private const double ROW_ITEM_SPACING_EM = 0.2;
        private const int ICON_SIZE = 16;

        // Public types -------------------------------------------------------

        public class ColumnMetric
        {
            public ColumnMetric(PixelRectangle headerBounds,
                PixelPoint headerTitle)
            {
                HeaderTitle = headerTitle;
                HeaderBounds = headerBounds;
            }

            public PixelPoint HeaderTitle { get; }
            public PixelRectangle HeaderBounds { get; }
        }

        public class ColumnMetrics
        {
            public ColumnMetrics(IReadOnlyList<ColumnMetric> columns, 
                int horizontalMarginPx)
            {
                Columns = columns;
                HorizontalMarginPx = horizontalMarginPx;
            }

            public IReadOnlyList<ColumnMetric> Columns { get; }
            public int HorizontalMarginPx { get; }
        }

        public class CharacterMetrics
        {
            public CharacterMetrics(int charWidth, int charHeight)
            {
                CharWidth = charWidth;
                CharHeight = charHeight;
            }

            public int CharWidth { get; }
            public int CharHeight { get; }
        }

        public class HeaderMetrics
        {
            public HeaderMetrics(PixelRectangle headerBounds)
            {
                HeaderBounds = headerBounds;
            }

            public PixelRectangle HeaderBounds { get; }
        }

        public class ItemMetrics
        {
            public ItemMetrics(PixelRectangle itemArea,
                int itemCount,
                int itemTotalHeight,
                int scrollMaximum,
                int scrollSmallChange,
                int scrollLargeChange,
                int itemsInView,
                int itemHeight, 
                int verticalMargin, 
                int itemSpacing)
            {
                ItemArea = itemArea;
                ItemCount = itemCount;
                ItemTotalHeight = itemTotalHeight;
                ScrollMaximum = scrollMaximum;
                ScrollSmallChange = scrollSmallChange;
                ScrollLargeChange = scrollLargeChange;
                ItemsInView = itemsInView;
                ItemHeight = itemHeight;
                VerticalMargin = verticalMargin;
                ItemSpacing = itemSpacing;
            }

            public PixelRectangle ItemArea { get; }
            public int ItemCount { get; }
            public int ItemTotalHeight { get; }
            public int ScrollMaximum { get; }
            public int ScrollSmallChange { get; }
            public int ScrollLargeChange { get; }
            public int ItemsInView { get; }
            public int ItemHeight { get; }
            public int VerticalMargin { get; }
            public int ItemSpacing { get; }
        }

        // Private fields -----------------------------------------------------

        private CharacterMetrics characterMetrics;
        private HeaderMetrics headerMetrics;
        private ColumnMetrics columnMetrics;
        private ItemMetrics itemMetrics;

        // Private methods ----------------------------------------------------

        private (int remainingWidthPx, int starSum) EvalColumnWidthStats()
        {
            int availableWidthPx = host.Bounds.Width;

            int requiredAbsoluteWidthPx = (int)DipToPx(Columns
                .Where(c => c.WidthKind == FileListColumnWidthKind.Dip)
                .Sum(c => c.Width));

            int remainingWidthPx = availableWidthPx - requiredAbsoluteWidthPx;

            int starSum = Columns
                .Where(c => c.WidthKind == FileListColumnWidthKind.Star)
                .Sum(c => c.Width);

            return (remainingWidthPx, starSum);
        }

        private List<ColumnMetric> BuildColumnMetrics(int remainingWidthPx, 
            int starSum, 
            HeaderMetrics headerMetrics)
        {
            List<ColumnMetric> columns = new();

            int runningX = headerMetrics.HeaderBounds.Left;

            if (Columns != null && Columns.Count > 0)
            {
                for (int i = 0; i < Columns.Count; i++)
                {
                    int columnWidth;

                    switch (Columns[i].WidthKind)
                    {

                        case FileListColumnWidthKind.Dip:
                            {
                                columnWidth = (int)DipToPx(Math.Max(MIN_COLUMN_WIDTH_DIP, Columns[i].Width));
                                break;
                            }
                        case FileListColumnWidthKind.Star:
                            {
                                columnWidth = (int)DipToPx(Math.Max(MIN_COLUMN_WIDTH_DIP, remainingWidthPx * Columns[i].Width / starSum));
                                break;
                            }
                        default:
                            throw new InvalidOperationException("Unsupported FileListColumnWidthKind!");
                    }

                    PixelRectangle headerBounds = new PixelRectangle(runningX,
                        headerMetrics.HeaderBounds.Top,
                        columnWidth,
                        headerMetrics.HeaderBounds.Height);

                    PixelPoint headerTitlePosition = new PixelPoint((int)(headerBounds.Left + characterMetrics.CharWidth * COLUMN_HORIZONTAL_MARGIN_EM),
                        (headerBounds.Top + (headerBounds.Height - characterMetrics.CharHeight) / 2));

                    var columnMetric = new ColumnMetric(headerBounds, headerTitlePosition);
                    columns.Add(columnMetric);

                    runningX += columnWidth;
                }
            }

            return columns;
        }

        private void InvalidateColumnMetrics()
        {
            columnMetrics = null;
        }

        private void InvalidateCharacterMetrics()
        {
            characterMetrics = null;
            headerMetrics = null;
            columnMetrics = null;
            itemMetrics = null;
        }

        private void InvalidateHeaderMetrics()
        {
            headerMetrics = null;
            columnMetrics = null;
            itemMetrics = null;
        }

        private void InvalidateItemMetrics()
        {
            itemMetrics = null;
        }

        private void ValidateHeaderMetrics()
        {
            if (headerMetrics != null)
                return;

            ValidateCharacterMetrics();

            int headerTop = host.Bounds.Top;
            int headerLeft = host.Bounds.Left;
            int headerWidth = host.Bounds.Width;
            int headerHeight = (int)(characterMetrics.CharHeight * COLUMN_HEADER_HEIGHT_EM);

            var headerArea = new PixelRectangle(headerLeft, headerTop, headerWidth, headerHeight);

            headerMetrics = new HeaderMetrics(headerArea);
        }

        private void ValidateColumnMetrics()
        {
            if (columnMetrics != null)
                return;

            ValidateCharacterMetrics();
            ValidateHeaderMetrics();

            int horizontalColumnMarginPx = (int)DipToPx(characterMetrics.CharWidth * COLUMN_HORIZONTAL_MARGIN_EM);

            if (Columns == null)
            {
                columnMetrics = new ColumnMetrics(new List<ColumnMetric>(), horizontalColumnMarginPx);
            }
            else
            {
                // Evaluate required values

                (int remainingWidthPx, int starSum) = EvalColumnWidthStats();

                // Evaluate column metrics

                List<ColumnMetric> columns = BuildColumnMetrics(remainingWidthPx, starSum, headerMetrics);

                columnMetrics = new ColumnMetrics(columns, horizontalColumnMarginPx);
            }
        }
       
        private void ValidateCharacterMetrics()
        {
            if (characterMetrics != null)
                return;

            FormattedText text = new FormattedText("W",
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface(host.FontFamily.Source),
                host.FontSize,
                Brushes.Black,
                host.PixelsPerDip);

            characterMetrics = new CharacterMetrics((int)Math.Ceiling(text.Width), (int)Math.Ceiling(text.Height));            
        }

        private void ValidateItemMetrics()
        {
            if (itemMetrics != null)
                return;

            ValidateCharacterMetrics();
            ValidateHeaderMetrics();

            int verticalMargin = (int)DipToPx(characterMetrics.CharHeight * ROW_VERTICAL_MARGIN_EM);
            int itemSpacing = (int)DipToPx(characterMetrics.CharWidth * ROW_ITEM_SPACING_EM);

            int iconSize = (int)DipToPx(ICON_SIZE);
            int itemHeight = (int)(2 * verticalMargin + Math.Max(iconSize, characterMetrics.CharHeight));

            PixelRectangle itemArea = new PixelRectangle(headerMetrics.HeaderBounds.Bottom + 1,
                host.Bounds.Left,
                host.Bounds.Bottom - headerMetrics.HeaderBounds.Bottom,
                host.Bounds.Width);

            int itemCount = FilesSource?.Cast<object>().Count() ?? 0;
            int itemTotalHeight = itemCount * itemHeight;
            int scrollMaximum = itemTotalHeight - itemArea.Height - 1;
            int scrollSmallChange = itemTotalHeight;
            int scrollLargeChange = itemArea.Height;

            int itemsInView = (int)Math.Ceiling((double)host.Bounds.Height / itemHeight);

            itemMetrics = new ItemMetrics(itemArea,
                itemCount,
                itemTotalHeight,
                scrollMaximum,
                scrollSmallChange,
                scrollLargeChange,
                itemHeight,
                itemsInView,
                verticalMargin,
                itemSpacing);
        }

        // Public methods -----------------------------------------------------

        public FileListGridRendererMetrics(IFileListRendererHost host)
            : base(host)
        {

        }

        public override void Invalidate()
        {
            InvalidateColumnMetrics();
            InvalidateCharacterMetrics();
            InvalidateHeaderMetrics();
            InvalidateItemMetrics();
        }

        public override void Validate()
        {
            if (characterMetrics == null)
                ValidateCharacterMetrics();

            if (headerMetrics == null) 
                ValidateHeaderMetrics();

            if (columnMetrics == null)
                ValidateColumnMetrics();

            if (itemMetrics == null)
                ValidateItemMetrics();
        }

        // Public properties --------------------------------------------------

        public ColumnMetrics Column => columnMetrics;

        public HeaderMetrics Header => headerMetrics;

        public CharacterMetrics Character => characterMetrics;

        public ItemMetrics Item => itemMetrics;

        public override bool Valid => headerMetrics != null && columnMetrics != null && characterMetrics != null && itemMetrics != null;
    }
}

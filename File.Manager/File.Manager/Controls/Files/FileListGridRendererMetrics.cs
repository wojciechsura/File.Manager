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
        private const double COLUMN_HEIGHT_EM = 2;
        private const double COLUMN_HEADER_MARGIN_EM = 0.5;

        // Public types -------------------------------------------------------

        public class ColumnMetric
        {
            public ColumnMetric(PixelRectangle headerBounds, PixelPoint headerTitle)
            {
                HeaderTitle = headerTitle;
                HeaderBounds = headerBounds;
            }

            public PixelPoint HeaderTitle { get; }
            public PixelRectangle HeaderBounds { get; }
        }

        public class ColumnMetrics
        {
            public ColumnMetrics(IReadOnlyList<ColumnMetric> columns)
            {
                Columns = columns;
            }

            public IReadOnlyList<ColumnMetric> Columns { get; }
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

        // Private fields -----------------------------------------------------

        private ColumnMetrics columnMetrics;
        private CharacterMetrics characterMetrics;

        // Private methods ----------------------------------------------------

        private void InvalidateColumnMetrics()
        {
            columnMetrics = null;
        }

        private void InvalidateCharacterMetrics()
        {
            characterMetrics = null;
        }

        private void ValidateColumnMetrics()
        {
            ValidateCharacterMetrics();

            if (columnMetrics != null)
                return;

            if (Columns == null)
            {
                columnMetrics = new ColumnMetrics(new List<ColumnMetric>());
            }
            else
            {
                // Evaluate required values

                int availableWidthPx = host.Bounds.Width;

                int requiredAbsoluteWidthPx = (int)DipToPx(Columns
                    .Where(c => c.WidthKind == FileListColumnWidthKind.Dip)
                    .Sum(c => c.Width));

                int remainingWidthPx = availableWidthPx - requiredAbsoluteWidthPx;

                int starSum = Columns
                    .Where(c => c.WidthKind == FileListColumnWidthKind.Star)
                    .Sum(c => c.Width);

                // Evaluate column metrics

                List<ColumnMetric> columns = new();
                
                int runningX = host.Bounds.Left;
                int headerY = host.Bounds.Top;
                int columnHeight = (int)(characterMetrics.CharHeight * COLUMN_HEIGHT_EM);

                for (int i = 0; i < Columns.Count; i++)
                {
                    switch (Columns[i].WidthKind)
                    {
                        case FileListColumnWidthKind.Dip:
                            {
                                int columnWidth = (int)DipToPx(Math.Max(MIN_COLUMN_WIDTH_DIP, Columns[i].Width));

                                Rect headerBounds = new Rect(runningX,
                                    headerY,
                                    columnWidth,
                                    columnHeight);

                                // TODO

                                runningX += columnWidth;

                                break;
                            }
                        case FileListColumnWidthKind.Star:
                            {
                                break;
                            }
                        default:
                            throw new InvalidOperationException("Unsupported FileListColumnWidthKind!");
                    }
                }
            }
        }

        private void ValidateCharacterMetrics()
        {
            if (characterMetrics != null)
                return;

            FormattedText text = new FormattedText("W",
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface(host.FontFamily),
                host.FontSize,
                Brushes.Black,
                host.PixelsPerDip);

            characterMetrics = new CharacterMetrics((int)Math.Ceiling(text.Width), (int)Math.Ceiling(text.Height));            
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
        }

        public override void Validate()
        {
            if (columnMetrics == null)
                ValidateColumnMetrics();

            if (characterMetrics == null)
                ValidateCharacterMetrics();
        }

        // Public properties --------------------------------------------------

        public override bool Valid => columnMetrics != null && characterMetrics != null;
    }
}

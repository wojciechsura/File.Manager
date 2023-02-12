using File.Manager.BusinessLogic.Types;
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

        private const int MIN_COLUMN_WIDTH = 10;

        // Public types -------------------------------------------------------

        public class ColumnMetric
        {
            public ColumnMetric(Point headerTitle, Rect headerBounds)
            {
                HeaderTitle = headerTitle;
                HeaderBounds = headerBounds;
            }

            public Point HeaderTitle { get; }
            public Rect HeaderBounds { get; }
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
            if (columnMetrics != null)
                return;

            if (Columns == null)
            {
                columnMetrics = new ColumnMetrics(new List<ColumnMetric>());
            }
            else
            {
                // Evaluate required values

                double availableWidthPx = Bounds.Width;

                double requiredAbsoluteWidthPx = DipToPixels(Columns
                    .Where(c => c.WidthKind == BusinessLogic.Types.FileListColumnWidthKind.Dip)
                    .Sum(c => c.Width));

                double remainingWidthPx = availableWidthPx - requiredAbsoluteWidthPx;

                int starSum = Columns
                    .Where(c => c.WidthKind == BusinessLogic.Types.FileListColumnWidthKind.Star)
                    .Sum(c => c.Width);

                // Evaluate column metrics

                List<ColumnMetric> columns = new();
                
                double runningX = Bounds.Left;
                double headerY = Bounds.Top;
                double columnHeight = DipToPixels(10);

                for (int i = 0; i < Columns.Count; i++)
                {
                    switch (Columns[i].WidthKind)
                    {
                        case FileListColumnWidthKind.Dip:
                            {
                                double columnWidth = DipToPixels(Columns[i].Width);

                                Rect headerBounds = new Rect(runningX,
                                    headerY,
                                    columnWidth,
                                    columnHeight);

                                

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
                System.Windows.FlowDirection.LeftToRight,
                new Typeface(FontFamily),
                FontSize,
                System.Windows.Media.Brushes.Black,
                PixelsPerDip);

            characterMetrics = new CharacterMetrics((int)Math.Ceiling(text.Width), (int)Math.Ceiling(text.Height));            
        }
        // Public methods -----------------------------------------------------

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

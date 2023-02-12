using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace File.Manager.Controls.Files
{
    public partial class FileList
    {
        private sealed class Metrics
        {
            // Private constants ----------------------------------------------

            private const int SPACE_BETWEEN_PANES = 4;

            private const int PANE_BORDER_THICKNESS = 1;

            // Evaluated

            private const int MIN_WIDTH = 2 * PANE_BORDER_THICKNESS + 1 +
                    SPACE_BETWEEN_PANES +
                    2 * PANE_BORDER_THICKNESS + 1;
            private const int MIN_HEIGHT = 2 * PANE_BORDER_THICKNESS + 1 +
				SPACE_BETWEEN_PANES +
				2 * PANE_BORDER_THICKNESS + 1;

			// Public types ---------------------------------------------------

            public class GeneralMetrics
            {
                public GeneralMetrics(Rect controlArea)
                {
                    ControlArea = controlArea;
                }

                public Rect ControlArea { get; }
            }

			public class PaneMetrics
            {
                public PaneMetrics (Rect leftPaneBounds, 
                    Rect rightPaneBounds,
                    Rect leftPaneArea,
                    Rect rightPaneArea)
                {
                    LeftPaneBounds = leftPaneBounds;
                    RightPaneBounds = rightPaneBounds;
                    LeftPaneArea = leftPaneArea;
                    RightPaneArea = rightPaneArea;
                }   

                public Rect LeftPaneBounds { get; }
                public Rect RightPaneBounds { get; }
                public Rect LeftPaneArea { get; }
				public Rect RightPaneArea { get; }
			}

            // Private fields -------------------------------------------------

            private GeneralMetrics generalMetrics;
			private PaneMetrics paneMetrics;

            private double width;
            private double height;
            private Thickness padding;
            private double pixelsPerDip;

            // Private methods ------------------------------------------------

            private void SetWidth(double value)
            {
                if (width != value)
                {
                    width = value;
                    Invalidate();
                }
            }

            private void SetHeight(double value)
            {
                if (height != value)
                {
                    height = value;
                    Invalidate();
                }
            }

			private void SetPadding(Thickness value)
			{
				if (padding != value)
                {
                    padding = value;
                    Invalidate();
                }
			}

            private void SetPixelsPerDip(double value)
            {
                if (pixelsPerDip != value)
                {
                    pixelsPerDip = value;
                    Invalidate();
                }
            }

            private void InvalidateGeneralMetrics()
            {
                generalMetrics = null;
            }

			private void InvalidatePaneMetrics()
            {
                paneMetrics = null;
            }

            private void ValidateGeneralMetrics()
            {
                if (generalMetrics != null)
                    return;

                Rect controlArea = new Rect(0, 0, Width, Height);

                generalMetrics = new GeneralMetrics(controlArea);
            }

			private void ValidatePaneMetrics()
			{
                if (paneMetrics != null)
                    return;

                Rect leftRect;
                Rect rightRect;

                if (Width - Padding.Left - Padding.Right < MIN_WIDTH  || 
                    Height - Padding.Top - Padding.Bottom < MIN_HEIGHT)
                {
                    leftRect = new Rect(0, 0, 0, 0);
                    rightRect = new Rect(0, 0, 0, 0);
                }
                else
                {
                    int PaneWidth = (int)((Width - Padding.Left - Padding.Right - SPACE_BETWEEN_PANES) / 2);
                    int PaneHeight = (int)(Height - Padding.Top - Padding.Bottom);

                    leftRect = new Rect((int)Padding.Left,
                        (int)Padding.Top,
                        PaneWidth,
                        PaneHeight);

                    rightRect = new Rect((int)Width - 1 - (int)Padding.Right - PaneWidth,
                        (int)Padding.Top,
                        PaneWidth,
                        PaneHeight);
                }

				var leftPaneArea = new Rect(leftRect.Left + 1,
					leftRect.Top + 1,
					Math.Max(0,leftRect.Width - 2),
					Math.Max(0,leftRect.Height - 2));

				var rightPaneArea = new Rect(rightRect.Left + 1,
					rightRect.Top + 1,
					Math.Max(0, rightRect.Width - 2),
					Math.Max(0, rightRect.Height - 2));

				paneMetrics = new PaneMetrics(leftRect, 
                    rightRect,
                    leftPaneArea,
                    rightPaneArea);
			}

			// Public methods -------------------------------------------------

            public Metrics()
            {
                
            }

			public void Invalidate()
            {
                InvalidateGeneralMetrics();
                InvalidatePaneMetrics();
            }

            public void Validate()
            {
                if (paneMetrics == null)
                    ValidatePaneMetrics();

                if (generalMetrics == null)
                    ValidateGeneralMetrics();
            }

            // Public properties ----------------------------------------------

            public GeneralMetrics General => generalMetrics;

            public PaneMetrics Pane => paneMetrics;

            public double Width
            {
                get => width;
                set => SetWidth(value);
            }

            public double Height
            {
                get => height;
                set => SetHeight(value);
            }

            public bool Valid => generalMetrics != null && 
                paneMetrics != null;

            public Thickness Padding
            {
                get => padding;
                set => SetPadding(value);
            }

            public double PixelsPerDip
            {
                get => pixelsPerDip;
                set => SetPixelsPerDip(value);
            }
        }
    }
}

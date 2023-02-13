using File.Manager.Types;
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

            private const int SPACE_BETWEEN_PANES_DIP = 4;

            private const int PANE_BORDER_THICKNESS_DIP = 1;

            // Evaluated

            private const int MIN_WIDTH_DIP = 2 * PANE_BORDER_THICKNESS_DIP + 1 +
                    SPACE_BETWEEN_PANES_DIP +
                    2 * PANE_BORDER_THICKNESS_DIP + 1;
            private const int MIN_HEIGHT_DIP = 2 * PANE_BORDER_THICKNESS_DIP + 1 +
				SPACE_BETWEEN_PANES_DIP +
				2 * PANE_BORDER_THICKNESS_DIP + 1;

			// Public types ---------------------------------------------------

            public class GeneralMetrics
            {
                public GeneralMetrics(PixelRectangle controlArea)
                {
                    ControlArea = controlArea;
                }

                public PixelRectangle ControlArea { get; }
            }

			public class PaneMetrics
            {
                public PaneMetrics (PixelRectangle leftPaneBounds, 
                    PixelRectangle rightPaneBounds,
                    PixelRectangle leftPaneArea,
                    PixelRectangle rightPaneArea)
                {
                    LeftPaneBounds = leftPaneBounds;
                    RightPaneBounds = rightPaneBounds;
                    LeftPaneArea = leftPaneArea;
                    RightPaneArea = rightPaneArea;
                }   

                public PixelRectangle LeftPaneBounds { get; }
                public PixelRectangle RightPaneBounds { get; }
                public PixelRectangle LeftPaneArea { get; }
				public PixelRectangle RightPaneArea { get; }
			}

            // Private fields -------------------------------------------------

            private GeneralMetrics generalMetrics;
			private PaneMetrics paneMetrics;

            private double width;
            private double height;
            private Thickness padding;
            private double pixelsPerDip;

            // Private methods ------------------------------------------------

            private double PxToDip(double pixels) => pixels / pixelsPerDip;

            private double DipToPx(double dip) => dip * pixelsPerDip;

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

                PixelRectangle controlArea = new PixelRectangle(0, 0, Math.Max(0, (int)Width - 1), Math.Max(0, (int)Height - 1));

                generalMetrics = new GeneralMetrics(controlArea);
            }

			private void ValidatePaneMetrics()
			{
                if (paneMetrics != null)
                    return;

                PixelRectangle leftRectangle;
                PixelRectangle rightRectangle;

                if (Width - Padding.Left - Padding.Right < DipToPx(MIN_WIDTH_DIP) || 
                    Height - Padding.Top - Padding.Bottom < DipToPx(MIN_HEIGHT_DIP))
                {
                    leftRectangle = new PixelRectangle(0, 0, 0, 0);
                    rightRectangle = new PixelRectangle(0, 0, 0, 0);
                }
                else
                {
                    int PaneWidth = (int)((Width - Padding.Left - Padding.Right - SPACE_BETWEEN_PANES_DIP) / 2);
                    int PaneHeight = (int)(Height - Padding.Top - Padding.Bottom);

                    int leftRectLeft = (int)Padding.Left;
                    int leftRectTop = (int)Padding.Top;

                    leftRectangle = new PixelRectangle(leftRectLeft,
                        leftRectTop,
                        leftRectLeft + PaneWidth - 1,
                        leftRectTop + PaneHeight - 1);

                    int rightRectLeft = (int)Width - 1 - (int)Padding.Right - PaneWidth;
                    int rightRectTop = (int)Padding.Top;

                    rightRectangle = new PixelRectangle(rightRectLeft,
                        rightRectTop,
                        rightRectLeft + PaneWidth - 1,
                        rightRectTop + PaneHeight - 1);
                }

                var leftPaneArea = leftRectangle.Offset(1, 1, -1, -1);
				var rightPaneArea = rightRectangle.Offset(1, 1, -1, -1);

				paneMetrics = new PaneMetrics(leftRectangle, 
                    rightRectangle,
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

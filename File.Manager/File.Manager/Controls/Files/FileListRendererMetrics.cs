using File.Manager.BusinessLogic.Models.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace File.Manager.Controls.Files
{
    internal abstract class FileListRendererMetrics
    {
        private Rect bounds;
        private double pixelsPerDip;
        private IReadOnlyList<FileListColumn> columns;
        private string fontFamily;
        private double fontSize;

        private void SetBounds(Rect newBounds)
        {
            if (bounds != newBounds)
            {
                bounds = newBounds;
                Invalidate();
            }
        }

        private void SetPixelsPerDip(double newPixelsPerDip)
        {
            if (pixelsPerDip != newPixelsPerDip) 
            {
                pixelsPerDip = newPixelsPerDip;
                Invalidate();
            }
        }

        private void SetColumns(IReadOnlyList<FileListColumn> newColumns)
        {
            if (columns != newColumns)
            {
                columns = newColumns;
                Invalidate();
            }
        }

        private void SetFontFamily(string newFontFamily)
        {
            if (fontFamily != newFontFamily)
            {
                fontFamily = newFontFamily;
                Invalidate();
            }
        }

        private void SetFontSize(double newFontSize)
        {
            if (fontSize != newFontSize)
            {
                fontSize = newFontSize;
                Invalidate();
            }
        }

        protected double PxToDip(double pixels) => pixels / pixelsPerDip;

        protected double DipToPixels(double dip) => dip * pixelsPerDip;

        public abstract void Invalidate();

        public abstract void Validate();

        public Rect Bounds
        {
            get => bounds;
            set => SetBounds(value);
        }

        public double PixelsPerDip
        {
            get => pixelsPerDip;
            set => SetPixelsPerDip(value);
        }

        public IReadOnlyList<FileListColumn> Columns
        {
            get => columns;
            set => SetColumns(value);
        }

        public string FontFamily
        {
            get => fontFamily;
            set => SetFontFamily(value);
        }

        public double FontSize
        {
            get => fontSize;
            set => SetFontSize(value);
        }

        public abstract bool Valid { get; }
    }
}

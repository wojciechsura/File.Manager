using File.Manager.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace File.Manager.Types
{
    public class PixelRectangle : BaseIntRectangle<PixelRectangle, PixelPoint>
    {
        public PixelRectangle(int left, int top, int right, int bottom) 
            : base(left, top, right, bottom)
        {

        }

        public Rect ToRect(RectConversionPurpose purpose) => purpose switch
        {
            RectConversionPurpose.Pen => new Rect(Left + 0.5, Top + 0.5, Width, Height),
            RectConversionPurpose.None or RectConversionPurpose.Brush => new Rect(Left, Top, Width, Height),
            _ => throw new InvalidOperationException("Unsupported RectConversionPurpose")
        };

        protected override PixelPoint CreatePoint(int x, int y)
            => new PixelPoint(x, y);

        protected override PixelRectangle CreateRectangle(int left, int top, int right, int bottom)
            => new PixelRectangle(left, top, right, bottom);        
    }
}

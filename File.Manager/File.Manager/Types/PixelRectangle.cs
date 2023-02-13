﻿using File.Manager.Geometry;
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

        public Rect ToPenRect(double penWidth) => new Rect(Left + penWidth / 2.0, Top + penWidth / 2.0, Math.Max(0, Width - penWidth), Math.Max(0, Height - penWidth));

        public Rect ToBrushRect() => new Rect(Left, Top, Width, Height);

        public Rect ToRegionRect() => new Rect(Left, Top, Width, Height);

        protected override PixelPoint CreatePoint(int x, int y)
            => new PixelPoint(x, y);

        protected override PixelRectangle CreateRectangle(int left, int top, int right, int bottom)
            => new PixelRectangle(left, top, right, bottom);        
    }
}

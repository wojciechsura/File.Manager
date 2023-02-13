/******************************************************************************
*                                                                             *
* This code was generated automatically from a template. Don't modify it,     *
* because all your changes will be overwritten. Instead, if needed, modify    *
* the template file (*.tt)                                                    *
*                                                                             *
******************************************************************************/

using System;

namespace File.Manager.Geometry
{
    // *** BaseIntRectangle ***

    public abstract class BaseIntRectangle<TConcreteRectangle, TConcretePoint>
	    where TConcreteRectangle : BaseIntRectangle<TConcreteRectangle, TConcretePoint>
		where TConcretePoint : BaseIntPoint<TConcretePoint>
    {
        // Protected methods ------------------------------------------------------

        protected abstract TConcreteRectangle CreateRectangle(int left, int top, int width, int height);
        protected abstract TConcretePoint CreatePoint(int x, int y);

        // Public methods ---------------------------------------------------------

        public BaseIntRectangle(int left, int top, int width, int height)
        {
            if (width < 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height < 0)
                throw new ArgumentOutOfRangeException(nameof(height));

	        Left = left;
            Top = top;
            Width = width;
            Height = height;
        }

		public bool Contains(TConcretePoint point)
		{
			return point.X >= Left && point.X <= Right && point.Y >= Top && point.Y <= Bottom;
		}

		public TConcreteRectangle ExtendBy(TConcretePoint point)
		{
            int newLeft = Math.Min(Left, point.X);
            int newTop = Math.Min(Top, point.Y);
            int newRight = Math.Max(Right, point.X);
            int newBottom = Math.Max(Bottom, point.Y);

            return CreateRectangle(newLeft, newTop, newRight - newLeft + 1, newBottom - newTop + 1);
		}

        public TConcreteRectangle Offset(int dLeft, int dTop) 
        {
            int newLeft = Left + dLeft;
            int newTop = Top + dTop;

            return CreateRectangle(newLeft, newTop, Width, Height);
        }

        public TConcreteRectangle OffsetSize(int dWidth, int dHeight)
        {
            int newWidth = Math.Max(0, Width + dWidth);
            int newHeight = Math.Max(0, Height + dHeight);

            return CreateRectangle(Left, Top, newWidth, newHeight);
        }

		public bool IntersectsWith(TConcreteRectangle rect) 
		{
			return rect.Left <= Right &&
				rect.Right >= Left &&
				rect.Top <= Bottom &&
				rect.Bottom >= Top;
		}

        // Public properties ------------------------------------------------------

        public int Left { get; }    
        public int Top { get; }
        public int Right => Left + Width - 1;
        public int Bottom => Top + Height - 1;
        public int Width { get; }
        public int Height { get; }

        public TConcretePoint Center => CreatePoint(Left + Width / 2, Top + Height / 2);
    }

    // *** BaseFloatRectangle ***

    public abstract class BaseFloatRectangle<TConcreteRectangle, TConcretePoint>
	    where TConcreteRectangle : BaseFloatRectangle<TConcreteRectangle, TConcretePoint>
		where TConcretePoint : BaseFloatPoint<TConcretePoint>
    {
        // Protected methods ------------------------------------------------------

        protected abstract TConcreteRectangle CreateRectangle(float left, float top, float width, float height);
        protected abstract TConcretePoint CreatePoint(float x, float y);

        // Public methods ---------------------------------------------------------

        public BaseFloatRectangle(float left, float top, float width, float height)
        {
            if (float.IsNaN(left))
			    throw new ArgumentException(nameof(left));
            if (float.IsNaN(top))
			    throw new ArgumentException(nameof(top));
            if (float.IsNaN(width))
			    throw new ArgumentException(nameof(width));
            if (float.IsNaN(height))
			    throw new ArgumentException(nameof(height));
            if (width < 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height < 0)
                throw new ArgumentOutOfRangeException(nameof(height));

	        Left = left;
            Top = top;
            Width = width;
            Height = height;
        }

		public bool Contains(TConcretePoint point)
		{
			return point.X >= Left && point.X <= Right && point.Y >= Top && point.Y <= Bottom;
		}

		public TConcreteRectangle ExtendBy(TConcretePoint point)
		{
            float newLeft = Math.Min(Left, point.X);
            float newTop = Math.Min(Top, point.Y);
            float newRight = Math.Max(Right, point.X);
            float newBottom = Math.Max(Bottom, point.Y);

            return CreateRectangle(newLeft, newTop, newRight - newLeft, newBottom - newTop);
		}

        public TConcreteRectangle Offset(float dLeft, float dTop) 
        {
            float newLeft = Left + dLeft;
            float newTop = Top + dTop;

            return CreateRectangle(newLeft, newTop, Width, Height);
        }

        public TConcreteRectangle OffsetSize(float dWidth, float dHeight)
        {
            float newWidth = Math.Max(0, Width + dWidth);
            float newHeight = Math.Max(0, Height + dHeight);

            return CreateRectangle(Left, Top, newWidth, newHeight);
        }

		public bool IntersectsWith(TConcreteRectangle rect) 
		{
			return rect.Left <= Right &&
				rect.Right >= Left &&
				rect.Top <= Bottom &&
				rect.Bottom >= Top;
		}

        // Public properties ------------------------------------------------------

        public float Left { get; }    
        public float Top { get; }
        public float Right => Left + Width;
        public float Bottom => Top + Height;
        public float Width { get; }
        public float Height { get; }

        public TConcretePoint Center => CreatePoint(Left + Width / 2.0f, Top + Height / 2.0f);
    }

    // *** BaseDoubleRectangle ***

    public abstract class BaseDoubleRectangle<TConcreteRectangle, TConcretePoint>
	    where TConcreteRectangle : BaseDoubleRectangle<TConcreteRectangle, TConcretePoint>
		where TConcretePoint : BaseDoublePoint<TConcretePoint>
    {
        // Protected methods ------------------------------------------------------

        protected abstract TConcreteRectangle CreateRectangle(double left, double top, double width, double height);
        protected abstract TConcretePoint CreatePoint(double x, double y);

        // Public methods ---------------------------------------------------------

        public BaseDoubleRectangle(double left, double top, double width, double height)
        {
            if (double.IsNaN(left))
			    throw new ArgumentException(nameof(left));
            if (double.IsNaN(top))
			    throw new ArgumentException(nameof(top));
            if (double.IsNaN(width))
			    throw new ArgumentException(nameof(width));
            if (double.IsNaN(height))
			    throw new ArgumentException(nameof(height));
            if (width < 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height < 0)
                throw new ArgumentOutOfRangeException(nameof(height));

	        Left = left;
            Top = top;
            Width = width;
            Height = height;
        }

		public bool Contains(TConcretePoint point)
		{
			return point.X >= Left && point.X <= Right && point.Y >= Top && point.Y <= Bottom;
		}

		public TConcreteRectangle ExtendBy(TConcretePoint point)
		{
            double newLeft = Math.Min(Left, point.X);
            double newTop = Math.Min(Top, point.Y);
            double newRight = Math.Max(Right, point.X);
            double newBottom = Math.Max(Bottom, point.Y);

            return CreateRectangle(newLeft, newTop, newRight - newLeft, newBottom - newTop);
		}

        public TConcreteRectangle Offset(double dLeft, double dTop) 
        {
            double newLeft = Left + dLeft;
            double newTop = Top + dTop;

            return CreateRectangle(newLeft, newTop, Width, Height);
        }

        public TConcreteRectangle OffsetSize(double dWidth, double dHeight)
        {
            double newWidth = Math.Max(0, Width + dWidth);
            double newHeight = Math.Max(0, Height + dHeight);

            return CreateRectangle(Left, Top, newWidth, newHeight);
        }

		public bool IntersectsWith(TConcreteRectangle rect) 
		{
			return rect.Left <= Right &&
				rect.Right >= Left &&
				rect.Top <= Bottom &&
				rect.Bottom >= Top;
		}

        // Public properties ------------------------------------------------------

        public double Left { get; }    
        public double Top { get; }
        public double Right => Left + Width;
        public double Bottom => Top + Height;
        public double Width { get; }
        public double Height { get; }

        public TConcretePoint Center => CreatePoint(Left + Width / 2.0, Top + Height / 2.0);
    }

}
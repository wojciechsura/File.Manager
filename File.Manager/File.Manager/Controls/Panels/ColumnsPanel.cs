﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace File.Manager.Controls.Panels
{
    public class ColumnsPanel : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            List<double> rowHeights = new();

            double columnWidth = Math.Max(MinColumnWidth, (availableSize.Width - ColumnSpacing * (ColumnCount - 1)) / ColumnCount );

            for (int i = 0; i < InternalChildren.Count; i++)
            {
                int row = i / ColumnCount;
                if (rowHeights.Count < row + 1)
                    rowHeights.Add(0.0);

                InternalChildren[i].Measure(new Size(columnWidth, double.PositiveInfinity));
                rowHeights[row] = Math.Max(rowHeights[row], InternalChildren[i].DesiredSize.Height);
            }

            // Integer version of Ceiling()
            int rows = (InternalChildren.Count + ColumnCount - 1) / ColumnCount;

            double resultWidth = (columnWidth * ColumnCount + ColumnSpacing * (ColumnCount - 1));
            double resultHeight = (rowHeights.Any() ? rowHeights.Sum() : 0.0) + (Math.Max(0, rows - 1)) * RowSpacing;

            return new Size(resultWidth, resultHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double columnWidth = Math.Max(MinColumnWidth, (finalSize.Width - ColumnSpacing * (ColumnCount - 1)) / ColumnCount);

            List<double> rowHeights = new();
            double runningY = 0.0;

            for (int i = 0; i < InternalChildren.Count; i++)
            {
                int row = i / ColumnCount;
                int col = i % ColumnCount;

                if (row > 0 && col == 0)
                {
                    runningY += rowHeights[row - 1] + RowSpacing;                    
                }

                if (rowHeights.Count < row + 1)
                    rowHeights.Add(0.0);

                rowHeights[row] = Math.Max(rowHeights[row], InternalChildren[i].DesiredSize.Height);

                InternalChildren[i].Arrange(new Rect(col * (columnWidth + ColumnSpacing), runningY, columnWidth, InternalChildren[i].DesiredSize.Height));
            }

            // Integer version of Ceiling()
            int rows = (InternalChildren.Count + ColumnCount - 1) / ColumnCount;

            double resultWidth = (columnWidth * ColumnCount + ColumnSpacing * (ColumnCount - 1));
            double resultHeight = (rowHeights.Any() ? rowHeights.Sum() : 0.0) + (Math.Max(0, rows - 1)) * RowSpacing;

            return new Size(resultWidth, resultHeight);
        }

        #region ColumnCount dependency property

        public int ColumnCount
        {
            get { return (int)GetValue(ColumnCountProperty); }
            set { SetValue(ColumnCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColumnCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnCountProperty =
            DependencyProperty.Register("ColumnCount", typeof(int), typeof(ColumnsPanel), new FrameworkPropertyMetadata(2, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange, null, CoerceColumnCount));

        private static object CoerceColumnCount(DependencyObject d, object baseValue)
        {
            var value = (int)baseValue;
            return Math.Max(1, value);
        }

        #endregion

        #region ColumnSpacing dependency property

        public double ColumnSpacing
        {
            get { return (double)GetValue(ColumnSpacingProperty); }
            set { SetValue(ColumnSpacingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColumnSpacing.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnSpacingProperty =
            DependencyProperty.Register("ColumnSpacing", typeof(double), typeof(ColumnsPanel), new PropertyMetadata(16.0));

        #endregion

        #region RowSpacing dependency property

        public double RowSpacing
        {
            get { return (double)GetValue(RowSpacingProperty); }
            set { SetValue(RowSpacingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RowSpacing.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RowSpacingProperty =
            DependencyProperty.Register("RowSpacing", typeof(double), typeof(ColumnsPanel), new PropertyMetadata(8.0));

        #endregion

        #region MinColumnWidth dependencyProperty

        public int MinColumnWidth
        {
            get { return (int)GetValue(MinColumnWidthProperty); }
            set { SetValue(MinColumnWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinColumnWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinColumnWidthProperty =
            DependencyProperty.Register("MinColumnWidth", typeof(int), typeof(ColumnsPanel), new FrameworkPropertyMetadata(100, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange, null, CoerceMinColumnWidth));

        private static object CoerceMinColumnWidth(DependencyObject d, object baseValue)
        {
            var value = (int)baseValue;
            return Math.Max(1, value);
        }

        #endregion
    }
}

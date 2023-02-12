using File.Manager.API.Filesystem.Models.Items;
using File.Manager.BusinessLogic.Models.Files;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace File.Manager.Controls.Files
{

    // TODO:
    // 1. Unify Rectangles to RectanglesF
    // 2. Remove parameters in Notify* passed to renderers
    // 3. Finish FileListGridRendererMetrics.Validate*
    // 4. Render header

    public partial class FileList : FrameworkElement, IFileListRendererHost
    {
        // Private constants --------------------------------------------------

        private static readonly FileListAppearance DefaultAppearance = new FileListAppearance();

        // Private types ------------------------------------------------------

        private FileListRenderer firstRenderer;
        private FileListRenderer secondRenderer;
        private readonly Metrics metrics;
        private bool firstIsLeft;

        // Private methods ----------------------------------------------------

        private void HandleAppearanceChanged(object sender, EventArgs e)
        {
            InvalidateVisual();
        }

        private void ValidateMetrics()
        {
            if (!metrics.Valid)
            {
                metrics.Validate();

                if (firstIsLeft)
                {
                    firstRenderer.NotifyBoundsChanged(metrics.Pane.LeftPaneArea);
                    secondRenderer.NotifyBoundsChanged(metrics.Pane.RightPaneArea);
                }
                else
                {
                    secondRenderer.NotifyBoundsChanged(metrics.Pane.LeftPaneArea);
                    firstRenderer.NotifyBoundsChanged(metrics.Pane.RightPaneArea);
                }
            }
        }

        private void ForEachRenderer(Action<FileListRenderer> action)
        {
            action(firstRenderer);
            action(secondRenderer);
        }

        // Protected methods --------------------------------------------------

        protected override void OnRender(DrawingContext drawingContext)
        {
            ValidateMetrics();

            drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight)));
            try
            {
                var appearance = Appearance ?? DefaultAppearance;

                // Background
                drawingContext.DrawRectangle(appearance.Background, null, metrics.General.ControlArea);

                var panePen = new Pen(appearance.PaneBorderBrush, metrics.PixelsPerDip * 1.0);

                // Left pane
                drawingContext.DrawRectangle(appearance.PaneBackgroundBrush,
                    panePen,
                    metrics.Pane.LeftPaneBounds);

                drawingContext.PushClip(new RectangleGeometry(metrics.Pane.LeftPaneArea));
                try
                {
                    // Draw left pane contents
                }
                finally
                {
                    drawingContext.Pop();
                }

                // Right pane
                drawingContext.DrawRectangle(appearance.PaneBackgroundBrush,
                    panePen,
                    metrics.Pane.RightPaneBounds);

                drawingContext.PushClip(new RectangleGeometry(metrics.Pane.RightPaneArea));
                try
                {
                    // Draw right pane contents
                }
                finally
                {
                    drawingContext.Pop();
                }
            }
            finally
            {
                drawingContext.Pop();
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            metrics.Width = sizeInfo.NewSize.Width;
            metrics.Height = sizeInfo.NewSize.Height;

            InvalidateVisual();
        }

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);

            metrics.PixelsPerDip = newDpi.PixelsPerDip;

            ForEachRenderer(renderer => renderer.NotifyDpiChanged(metrics.PixelsPerDip));
        }

        // IFileListRendererHost implementation -------------------------------

        void IFileListRendererHost.RequestInvalidateVisual()
        {
            InvalidateVisual();
        }

        FileListAppearance IFileListRendererHost.Appearance => Appearance ?? DefaultAppearance;

        IReadOnlyList<FileListColumn> IFileListRendererHost.Columns => this.Columns;

        string IFileListRendererHost.FontFamily => this.FontFamily;

        double IFileListRendererHost.FontSize => this.FontSize;

        // Public methods -----------------------------------------------------

        public FileList()
        {
            metrics = new();

            firstRenderer = new FileListGridRenderer(this);
            secondRenderer = new FileListGridRenderer(this);

            metrics.PixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
        }

        // Public properties --------------------------------------------------

        #region Padding dependency property

        public Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }
        
        // Using a DependencyProperty as the backing store for Padding.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PaddingProperty =
            DependencyProperty.Register("Padding", typeof(Thickness), typeof(FileList), new PropertyMetadata(new Thickness(0), PaddingPropertyChanged));

        private static void PaddingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileList fileList)
            {
                fileList.PaddingChanged();
            }
        }

        private void PaddingChanged()
        {
            metrics.Padding = Padding;
            InvalidateVisual();
        }

        #endregion

        #region Appearance dependency property

        public FileListAppearance Appearance
        {
            get { return (FileListAppearance)GetValue(AppearanceProperty); }
            set { SetValue(AppearanceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Appearance.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AppearanceProperty =
            DependencyProperty.Register("Appearance", typeof(FileListAppearance), typeof(FileList), new PropertyMetadata(null, AppearancePropertyChanged));

        private static void AppearancePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileList fileList)
                fileList.AppearanceChanged(e.OldValue as FileListAppearance, e.NewValue as FileListAppearance);
        }

        private void AppearanceChanged(FileListAppearance oldValue, FileListAppearance newValue)
        {
            if (oldValue != null)
                oldValue.AppearanceChanged -= HandleAppearanceChanged;

            if (newValue != null)
                newValue.AppearanceChanged += HandleAppearanceChanged;

            InvalidateVisual();
        }

        #endregion

        #region FirstSource dependency property

        public ICollectionView FirstSource
        {
            get { return (ICollectionView)GetValue(FirstSourceProperty); }
            set { SetValue(FirstSourceProperty, value); }
        }

        public static readonly DependencyProperty FirstSourceProperty =
            DependencyProperty.Register("FirstSource", typeof(ICollectionView), typeof(FileList), new PropertyMetadata(null, FirstSourcePropertyChanged));

        private static void FirstSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileList fileLister)
                fileLister.FirstSourceChanged();
        }

        private void FirstSourceChanged()
        {
            // TODO
        }

        #endregion

        #region SecondSource dependency property

        public ICollectionView SecondSource
        {
            get { return (ICollectionView)GetValue(SecondSourceProperty); }
            set { SetValue(SecondSourceProperty, value); }
        }

        public static readonly DependencyProperty SecondSourceProperty =
            DependencyProperty.Register("SecondSource", typeof(ICollectionView), typeof(FileList), new PropertyMetadata(null, SecondSourcePropertyChanged));

        private static void SecondSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileList fileLister)
                fileLister.SecondSourceChanged();
        }

        private void SecondSourceChanged()
        {
            // TODO
        }

        #endregion

        #region Columns dependency property

        public FileListColumnCollection Columns
        {
            get { return (FileListColumnCollection)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Columns.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register("Columns", typeof(FileListColumnCollection), typeof(FileList), new PropertyMetadata(null, ColumnsPropertyChanged));

        private static void ColumnsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileList fileList)
            {
                fileList.ColumnsChanged(e.OldValue as FileListColumnCollection, e.NewValue as FileListColumnCollection);
            }
        }

        private void ColumnsChanged(FileListColumnCollection oldValue, FileListColumnCollection newValue)
        {
            ForEachRenderer(renderer =>
            {
                renderer.Columns = newValue;
            });
        }

        #endregion

        #region FontFamily dependency property

        public string FontFamily
        {
            get { return (string)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FontFamily.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontFamilyProperty =
            DependencyProperty.Register("FontFamily", typeof(string), typeof(FileList), new PropertyMetadata("Calibri", FontFamilyPropertyChanged));

        private static void FontFamilyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileList fileList)
                fileList.FontFamilyChanged();
        }

        private void FontFamilyChanged()
        {
            ForEachRenderer(renderer => renderer.NotifyFontChanged(FontFamily, FontSize));
        }

        #endregion

        #region FontSize dependency property

        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FontSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize", typeof(double), typeof(FileList), new PropertyMetadata(11.0, FontSizePropertyChanged));

        private static void FontSizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileList fileList)
            {
                fileList.FontSizeChanged();
            }
        }

        private void FontSizeChanged()
        {
            ForEachRenderer(renderer => renderer.NotifyFontChanged(FontFamily, FontSize));
        }

        #endregion
    }
}

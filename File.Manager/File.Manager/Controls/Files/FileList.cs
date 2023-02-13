using File.Manager.API.Filesystem.Models.Items;
using File.Manager.BusinessLogic.Models.Files;
using File.Manager.Tools;
using File.Manager.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace File.Manager.Controls.Files
{
    public partial class FileList : FrameworkElement
    {
        // Private constants --------------------------------------------------

        private static readonly FileListAppearance DefaultAppearance = new FileListAppearance();

        // Private types ------------------------------------------------------

        private sealed class FileListRendererHost : IFileListRendererHost
        {
            private readonly FileList fileList;
            private readonly Func<PixelRectangle> getBounds;

            public FileListRendererHost(FileList fileList,
                Func<PixelRectangle> getBounds)
            {
                this.fileList = fileList;
                this.getBounds = getBounds;
            }

            public void RequestInvalidateVisual()
            {
                fileList.InvalidateVisual();
            }

            public PixelRectangle Bounds => getBounds();

            public FileListAppearance Appearance => fileList.Appearance ?? FileList.DefaultAppearance;

            public IReadOnlyList<FileListColumn> Columns => fileList.Columns;

            public System.Windows.Media.FontFamily FontFamily => fileList.FontFamily;

            public double FontSize => fileList.FontSize;

            public double PixelsPerDip => fileList.metrics.PixelsPerDip;
        }

        // Private fields -----------------------------------------------------

        private FileListRenderer firstRenderer;
        private readonly FileListRendererHost firstHost;

        private FileListRenderer secondRenderer;
        private readonly FileListRendererHost secondHost;
        
        private readonly Metrics metrics;
        private bool panesSwitched;

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
                ForEachRenderer(renderer => renderer.NotifyMetricsChanged());
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
                drawingContext.DrawRectangle(appearance.Background, null, metrics.General.ControlArea.ToBrushRect());

                var panePen = new System.Windows.Media.Pen(appearance.PaneBorderBrush, metrics.PixelsPerDip * 1.0);

                // Left pane
                drawingContext.DrawRectangle(appearance.PaneBackgroundBrush,
                    panePen,
                    metrics.Pane.LeftPaneBounds.ToPenRect(panePen.Thickness));

                //drawingContext.PushClip(new RectangleGeometry(metrics.Pane.LeftPaneArea.ToRect(RectConversionPurpose.None)));
                try
                {
                    if (panesSwitched)
                        secondRenderer.Render(drawingContext);
                    else
                        firstRenderer.Render(drawingContext);
                }
                finally
                {
                    //drawingContext.Pop();
                }

                // Right pane
                drawingContext.DrawRectangle(appearance.PaneBackgroundBrush,
                    panePen,
                    metrics.Pane.RightPaneBounds.ToPenRect(panePen.Thickness));

                drawingContext.PushClip(new RectangleGeometry(metrics.Pane.RightPaneArea.ToRegionRect()));
                try
                {
                    if (panesSwitched)
                        firstRenderer.Render(drawingContext);
                    else
                        secondRenderer.Render(drawingContext);
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

            ForEachRenderer(renderer => renderer.NotifyMetricsChanged());
        }        

        // Public methods -----------------------------------------------------

        public FileList()
        {
            metrics = new();

            panesSwitched = false;

            firstHost = new FileListRendererHost(this, () => panesSwitched ? metrics.Pane.RightPaneArea : metrics.Pane.LeftPaneArea);
            secondHost = new FileListRendererHost(this, () => panesSwitched ? metrics.Pane.LeftPaneArea : metrics.Pane.RightPaneArea);

            firstRenderer = new FileListGridRenderer(firstHost);
            secondRenderer = new FileListGridRenderer(secondHost);

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
            ForEachRenderer(renderer => renderer.Columns = newValue);
        }

        #endregion

        #region FontFamily dependency property

        public System.Windows.Media.FontFamily FontFamily
        {
            get { return (System.Windows.Media.FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        public static readonly DependencyProperty FontFamilyProperty =
                TextElement.FontFamilyProperty.AddOwner(typeof(FileList), 
                    new FrameworkPropertyMetadata(System.Windows.SystemFonts.CaptionFontFamily, FrameworkPropertyMetadataOptions.Inherits, FontFamilyPropertyChanged));

        private static void FontFamilyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileList fileList)
                fileList.FontFamilyChanged();
        }

        private void FontFamilyChanged()
        {
            ForEachRenderer(renderer => renderer.NotifyMetricsChanged());
        }

        #endregion

        #region FontSize dependency property

        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public static readonly DependencyProperty FontSizeProperty =
            TextElement.FontSizeProperty.AddOwner(typeof(FileList),
                new FrameworkPropertyMetadata(FontSizePropertyChanged));

        private static void FontSizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileList fileList)
            {
                fileList.FontSizeChanged();
            }
        }

        private void FontSizeChanged()
        {
            ForEachRenderer(renderer => renderer.NotifyMetricsChanged());
        }

        #endregion

        #region PanesSwitched dependency property

        public bool PanesSwitched
        {
            get { return (bool)GetValue(PanesSwitchedProperty); }
            set { SetValue(PanesSwitchedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PanesSwitched.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PanesSwitchedProperty =
            DependencyProperty.Register("PanesSwitched", typeof(bool), typeof(FileList), new PropertyMetadata(false, PanesSwitchedPropertyChanged));

        private static void PanesSwitchedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileList fileList)
            {
                fileList.PanesSwitchedChanged();
            }
        }

        private void PanesSwitchedChanged()
        {
            ForEachRenderer(renderer => renderer.NotifyMetricsChanged());
        }

        #endregion
    }
}

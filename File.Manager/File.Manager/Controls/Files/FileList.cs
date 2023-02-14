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
    public partial class FileList : FrameworkElement, IFileListRendererHost
    {
        // Private constants --------------------------------------------------

        private static readonly FileListAppearance DefaultAppearance = new();

        // Private fields -----------------------------------------------------

        private FileListRenderer renderer;        
        private readonly Metrics metrics;

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
                renderer.NotifyMetricsChanged();
            }
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

                // Pane

                drawingContext.DrawRectangle(appearance.PaneBackgroundBrush,
                    panePen,
                    metrics.Pane.PaneBounds.ToPenRect(panePen.Thickness));

                drawingContext.PushClip(new RectangleGeometry(metrics.Pane.PaneArea.ToRegionRect()));
                try
                {
                    renderer.Render(drawingContext);
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

            renderer.NotifyMetricsChanged();
        }

        // IFileListRendererHost implementation -------------------------------

        PixelRectangle IFileListRendererHost.Bounds => metrics.Pane.PaneBounds;

        FileListAppearance IFileListRendererHost.Appearance => Appearance ?? DefaultAppearance;

        System.Windows.Media.FontFamily IFileListRendererHost.FontFamily => FontFamily;

        double IFileListRendererHost.FontSize => FontSize;

        double IFileListRendererHost.PixelsPerDip => metrics.PixelsPerDip;

        void IFileListRendererHost.RequestInvalidateVisual()
        {
            InvalidateVisual();
        }

        // Public methods -----------------------------------------------------

        public FileList()
        {
            metrics = new();

            renderer = new FileListGridRenderer(this);

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

        public ICollectionView FilesSource
        {
            get { return (ICollectionView)GetValue(FilesSourceProperty); }
            set { SetValue(FilesSourceProperty, value); }
        }

        public static readonly DependencyProperty FilesSourceProperty =
            DependencyProperty.Register("FilesSource", typeof(ICollectionView), typeof(FileList), new PropertyMetadata(null, FirstSourcePropertyChanged));

        private static void FirstSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileList fileLister)
                fileLister.FirstSourceChanged();
        }

        private void FirstSourceChanged()
        {
            renderer.FilesSource = FilesSource;
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
            renderer.Columns = newValue;
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
            renderer.NotifyMetricsChanged();
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
            renderer.NotifyMetricsChanged();
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
            renderer.NotifyMetricsChanged();
        }

        #endregion
    }
}

using File.Manager.API.Filesystem.Models.Items;
using File.Manager.BusinessLogic.Models.Files;
using File.Manager.Controls.Files.Renderers.Grid;
using File.Manager.Geometry;
using File.Manager.Tools;
using File.Manager.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace File.Manager.Controls.Files
{
    public partial class FileList : FrameworkElement, IFileListRendererHost
    {
        // Private constants --------------------------------------------------

        private static readonly FileListAppearance DefaultAppearance = new();

        // Private fields -----------------------------------------------------

        private readonly Metrics metrics;
        private FileListRenderer renderer;
        private MouseButton lastMouseButton;
        private long lastMouseButtonDown;
        private Point lastMouseButtonPos;

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

        private void SetNewRenderer(FileListRenderer newRenderer)
        {
            if (newRenderer == null)
                throw new ArgumentNullException(nameof(newRenderer));

            renderer = newRenderer;

            renderer.Columns = Columns;
            renderer.FilesSource = FilesSource;

            // Trigger notifications, so that renderer updates itself

            renderer.NotifyMetricsChanged();
            renderer.UpdateScrollData();
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

                var panePen = new Pen(appearance.PaneBorderBrush, metrics.PixelsPerDip * 1.0);

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

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            InvalidateVisual();
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            InvalidateVisual();
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            
            if (!IsFocused)
                Focus();

            long nowTicks = DateTime.Now.Ticks;
            long diffMs = (nowTicks - lastMouseButtonDown) / TimeSpan.TicksPerMillisecond;
            Point clickPosition = e.GetPosition(this);
            Size diffSize = new Size(Math.Abs(lastMouseButtonPos.X - clickPosition.X),
                Math.Abs(lastMouseButtonPos.Y - clickPosition.Y));

            if (e.ChangedButton == lastMouseButton &&
                diffMs <= System.Windows.Forms.SystemInformation.DoubleClickTime &&
                diffSize.Width <= System.Windows.Forms.SystemInformation.DoubleClickSize.Width &&
                diffSize.Height <= System.Windows.Forms.SystemInformation.DoubleClickSize.Height)
            {
                // Double click

                // Reset double click data
                lastMouseButton = MouseButton.Left;
                lastMouseButtonDown = 0;
                lastMouseButtonPos = new Point(0, 0);

                renderer.OnMouseDoubleClick(e);
            }
            else
            {
                lastMouseButton = e.ChangedButton;
                lastMouseButtonDown = DateTime.Now.Ticks;
                lastMouseButtonPos = clickPosition;

                renderer.OnMouseDown(e);
            }
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            renderer.OnMouseMove(e);
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            renderer.OnMouseUp(e);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            renderer.OnKeyDown(e);
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            renderer.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            renderer.OnMouseLeave(e);
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);
            renderer.OnMouseWheel(e);
        }

        // IFileListRendererHost implementation -------------------------------

        void IFileListRendererHost.RequestInvalidateVisual()
        {
            InvalidateVisual();
        }

        void IFileListRendererHost.RequestExecuteCurrentItem()
        {
            var currentItem = FilesSource.CurrentItem;

            if (ExecuteCurrentItemCommand != null && ExecuteCurrentItemCommand.CanExecute(currentItem))
            {
                ExecuteCurrentItemCommand.Execute(currentItem);
            }
        }

        void IFileListRendererHost.RequestMouseCapture()
        {
            CaptureMouse();
        }

        void IFileListRendererHost.RequestMouseRelease()
        {
            ReleaseMouseCapture();
        }

        PixelRectangle IFileListRendererHost.Bounds => metrics.Pane.PaneArea;

        bool IFileListRendererHost.IsActive => IsActive;

        bool IFileListRendererHost.IsFocused => IsFocused;

        FileListAppearance IFileListRendererHost.Appearance => Appearance ?? DefaultAppearance;

        FontFamily IFileListRendererHost.FontFamily => FontFamily;

        FontWeight IFileListRendererHost.FontWeight => FontWeight;

        double IFileListRendererHost.FontSize => FontSize;

        FontStyle IFileListRendererHost.FontStyle => FontStyle;

        FontStretch IFileListRendererHost.FontStretch => FontStretch;

        double IFileListRendererHost.PixelsPerDip => metrics.PixelsPerDip;

        int IFileListRendererHost.ScrollPosition 
        {
            get => this.ScrollPosition;
            set => this.ScrollPosition = value;
        }

        int IFileListRendererHost.ScrollMaximum 
        {
            get => this.ScrollMaximum;
            set => this.ScrollMaximum = value;
        }

        int IFileListRendererHost.ScrollSmallChange 
        {
            get => this.ScrollSmallChange;
            set => this.ScrollSmallChange = value;
        }

        int IFileListRendererHost.ScrollLargeChange 
        {
            get => this.ScrollLargeChange;
            set => this.ScrollLargeChange = value;
        }

        int IFileListRendererHost.ScrollViewportSize
        {
            get => this.ScrollViewportSize;
            set => this.ScrollViewportSize = value;
        }

        IInputElement IFileListRendererHost.InputElement => this;

        // Public methods -----------------------------------------------------

        public FileList()
        {
            metrics = new();
            metrics.Validate();

            SetNewRenderer(new FileListGridRenderer(this));

            metrics.PixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;

            Focusable = true;

            lastMouseButton = MouseButton.Left;
            lastMouseButtonDown = 0;
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

        public FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        public static readonly DependencyProperty FontFamilyProperty =
                TextElement.FontFamilyProperty.AddOwner(typeof(FileList), 
                    new FrameworkPropertyMetadata(TextElement.FontFamilyProperty.DefaultMetadata.DefaultValue, FrameworkPropertyMetadataOptions.Inherits, FontFamilyPropertyChanged));

        private static void FontFamilyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileList fileList)
                fileList.FontFamilyChanged();
        }

        private void FontFamilyChanged()
        {
            renderer.NotifyFontChanged();
            renderer.NotifyMetricsChanged();
        }

        #endregion

        #region FontStyle

        public FontStyle FontStyle
        {
            get { return (FontStyle)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        public static readonly DependencyProperty FontStyleProperty =
                TextElement.FontStyleProperty.AddOwner(typeof(FileList),
                    new FrameworkPropertyMetadata(TextElement.FontStyleProperty.DefaultMetadata.DefaultValue, FrameworkPropertyMetadataOptions.Inherits, FontStylePropertyChanged));

        private static void FontStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileList fileList)
                fileList.FontStyleChanged();
        }

        private void FontStyleChanged()
        {
            renderer.NotifyFontChanged();
            renderer.NotifyMetricsChanged();
        }

        #endregion

        #region FontWeight

        public FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        public static readonly DependencyProperty FontWeightProperty =
                TextElement.FontWeightProperty.AddOwner(typeof(FileList),
                    new FrameworkPropertyMetadata(TextElement.FontWeightProperty.DefaultMetadata.DefaultValue, FrameworkPropertyMetadataOptions.Inherits, FontWeightPropertyChanged));

        private static void FontWeightPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileList fileList)
                fileList.FontWeightChanged();
        }

        private void FontWeightChanged()
        {
            renderer.NotifyFontChanged();
            renderer.NotifyMetricsChanged();
        }

        #endregion

        #region FontStretch

        public FontStretch FontStretch
        {
            get { return (FontStretch)GetValue(FontStretchProperty); }
            set { SetValue(FontStretchProperty, value); }
        }

        public static readonly DependencyProperty FontStretchProperty =
                TextElement.FontStretchProperty.AddOwner(typeof(FileList),
                    new FrameworkPropertyMetadata(TextElement.FontStretchProperty.DefaultMetadata.DefaultValue, FrameworkPropertyMetadataOptions.Inherits, FontStretchPropertyChanged));

        private static void FontStretchPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileList fileList)
                fileList.FontStretchChanged();
        }

        private void FontStretchChanged()
        {
            renderer.NotifyFontChanged();
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
            renderer.NotifyFontChanged();
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

        #region ScrollPosition dependency property

        public int ScrollPosition
        {
            get { return (int)GetValue(ScrollPositionProperty); }
            set { SetValue(ScrollPositionProperty, value); }
        }

        public static readonly DependencyProperty ScrollPositionProperty =
            DependencyProperty.Register("ScrollPosition", typeof(int), typeof(FileList), new PropertyMetadata(0, HandleScrollPositionPropertyChanged, HandleScrollPositionPropertyCoerce));

        private static void HandleScrollPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileList fileList)
            {
                fileList.HandleScrollPositionChanged();
            }
        }

        private void HandleScrollPositionChanged()
        {
            if (ScrollPosition < 0 || ScrollPosition > ScrollMaximum)
                throw new ArgumentOutOfRangeException(nameof(ScrollPosition));

            renderer.NotifyScrollPositionChanged();
        }

        private static object HandleScrollPositionPropertyCoerce(DependencyObject d, object baseValue)
        {
            if (d is FileList fileList)
            {
                return fileList.HandleScrollPositionCoerce((int)baseValue);
            }

            return baseValue;
        }

        private object HandleScrollPositionCoerce(int scrollPosition)
        {
            return scrollPosition.ClampTo(0, ScrollMaximum);
        }

        #endregion

        #region ScrollMaximum dependency property

        public int ScrollMaximum
        {
            get => (int)GetValue(ScrollMaximumProperty);
            private set => SetValue(ScrollMaximumPropertyKey, value);
        }

        private static readonly DependencyPropertyKey ScrollMaximumPropertyKey =
            DependencyProperty.RegisterReadOnly("ScrollMaximum", typeof(int), typeof(FileList), new PropertyMetadata(0, ScrollMaximumPropertyChanged, ScrollMaximumPropertyCoerce));

        private static object ScrollMaximumPropertyCoerce(DependencyObject d, object baseValue)
        {
            var intValue = (int)baseValue;
            return Math.Max(0, intValue);
        }

        public static readonly DependencyProperty ScrollMaximumProperty = ScrollMaximumPropertyKey.DependencyProperty;

        private static void ScrollMaximumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileList fileList)
            {
                fileList.ScrollMaximumChanged();
            }
        }

        private void ScrollMaximumChanged()
        {
            CoerceValue(ScrollPositionProperty);
        }

        #endregion

        #region ScrollLargeChange dependency property

        public int ScrollLargeChange
        {
            get => (int)GetValue(ScrollLargeChangeProperty);
            private set => SetValue(ScrollLargeChangePropertyKey, value);
        }

        private static readonly DependencyPropertyKey ScrollLargeChangePropertyKey =
                    DependencyProperty.RegisterReadOnly("ScrollLargeChange", typeof(int), typeof(FileList), new PropertyMetadata(0));

        public static readonly DependencyProperty ScrollLargeChangeProperty = ScrollLargeChangePropertyKey.DependencyProperty;

        #endregion

        #region ScrollSmallChange dependency property

        public int ScrollSmallChange
        {
            get => (int)GetValue(ScrollSmallChangeProperty);
            private set => SetValue(ScrollSmallChangePropertyKey, value);
        }

        private static readonly DependencyPropertyKey ScrollSmallChangePropertyKey =
                    DependencyProperty.RegisterReadOnly("ScrollSmallChange", typeof(int), typeof(FileList), new PropertyMetadata(1));

        public static readonly DependencyProperty ScrollSmallChangeProperty = ScrollSmallChangePropertyKey.DependencyProperty;

        #endregion

        #region ScrollViewportSize dependency property

        public int ScrollViewportSize
        {
            get { return (int)GetValue(ScrollViewportSizeProperty); }
            set { SetValue(ScrollViewportSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ScrollViewportSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScrollViewportSizeProperty =
            DependencyProperty.Register("ScrollViewportSize", typeof(int), typeof(FileList), new PropertyMetadata(0));

        #endregion

        #region IsActive dependency property

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsActive.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(FileList), new PropertyMetadata(false, IsActivePropertyChanged));

        private static void IsActivePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileList fileList)
            {
                fileList.ActiveChanged();
            }
        }

        private void ActiveChanged()
        {
            InvalidateVisual();

            // Keep in mind, this happens only on change. It is possible that
            // pane is active and not focused.
            if (IsActive && !IsFocused)
                Focus();
        }

        #endregion

        #region ExecuteCurrentItemCommand dependency property

        public ICommand ExecuteCurrentItemCommand
        {
            get { return (ICommand)GetValue(ExecuteCurrentItemCommandProperty); }
            set { SetValue(ExecuteCurrentItemCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ExecuteCurrentItemCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExecuteCurrentItemCommandProperty =
            DependencyProperty.Register("ExecuteCurrentItemCommand", typeof(ICommand), typeof(FileList), new PropertyMetadata(null));

        #endregion
    }
}

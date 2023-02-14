using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace File.Manager.Controls.Files
{
    public class FileListAppearance : DependencyObject
    {
        // Private methods ----------------------------------------------------

        private static void AppearancePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileListAppearance fileListAppearance)
                fileListAppearance.OnAppearanceChanged();
        }

        private static LinearGradientBrush BuildHeaderBackgroundBrush()
        {
            var stops = new GradientStopCollection
            {
                new GradientStop(Color.FromArgb(0xff, 0xf8, 0xf8, 0xf8), 0.0),
                new GradientStop(Color.FromArgb(0xff, 0xf8, 0xf8, 0xf8), 0.66),
                new GradientStop(Color.FromArgb(0xff, 0xf0, 0xf0, 0xf0), 0.661),
                new GradientStop(Color.FromArgb(0xff, 0xf0, 0xf0, 0xf0), 1.0)
            };

            return new LinearGradientBrush(stops, 90.0);
        }

        // Protected methods --------------------------------------------------

        protected void OnAppearanceChanged()
        {
            AppearanceChanged?.Invoke(this, EventArgs.Empty);
        }

        // Public properties --------------------------------------------------

        #region Background dependency property

        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Background.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(Brush), typeof(FileListAppearance), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xff, 0xff)), AppearancePropertyChanged));

        #endregion

        #region PaneBorderBrush dependency property

        public Brush PaneBorderBrush
        {
            get { return (Brush)GetValue(PaneBorderBrushProperty); }
            set { SetValue(PaneBorderBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PaneBorderBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PaneBorderBrushProperty =
            DependencyProperty.Register("PaneBorderBrush", typeof(Brush), typeof(FileListAppearance), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0xff, 0xa0, 0xa0, 0xa0)), AppearancePropertyChanged));

        #endregion

        #region PaneBackgroundBrush dependency property

        public Brush PaneBackgroundBrush
        {
            get { return (Brush)GetValue(PaneBackgroundBrushProperty); }
            set { SetValue(PaneBackgroundBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PaneBackgroundBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PaneBackgroundBrushProperty =
            DependencyProperty.Register("PaneBackgroundBrush", typeof(Brush), typeof(FileListAppearance), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xff, 0xff)), AppearancePropertyChanged));

        #endregion

        #region HeaderBackgroundBrush dependency property

        public Brush HeaderBackgroundBrush
        {
            get { return (Brush)GetValue(HeaderBackgroundBrushProperty); }
            set { SetValue(HeaderBackgroundBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeaderBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderBackgroundBrushProperty =
            DependencyProperty.Register("HeaderBackgroundBrush", typeof(Brush), typeof(FileListAppearance), new PropertyMetadata(BuildHeaderBackgroundBrush(), AppearancePropertyChanged));

        #endregion

        #region HeaderForegroundBrush dependency property

        public Brush HeaderForegroundBrush
        {
            get { return (Brush)GetValue(HeaderForegroundBrushProperty); }
            set { SetValue(HeaderForegroundBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeaderForegroundBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderForegroundBrushProperty =
            DependencyProperty.Register("HeaderForegroundBrush", typeof(Brush), typeof(FileListAppearance), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0xff, 0x4c, 0x60, 0x7a)), AppearancePropertyChanged));

        #endregion

        #region HeaderSeparatorBrush dependency property

        public Brush HeaderSeparatorBrush
        {
            get { return (Brush)GetValue(HeaderSeparatorBrushProperty); }
            set { SetValue(HeaderSeparatorBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeaderSeparatorBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderSeparatorBrushProperty =
            DependencyProperty.Register("HeaderSeparatorBrush", typeof(Brush), typeof(FileListAppearance), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0xff, 0xe0, 0xe0, 0xe0)), AppearancePropertyChanged));

        #endregion

        #region ItemForegroundBrush dependency property

        public Brush ItemForegroundBrush
        {
            get { return (Brush)GetValue(ItemForegroundBrushProperty); }
            set { SetValue(ItemForegroundBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemForegroundBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemForegroundBrushProperty =
            DependencyProperty.Register("ItemForegroundBrush", typeof(Brush), typeof(FileListAppearance), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0xff, 0x40, 0x40, 0x40)), AppearancePropertyChanged));

        #endregion

        #region FocusBorderBrush dependency property

        public Brush FocusBorderBrush
        {
            get { return (Brush)GetValue(FocusBorderBrushProperty); }
            set { SetValue(FocusBorderBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FocusBorderBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FocusBorderBrushProperty =
            DependencyProperty.Register("FocusBorderBrush", typeof(Brush), typeof(FileListAppearance), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0xff, 0x99, 0xd1, 0xff)), AppearancePropertyChanged));

        #endregion

        #region FocusBackgroundBrush dependency property

        public Brush FocusBackgroundBrush
        {
            get { return (Brush)GetValue(FocusBackgroundBrushProperty); }
            set { SetValue(FocusBackgroundBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackgroundBorderBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FocusBackgroundBrushProperty =
            DependencyProperty.Register("FocusBackgroundBrush", typeof(Brush), typeof(FileListAppearance), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0xff, 0xcc, 0xe8, 0xff)), AppearancePropertyChanged));

        #endregion

        // Events

        public event EventHandler AppearanceChanged;
    }
}

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

        private static LinearGradientBrush BuildHeaderBrush()
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

        #region HeaderBrush dependency property

        public Brush HeaderBrush
        {
            get { return (Brush)GetValue(HeaderBrushProperty); }
            set { SetValue(HeaderBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeaderBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderBrushProperty =
            DependencyProperty.Register("HeaderBrush", typeof(Brush), typeof(FileListAppearance), new PropertyMetadata(BuildHeaderBrush(), AppearancePropertyChanged));

        #endregion

        #region HeaderForegroundBrush

        public Brush HeaderForegroundBrush
        {
            get { return (Brush)GetValue(HeaderForegroundBrushProperty); }
            set { SetValue(HeaderForegroundBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeaderForegroundBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderForegroundBrushProperty =
            DependencyProperty.Register("HeaderForegroundBrush", typeof(Brush), typeof(FileListAppearance), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0xff, 0x1e, 0x32, 0x87)), AppearancePropertyChanged));

        #endregion

        // Events

        public event EventHandler AppearanceChanged;
    }
}

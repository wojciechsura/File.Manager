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

        // Events

        public event EventHandler AppearanceChanged;
    }
}

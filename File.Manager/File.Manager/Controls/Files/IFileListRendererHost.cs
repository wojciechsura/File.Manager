using File.Manager.BusinessLogic.Models.Files;
using File.Manager.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace File.Manager.Controls.Files
{
    internal interface IFileListRendererHost
    {
        void RequestInvalidateVisual();

        PixelRectangle Bounds { get; }

        FileListAppearance Appearance { get; }

        FontFamily FontFamily { get; }

        double FontSize { get; }

        FontWeight FontWeight { get; }

        FontStyle FontStyle { get; }

        FontStretch FontStretch { get; }

        double PixelsPerDip { get; }

        int ScrollPosition { get; set; }
        
        int ScrollMaximum { get; set; }

        int ScrollSmallChange { get; set; }
        
        int ScrollLargeChange { get; set; }
    }
}

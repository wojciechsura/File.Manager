using File.Manager.BusinessLogic.Models.Files;
using File.Manager.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace File.Manager.Controls.Files
{
    internal interface IFileListRendererHost
    {
        void RequestInvalidateVisual();

        PixelRectangle Bounds { get; }

        FileListAppearance Appearance { get; }

        string FontFamily { get; }

        double FontSize { get; }

        double PixelsPerDip { get; }
    }
}

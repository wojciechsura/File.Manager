using File.Manager.BusinessLogic.Models.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.Controls.Files
{
    internal interface IFileListRendererHost
    {
        void RequestInvalidateVisual();

        FileListAppearance Appearance { get; }

        IReadOnlyList<FileListColumn> Columns { get; }

        string FontFamily { get; }

        double FontSize { get; }
    }
}

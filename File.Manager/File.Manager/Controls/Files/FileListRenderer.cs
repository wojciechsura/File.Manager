using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.Controls.Files
{
    internal abstract class FileListRenderer
    {
        protected readonly IFileListRendererHost host;

        protected FileListRenderer(IFileListRendererHost host)
        {
            this.host = host;
        }
    }
}

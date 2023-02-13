using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.Controls.Files
{
    internal class FileListGridRenderer : FileListMeasuredRenderer<FileListGridRendererMetrics>
    {
        public FileListGridRenderer(IFileListRendererHost host) 
            : base(host, new FileListGridRendererMetrics(host))
        {

        }

        protected override void HandleColumnsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            InvalidateMetrics();
            host.RequestInvalidateVisual();
        }

        protected override void HandleColumnsWidthsChanged(object sender, EventArgs e)
        {
            host.RequestInvalidateVisual();
        }
    }
}

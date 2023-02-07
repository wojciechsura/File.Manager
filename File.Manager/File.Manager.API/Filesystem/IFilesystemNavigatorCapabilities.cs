using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.API.Filesystem
{
    public interface IFilesystemNavigatorCapabilities
    {
        /// <summary>
        /// Determines, if navigator supports buffered copying
        /// (sending and receiving files by series of buffers)
        /// </summary>
        bool SupportsBufferedCopy { get; }

        /// <summary>
        /// Determines, if navigator supports fast copying
        /// within same navigator type (usually true when
        /// medium represented by navigator supports that)
        /// </summary>
        bool SupportsInModuleCopy { get; }
    }
}

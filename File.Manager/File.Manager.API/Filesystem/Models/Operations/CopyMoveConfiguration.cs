using File.Manager.API.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.API.Filesystem.Models.Operations
{
    public class CopyMoveConfiguration
    {
        public CopyMoveConfiguration(string fileMask, OverwritingOptions overwritingOptions)
        {
            FileMask = fileMask;
            OverwritingOptions = overwritingOptions;
        }

        public string FileMask { get; }
        public OverwritingOptions OverwritingOptions { get; }
    }
}

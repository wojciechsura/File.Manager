using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.API.Types
{
    [Flags]
    public enum LocationCapabilities
    {
        /// <summary>Can buffered-read files from the current location</summary>
        BufferedRead = 1,
        /// <summary>Can buffered-write files in the current location</summary>
        BufferedWrite = 2,
        /// <summary>Supports direct copy mechanism on supported navigators. This allows
        /// for faster copying without need to read and write buffers.</summary>
        DirectCopy = 4,
        /// <summary>Supports direct move mechanism on supported navigators. This allows
        /// for faster moving without need to read and write buffers.</summary>
        DirectMove = 8,
        /// <summary>Supports creating folders in the current location</summary>
        CreateFolder = 16,
        /// <summary>Supports deleting files and folders in the current location</summary>
        Delete = 32,
        /// <summary>Supports planning copy/move/delete operations before processing</summary>        
        Plan = 64
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.API.Tools
{
    public static class SizeTools
    {
        private static string[] sizeSuffixes = new[] { "B", "kB", "MB", "GB", "TB", "EB" };

        public static string BytesToHumanReadable(long bytes)
        {
            if (bytes < 1024L)
                return $"{bytes} B";
            else if (bytes < 1048576L)
                return $"{bytes / 1024.0:0.0} KB";
            else if (bytes < 1073741824L)
                return $"{bytes / 1048576.0:0.0} MB";
            else if (bytes < 1099511627776L)
                return $"{bytes / 1073741824.0:0.0} GB";
            else
                return $"{bytes / 1099511627776.0:0.0} TB";
        }
    }
}

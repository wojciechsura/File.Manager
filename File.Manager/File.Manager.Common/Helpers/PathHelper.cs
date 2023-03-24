using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.Common.Helpers
{
    public static class PathHelper
    {
        public static string EnsureTrailingSlash(string input)
            => input.EndsWith('/') ? input : $"{input}/";

    }
}

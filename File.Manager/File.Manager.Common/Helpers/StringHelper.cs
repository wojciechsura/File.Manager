using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.Common.Helpers
{
    public static class StringHelper
    {
        public static string ApplyControlChars(this string text)
        {
            StringBuilder result = new StringBuilder();
            foreach (char c in text)
            {
                if (c  == '\b' && result.Length > 0)
                {
                    result.Remove(result.Length - 1, 1);
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }
    }
}

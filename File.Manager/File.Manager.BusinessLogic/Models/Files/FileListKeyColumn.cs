using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Models.Files
{
    public class FileListKeyColumn : FileListColumn
    {
        public FileListKeyColumn(string header, string key)
            : base(header)
        {
            Key = key;
        }

        public string Key { get; }
    }
}

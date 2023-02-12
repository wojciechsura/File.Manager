using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Models.Files
{
    public class FileListKeyColumn : FileListColumn
    {
        public FileListKeyColumn(string key)
        {
            Key = key;
        }

        public string Key { get; }
    }
}

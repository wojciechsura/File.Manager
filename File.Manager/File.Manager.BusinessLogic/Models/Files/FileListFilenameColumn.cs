using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Models.Files
{
    public class FileListFilenameColumn : FileListColumn
    {
        public FileListFilenameColumn() 
        {
            // By default, name column is resized with window
            Width = 1;
            WidthKind = Types.FileListColumnWidthKind.Star;
        }
    }
}

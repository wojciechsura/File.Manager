using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace File.Manager.BusinessLogic.Models.Files
{
    public interface IFileListItem
    {
        string Name { get; }
        ImageSource SmallIcon { get; }
        ImageSource LargeIcon { get; }
        long? Size { get; }
        bool IsSelected { get; set; }
        object this[string key] { get; }
    }
}

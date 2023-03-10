using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace File.Manager.BusinessLogic.Models.Files
{
    public interface IFileListItem : INotifyPropertyChanged
    {
        string Name { get; }
        ImageSource SmallIcon { get; }
        ImageSource LargeIcon { get; }
        long? Size { get; }
        bool IsSelected { get; set; }
        bool IsSelectable { get; }
        object this[string key] { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.API.Filesystem.Models.Items
{
    public interface IFileInfo : IBaseItemInfo
    {
        long Size { get; }
        bool IsReadOnly { get; }
        bool IsHidden { get; }
        bool IsSystem { get; }
    }
}

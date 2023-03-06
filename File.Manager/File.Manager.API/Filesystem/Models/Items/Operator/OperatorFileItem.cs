using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.API.Filesystem.Models.Items.Operator
{
    public sealed class OperatorFileItem : BaseOperatorItem, IFileInfo
    {
        public OperatorFileItem(string name, long size, bool isReadOnly, bool isSystem, bool isHidden)
            : base(name)
        {
            Size = size;
            IsReadOnly = isReadOnly;
            IsSystem = isSystem;
            IsHidden = isHidden;
        }

        public long Size { get; }
        public bool IsReadOnly { get; }
        public bool IsSystem { get; }
        public bool IsHidden { get; }
    }
}

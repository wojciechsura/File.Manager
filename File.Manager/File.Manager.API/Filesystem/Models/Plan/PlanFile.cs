using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.API.Filesystem.Models.Plan
{
    public class PlanFile : BasePlanItem
    {
        public PlanFile(string name, long size, bool isReadOnly, bool isHidden, bool isSystem)
            : base(name)
        {
            Size = size;
            IsReadOnly = isReadOnly;
            IsHidden = isHidden;
            IsSystem = isSystem;
        }

        public long Size { get; }
        public bool IsReadOnly { get; }
        public bool IsHidden { get; }
        public bool IsSystem { get; }
    }
}

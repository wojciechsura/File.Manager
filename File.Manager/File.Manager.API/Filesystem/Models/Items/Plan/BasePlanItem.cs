using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.API.Filesystem.Models.Items.Plan
{
    public abstract class BasePlanItem : IBaseItemInfo
    {
        public BasePlanItem(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}

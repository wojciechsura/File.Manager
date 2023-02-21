using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.API.Filesystem.Models.Plan
{
    public abstract class BasePlanItem
    {
        public BasePlanItem(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}

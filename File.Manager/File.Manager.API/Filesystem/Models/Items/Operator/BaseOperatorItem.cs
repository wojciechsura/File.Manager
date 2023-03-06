using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.API.Filesystem.Models.Items.Operator
{
    public abstract class BaseOperatorItem : IBaseItemInfo
    {
        protected BaseOperatorItem(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}

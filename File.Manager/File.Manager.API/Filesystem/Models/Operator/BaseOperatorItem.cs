using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.API.Filesystem.Models.Operator
{
    public abstract class BaseOperatorItem
    {
        protected BaseOperatorItem(string name)
        {
            Name = name;
        }

        public string Name { get;  }
    }
}

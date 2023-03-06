using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.API.Filesystem.Models.Items.Operator
{
    public sealed class OperatorFolderItem : BaseOperatorItem, IFolderInfo
    {
        public OperatorFolderItem(string name)
            : base(name)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.API.Filesystem.Models.Items
{
    /// <summary>
    /// Represents a up-folder item. It should be added by all
    /// navigators as the first returned element.
    /// </summary>
    public sealed class UpFolderItem : Item
    {
        public UpFolderItem() :
            base(Resources.Strings.Item_UpFolder_Name)
        {

        }
    }
}

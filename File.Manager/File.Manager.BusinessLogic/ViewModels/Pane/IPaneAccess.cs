using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.ViewModels.Pane
{
    public interface IPaneAccess
    {
        void FocusItem(ItemViewModel selectedItem);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.ViewModels.Pane
{
    public class PaneViewModel
    {
        private readonly IPaneHandler handler;

        public PaneViewModel(IPaneHandler handler)
        {
            this.handler = handler;
        }


    }
}

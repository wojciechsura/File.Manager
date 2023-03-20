using Spooksoft.VisualStateManager.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace File.Manager.BusinessLogic.ViewModels.View.Display
{
    public abstract class BaseDisplayViewModel
    {
        private readonly string filename;

        protected readonly IDisplayHost host;

        protected BaseDisplayViewModel(IDisplayHost host, string filename)
        {
            this.host = host;
            this.filename = filename;
        }

        public string Filename => filename;
    }
}

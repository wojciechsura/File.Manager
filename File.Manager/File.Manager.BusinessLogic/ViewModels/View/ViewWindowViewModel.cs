using File.Manager.BusinessLogic.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.ViewModels.View
{
    public class ViewWindowViewModel : BaseViewModel
    {
        private readonly IViewWindowAccess access;

        public ViewWindowViewModel(IViewWindowAccess access, Stream stream, string filename)
        {
            this.access = access;
        }       
    }
}

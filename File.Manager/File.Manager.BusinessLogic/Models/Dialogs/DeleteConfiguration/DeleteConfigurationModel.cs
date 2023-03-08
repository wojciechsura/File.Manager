using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Models.Dialogs.DeleteConfiguration
{
    public class DeleteConfigurationModel
    {
        public DeleteConfigurationModel(string fileMask)
        {
            FileMask = fileMask;
        }

        public string FileMask { get; }
    }
}

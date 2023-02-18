using File.Manager.BusinessLogic.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Models.Dialogs.CopyMoveConfiguration
{
    public class CopyMoveConfigurationResultModel
    {
        public CopyMoveConfigurationResultModel(string fileMask, OverwritingOptions overwritingOptions)
        {
            FileMask = fileMask;
            OverwritingOptions = overwritingOptions;
        }

        public string FileMask { get; }
        public OverwritingOptions OverwritingOptions { get; }
    }
}

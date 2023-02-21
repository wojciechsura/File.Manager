using File.Manager.BusinessLogic.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Models.Dialogs.CopyMoveConfiguration
{
    public class CopyMoveConfigurationModel
    {
        public CopyMoveConfigurationModel(string fileMask, GenericProblemResolution overwritingOptions)
        {
            FileMask = fileMask;
            OverwritingResolution = overwritingOptions;
        }

        public string FileMask { get; }
        public GenericProblemResolution OverwritingResolution { get; }
    }
}

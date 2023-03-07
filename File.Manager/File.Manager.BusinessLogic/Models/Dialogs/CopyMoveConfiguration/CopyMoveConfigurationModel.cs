using File.Manager.BusinessLogic.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Models.Dialogs.CopyMoveConfiguration
{
    public class CopyMoveConfigurationModel
    {
        public CopyMoveConfigurationModel(string fileMask, 
            GenericCopyMoveProblemResolution overwritingOptions,
            bool renameFiles,
            bool renameRecursive,
            string renameFrom,
            string renameTo)
        {
            FileMask = fileMask;
            OverwritingResolution = overwritingOptions;
            RenameFiles = renameFiles;
            RenameRecursive = renameRecursive;
            RenameFrom = renameFiles ? new Regex(renameFrom) : null;
            RenameTo = renameFiles ? renameTo : null;
        }

        public string FileMask { get; }
        public GenericCopyMoveProblemResolution OverwritingResolution { get; }
        public bool RenameFiles { get; }
        public bool RenameRecursive { get; }
        public Regex RenameFrom { get; }
        public string RenameTo { get; }
    }
}

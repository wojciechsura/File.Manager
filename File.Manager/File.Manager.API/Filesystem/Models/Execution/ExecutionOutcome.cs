using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.API.Filesystem.Models.Execution
{
    /// <summary>
    /// Represents base class for different outcomes of item execution
    /// by the <see cref="FilesystemNavigator"/>s.
    /// </summary>
    public abstract class ExecutionOutcome
    {
        private protected ExecutionOutcome()
        {

        }
    }
}

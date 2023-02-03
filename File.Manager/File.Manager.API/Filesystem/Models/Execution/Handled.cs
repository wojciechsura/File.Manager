using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.API.Filesystem.Models.Execution
{
    /// <summary>
    /// When returned as execution outcome, informs File.Manager,
    /// that execution was fully handled by the <see cref="FilesystemNavigator"/>
    /// and there is no additional action needed.
    /// </summary>
    public sealed class Handled : ExecutionOutcome
    {
        internal Handled()
        {

        }
    }
}

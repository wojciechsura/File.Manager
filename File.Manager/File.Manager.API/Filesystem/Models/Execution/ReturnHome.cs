using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.API.Filesystem.Models.Execution
{
    /// <summary>
    /// When returned as execution outcome, informs File.Manager,
    /// that it should return to the home location (module selection).
    /// </summary>
    public sealed class ReturnHome : ExecutionOutcome
    {
        internal ReturnHome()
        {

        }
    }
}

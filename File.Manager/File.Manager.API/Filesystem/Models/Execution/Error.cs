using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.API.Filesystem.Models.Execution
{
    /// <summary>
    /// When returned as execution outcome, informs File.Manager,
    /// that execution failed. <see cref="Error"/> contains then
    /// error message, which may be displayed to the user.
    /// </summary>
    public sealed class Error : ExecutionOutcome
    {
        internal Error(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }
}

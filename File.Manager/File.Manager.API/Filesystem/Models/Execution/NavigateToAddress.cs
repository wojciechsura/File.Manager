using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.API.Filesystem.Models.Execution
{
    /// <summary>
    /// When returned as execution outcome, informs File.Manager,
    /// that it should navigate to specific given address.
    /// </summary>
    public sealed class NavigateToAddress : ExecutionOutcome
    {
        public NavigateToAddress(string address)
        {
            Address = address;
        }

        public string Address { get; }
    }
}

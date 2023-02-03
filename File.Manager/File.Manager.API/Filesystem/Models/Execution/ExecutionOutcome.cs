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

        public static Error Error(string message) => new Error(message);
        public static Handled Handled() => new Handled();
        public static NavigateToAddress NavigateToAddress(string address) => new NavigateToAddress(address);
        public static NeedsRefresh NeedsRefresh() => new NeedsRefresh();
        public static ReplaceNavigator ReplaceNavigator(FilesystemNavigator newNavigator) => new ReplaceNavigator(newNavigator);
        public static ReturnHome ReturnHome() => new ReturnHome();
    }
}

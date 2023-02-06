using File.Manager.API.Filesystem.Models.Focus;
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

        public static Error Error(string message) => new(message);
        public static Handled Handled() => new();
        public static NavigateToAddress NavigateToAddress(string address) => new(address);
        public static NeedsRefresh NeedsRefresh(FocusedItemData? data = null) => new(data);
        public static ReplaceNavigator ReplaceNavigator(FilesystemNavigator newNavigator, FocusedItemData? data = null) => new(newNavigator, data);
        public static ReturnHome ReturnHome(FocusedItemData? data = null) => new(data);
    }
}

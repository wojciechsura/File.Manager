using File.Manager.API.Filesystem.Models.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.API.Filesystem.Models.Execution
{
    /// <summary>
    /// When returned as execution outcome, informs File.Manager,
    /// that execution resulted in <see cref="FilesystemNavigator"/>
    /// contents change, so it should refresh contents of the pane.
    /// </summary>
    public sealed class NeedsRefresh : ExecutionOutcome
    {
        internal NeedsRefresh(SelectionMemento selection = null)
        {
            Selection = selection;
        }

        public SelectionMemento Selection { get; }
    }
}

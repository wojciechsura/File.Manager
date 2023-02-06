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
    /// that it should replace current navigator with given one.
    /// </summary>
    public sealed class ReplaceNavigator : ExecutionOutcome
    {
        internal ReplaceNavigator(FilesystemNavigator newNavigator, SelectionMemento selection = null)
        {
            NewNavigator = newNavigator;
            Selection = selection;
        }

        public FilesystemNavigator NewNavigator { get; }
        public SelectionMemento Selection { get; }
    }
}

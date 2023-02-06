using File.Manager.API.Filesystem.Models;
using File.Manager.API.Filesystem.Models.Execution;
using File.Manager.API.Filesystem.Models.Items;
using File.Manager.API.Filesystem.Models.Navigation;
using File.Manager.API.Filesystem.Models.Focus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace File.Manager.API.Filesystem
{
    public abstract class FilesystemNavigator : IDisposable
    {
        // Protected methods --------------------------------------------------

        protected FilesystemNavigator()
        {

        }

        // Public methods -----------------------------------------------------

        public abstract NavigationOutcome NavigateToRoot();

        public abstract NavigationOutcome NavigateToAddress(string address);

        public abstract ExecutionOutcome Execute(Item item);

        public abstract Item ResolveFocusedItem(FocusedItemData data);

        public abstract void Dispose();

        // Public properties --------------------------------------------------

        public abstract string Address { get; }
        
        public abstract IReadOnlyList<Item> Items { get; }
    }
}

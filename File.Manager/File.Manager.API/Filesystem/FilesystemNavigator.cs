using File.Manager.API.Filesystem.Models;
using File.Manager.API.Filesystem.Models.Navigation;
using File.Manager.API.Filesystem.Models.Focus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using File.Manager.API.Types;
using File.Manager.API.Filesystem.Models.Items.Listing;

namespace File.Manager.API.Filesystem
{
    public abstract class FilesystemNavigator : IDisposable
    {
        // Private fields -----------------------------------------------------

        private IFilesystemNavigatorHandler? handler;

        // Protected fields ---------------------------------------------------

        protected IFilesystemNavigatorHandler? Handler => handler;

        // Protected methods --------------------------------------------------

        protected FilesystemNavigator()
        {
            
        }

        // Public methods -----------------------------------------------------

        public abstract void NavigateToRoot();

        public abstract void NavigateToAddress(string address);

        public abstract void Execute(Item item);

        public abstract Item ResolveFocusedItem(FocusedItemData data);

        public abstract void Dispose();

        public abstract void Refresh();

        public abstract LocationCapabilities GetLocationCapabilities();

        public abstract IFilesystemOperator CreateOperatorForCurrentLocation();

        public void SetHandler(IFilesystemNavigatorHandler handler) 
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            if (this.handler != null)
                throw new InvalidOperationException("Handler for navigator may be set only once!");

            this.handler = handler;
        }

        // Public properties --------------------------------------------------

        public abstract string Address { get; }
        
        public abstract IReadOnlyList<Item> Items { get; }
    }
}

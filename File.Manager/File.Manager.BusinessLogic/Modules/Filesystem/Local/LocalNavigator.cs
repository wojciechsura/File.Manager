using File.Manager.API;
using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Execution;
using File.Manager.API.Filesystem.Models.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Modules.Filesystem.Local
{
    public class LocalNavigator : FilesystemNavigator
    {
        // Private constants --------------------------------------------------

        private const string ROOT_ADDRESS = @"\\Local\";

        // Private fields -----------------------------------------------------

        private List<Item> items;
        private string address;

        // Private methods ----------------------------------------------------

        private void Update(string newAddress)
        {
            if (newAddress == ROOT_ADDRESS)
            {
                // Drives
            }
            else
            {
                // Files
            }
        }

        // Public methods -----------------------------------------------------

        public LocalNavigator(IModuleHost host) : base(host)
        {
            address = @"\\Local\";
        }

        public LocalNavigator(IModuleHost host, string address) : base(host)
        {

        }

        public override void Dispose()
        {
            
        }

        public override ExecutionOutcome Execute(Item item)
        {
            throw new NotImplementedException();
        }

        // Public properties --------------------------------------------------

        public override IReadOnlyList<Item> Items => items;

        public override string Address => address;
    }
}

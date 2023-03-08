using File.Manager.API.Filesystem.Models.Items.Listing;
using File.Manager.BusinessLogic.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Models.Dialogs.DeleteConfiguration
{
    public class DeleteConfigurationInputModel
    {
        public DeleteConfigurationInputModel(string address, IReadOnlyList<Item> selectedItems)
        {
            Address = address;
            SelectedItems = selectedItems;
        }
        
        public IReadOnlyList<Item> SelectedItems { get; }
        public string Address { get; }
    }
}

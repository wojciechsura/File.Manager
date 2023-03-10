using File.Manager.API.Filesystem.Models.Items.Listing;
using File.Manager.BusinessLogic.Models.Files;
using File.Manager.BusinessLogic.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace File.Manager.BusinessLogic.ViewModels.Pane
{
    public class ItemViewModel : BaseViewModel, IFileListItem
    {
        private bool isSelected;

        private void SetSelected(bool value)
        {
            if (value && !Item.IsSelectable)
            {
                throw new InvalidOperationException("Item is not selectable!");
            }
            Set(ref isSelected, value);
        }

        private bool GetSelected() => isSelected;        

        public ItemViewModel(Item item)
        {
            Item = item;
            isSelected = false;
        }

        public string Name => Item.Name;
        
        public ImageSource SmallIcon => Item.SmallIcon;
        
        public ImageSource LargeIcon => Item.LargeIcon;
        
        public long? Size => Item.Size;
        
        public string SizeDisplay => Item.SizeDisplay;
        
        public DateTime? Created => Item.Created;
        
        public DateTime? Modified => Item.Modified;
        
        public string Attributes => Item.Attributes;

        public Item Item { get; }

        public bool IsSelected
        {
            get => GetSelected();
            set => SetSelected(value);
        }

        public bool IsSelectable => Item.IsSelectable;

        [System.Runtime.CompilerServices.IndexerName("ItemItems")]
        public object this[string key] => Item[key];
    }
}

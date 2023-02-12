using File.Manager.API.Filesystem.Models.Items;
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
            get => isSelected;
            set => Set(ref isSelected, value);
        }

        [System.Runtime.CompilerServices.IndexerName("ItemItems")]
        public object this[string key] => Item[key];
    }
}

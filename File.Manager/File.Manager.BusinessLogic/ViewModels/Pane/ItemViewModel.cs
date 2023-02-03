using File.Manager.API.Filesystem.Models.Items;
using File.Manager.BusinessLogic.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace File.Manager.BusinessLogic.ViewModels.Pane
{
    public class ItemViewModel : BaseViewModel
    {
        public ItemViewModel(string name, ImageSource smallIcon, ImageSource largeIcon, Item item)
        {
            Name = name;
            SmallIcon = smallIcon;
            LargeIcon = largeIcon;
            Item = item;
        }

        public string Name { get; }
        public ImageSource SmallIcon { get; }
        public ImageSource LargeIcon { get; }
        public Item Item { get; }
    }
}

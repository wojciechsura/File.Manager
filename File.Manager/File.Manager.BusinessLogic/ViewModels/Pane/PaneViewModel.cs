using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Items;
using File.Manager.BusinessLogic.Modules.Filesystem.Home;
using File.Manager.BusinessLogic.Services.Icons;
using File.Manager.BusinessLogic.Services.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace File.Manager.BusinessLogic.ViewModels.Pane
{
    public class PaneViewModel
    {
        private readonly IPaneHandler handler;
        private readonly IModuleService moduleService;
        private readonly IIconService iconService;
        private FilesystemNavigator navigator;
        private List<ItemViewModel> items;

        private void UpdateItems()
        {
            items.Clear();

            for (int i = 0; i < navigator.Items.Count; i++)
            {
                var item = navigator.Items[i];

                string name = item.Name;
                ImageSource smallIcon = item.SmallIcon;
                ImageSource largeIcon = item.LargeIcon;

                if (smallIcon == null || largeIcon == null)
                {
                    (ImageSource smallIcon, ImageSource largeIcon) icons;
                    if (item is FileItem)
                        icons = iconService.GetIconFor(item.Name);
                    else if (item is FolderItem or UpFolderItem)
                        icons = iconService.GetFolderIcon();
                    else
                        throw new InvalidOperationException("Unsupported filesystem item!");

                    smallIcon ??= icons.smallIcon;
                    largeIcon ??= icons.largeIcon;
                }

                var itemViewModel = new ItemViewModel(name, smallIcon, largeIcon, item);
                items.Add(itemViewModel);
            }
        }

        public PaneViewModel(IPaneHandler handler, IModuleService moduleService, IIconService iconService)
        {
            this.handler = handler;
            this.moduleService = moduleService;
            this.iconService = iconService;
            navigator = new HomeNavigator(moduleService);
            items = new List<ItemViewModel>();

            UpdateItems();
        }

        public IReadOnlyList<ItemViewModel> Items => items;
    }
}

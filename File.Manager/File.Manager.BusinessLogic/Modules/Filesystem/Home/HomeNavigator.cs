using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Navigation;
using File.Manager.API.Filesystem.Models.Focus;
using File.Manager.BusinessLogic.Services.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File.Manager.API.Exceptions.Filesystem;
using File.Manager.API.Types;
using File.Manager.API.Filesystem.Models.Items.Listing;

namespace File.Manager.BusinessLogic.Modules.Filesystem.Home
{
    public class HomeNavigator : FilesystemNavigator
    {
        private const string ROOT_ADDRESS = @"\";

        private readonly List<ModuleFolderItem> items;
        private readonly IModuleService moduleService;

        private string address;

        private class ModuleFolderItem : FolderItem
        {
            public ModuleFolderItem(FilesystemModule module)
                : base(module.DisplayName)
            {                
                SmallIcon = module.SmallIcon;
                LargeIcon = module.LargeIcon;
                Module = module;
            }

            public FilesystemModule Module { get; }
        }

        private void LoadItems()
        {
            if (address == ROOT_ADDRESS)
            {
                items.Clear();

                foreach (var module in moduleService.FilesystemModules)
                {
                    var folder = new ModuleFolderItem(module);
                    items.Add(folder);
                }
            }
            else 
                throw new InvalidOperationException("Home navigator only supports root address!");
        }

        public HomeNavigator(IModuleService moduleService)
        {
            this.moduleService = moduleService;
            address = ROOT_ADDRESS;
            items = new();
        }

        public override void Dispose()
        {
            
        }

        public override void Execute(Item item)
        {
            if (item is ModuleFolderItem moduleFolderItem)
            {
                FilesystemNavigator navigator = null;
                try
                {
                    navigator = moduleFolderItem.Module.CreateNavigator();
                    navigator.NavigateToRoot();
                    Handler?.RequestReplaceNavigator(navigator, null);
                }
                catch
                {
                    navigator?.Dispose();
                    throw;
                }
            }
            else
            {
                throw new InvalidOperationException("Unsupported file!");
            }
        }

        public override void NavigateToRoot()
        {
            address = ROOT_ADDRESS;
            LoadItems();                   
        }

        public override void NavigateToAddress(string address)
        {
            throw new InvalidOperationException("HomeNavigator does not support navigating to address!");
        }

        public override void Refresh()
        {
            LoadItems();
        }

        public override Item ResolveFocusedItem(FocusedItemData data)
        {
            var homeData = data as HomeFocusedItemData;
            if (homeData != null)
                return items.FirstOrDefault(i => i.Module.Uid == homeData.ModuleUid);

            return null;
        }

        public override LocationCapabilities GetLocationCapabilities()
        {
            return (LocationCapabilities)0;
        }

        public override IFilesystemOperator CreateOperatorForCurrentLocation()
            => throw new NotSupportedException("Creating operator for home module is not supported.");

        public override string Address => address;

        public override IReadOnlyList<Item> Items => items;
    }
}

using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Items;
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
using File.Manager.API.Filesystem.Models.Plan;

namespace File.Manager.BusinessLogic.Modules.Filesystem.Home
{
    public class HomeNavigator : FilesystemNavigator
    {
        private const string ROOT_ADDRESS = @"\";

        private readonly List<ModuleFolderItem> items;
        private readonly IModuleService moduleService;

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

        public HomeNavigator(IModuleService moduleService)
        {
            this.moduleService = moduleService;
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
            items.Clear();

            foreach (var module in moduleService.FilesystemModules)
            {
                var folder = new ModuleFolderItem(module);
                items.Add(folder);
            }            
        }

        public override void NavigateToAddress(string address)
        {
            throw new InvalidOperationException("HomeNavigator does not support navigating to address!");
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

        public override string Address => ROOT_ADDRESS;

        public override IReadOnlyList<Item> Items => items;
    }
}

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
            public ModuleFolderItem(FilesystemModule module, RootModuleEntry rootEntry)
                : base(rootEntry.DisplayName, false)
            {
                Module = module;

                SmallIcon = rootEntry.SmallIcon;
                LargeIcon = rootEntry.LargeIcon;
                Data = rootEntry.Data;
                Id = rootEntry.Id;
            }

            public FilesystemModule Module { get; }
            public RootModuleEntryData Data { get; }
            public int Id { get; }
        }

        private void LoadItems()
        {
            if (address.ToLowerInvariant() == ROOT_ADDRESS.ToLowerInvariant())
            {
                items.Clear();

                foreach (var module in moduleService.FilesystemModules)
                {
                    var rootEntries = module.GetRootEntries().ToList();

                    if (rootEntries
                        .Select(re => re.Id)
                        .Distinct()
                        .Count() != rootEntries.Count)
                        throw new InvalidOperationException("Root entries must have unique IDs per module!");

                    foreach (var entry in rootEntries)
                    {
                        var folder = new ModuleFolderItem(module, entry);
                        items.Add(folder);
                    }
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
                    navigator = moduleFolderItem.Module.CreateNavigator(moduleFolderItem.Data);
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

        public void NavigateToRoot()
        {
            address = ROOT_ADDRESS;
            OnAddressChanged();
            LoadItems();                   
        }

        public override void Refresh()
        {
            LoadItems();
        }

        public override Item ResolveFocusedItem(FocusedItemData data)
        {
            if (data is HomeFocusedItemData homeData && homeData.ModuleUid != null && homeData.RootEntryId != null)
                return items.FirstOrDefault(i => i.Module.Uid.ToLowerInvariant() == homeData.ModuleUid.ToLowerInvariant() && i.Id == homeData.RootEntryId);

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

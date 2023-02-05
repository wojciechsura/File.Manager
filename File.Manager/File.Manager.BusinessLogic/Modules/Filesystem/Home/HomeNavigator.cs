using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Execution;
using File.Manager.API.Filesystem.Models.Items;
using File.Manager.API.Filesystem.Models.Navigation;
using File.Manager.BusinessLogic.Services.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Modules.Filesystem.Home
{
    public class HomeNavigator : FilesystemNavigator
    {
        private const string ROOT_ADDRESS = @"\";

        private readonly List<Item> items;
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
            items = new List<Item>();
        }

        public override void Dispose()
        {
            
        }

        public override ExecutionOutcome Execute(Item item)
        {
            if (item is ModuleFolderItem moduleFolderItem)
            {
                var navigator = moduleFolderItem.Module.CreateNavigator();
                var outcome = navigator.NavigateToRoot();

                if (outcome is NavigationSuccess)
                    return ExecutionOutcome.ReplaceNavigator(navigator);
                else if (outcome is NavigationError error)
                    return ExecutionOutcome.Error(error.Message);
                else
                    throw new InvalidOperationException("Unsupported navigation outcome!");
            }
            else
            {
                throw new InvalidOperationException("Unsupported file!");
            }
        }

        public override NavigationOutcome NavigateToRoot()
        {
            items.Clear();

            foreach (var module in moduleService.FilesystemModules)
            {
                var folder = new ModuleFolderItem(module);
                items.Add(folder);
            }

            return NavigationOutcome.NavigationSuccess();
        }

        public override NavigationOutcome NavigateToAddress(string address)
        {
            throw new InvalidOperationException("HomeNavigator does not support navigating to address!");
        }

        public override string Address => ROOT_ADDRESS;

        public override IReadOnlyList<Item> Items => items;
    }
}

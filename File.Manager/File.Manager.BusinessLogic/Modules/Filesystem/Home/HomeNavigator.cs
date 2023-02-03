using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Execution;
using File.Manager.API.Filesystem.Models.Items;
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

        private class ModuleFileItem : FileItem
        {
            public ModuleFileItem(FilesystemModule module)
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
            items = new List<Item>();
            foreach (var module in moduleService.FilesystemModules)
            {
                var file = new ModuleFileItem(module);
                items.Add(file);
            }
        }

        public override void Dispose()
        {
            
        }

        public override ExecutionOutcome Execute(Item item)
        {
            if (item is ModuleFileItem moduleFileItem)
            {
                return ExecutionOutcome.ReplaceNavigator(moduleFileItem.Module.OpenRoot());
            }
            else
            {
                throw new InvalidOperationException("Unsupported file!");
            }
        }

        public override string Address => ROOT_ADDRESS;

        public override IReadOnlyList<Item> Items => items;
    }
}

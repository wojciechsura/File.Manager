using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Items;
using File.Manager.API.Filesystem.Models.Navigation;
using File.Manager.API.Filesystem.Models.Focus;
using File.Manager.BusinessLogic.Modules.Filesystem.Home;
using File.Manager.BusinessLogic.Services.Icons;
using File.Manager.BusinessLogic.Services.Messaging;
using File.Manager.BusinessLogic.Services.Modules;
using File.Manager.BusinessLogic.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using File.Manager.API.Exceptions.Filesystem;

namespace File.Manager.BusinessLogic.ViewModels.Pane
{
    public class PaneViewModel : BaseViewModel, IFilesystemNavigatorHandler
    {
        // Private fields -----------------------------------------------------

        private readonly IPaneHandler handler;
        private readonly IModuleService moduleService;
        private readonly IIconService iconService;
        private readonly IMessagingService messagingService;

        private FilesystemNavigator navigator;
        private readonly ObservableCollection<ItemViewModel> items;
        private readonly List<IPaneAccess> accesses;
        private ItemViewModel selectedItem;

        // Private methods ----------------------------------------------------

        private void SelectAndFocus(ItemViewModel itemViewModel)
        {
            SelectedItem = itemViewModel;
            accesses.ForEach(access => access.FocusItem(itemViewModel));
        }

        private void UpdateItems(FocusedItemData data)
        {
            items.Clear();

            Item newSelectedItem = navigator.ResolveFocusedItem(data);
            ItemViewModel newSelectedItemViewModel = null;

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

                    item.SmallIcon ??= icons.smallIcon;
                    item.LargeIcon ??= icons.largeIcon;
                }

                var itemViewModel = new ItemViewModel(item);
                items.Add(itemViewModel);

                if (item != null && item == newSelectedItem)
                    newSelectedItemViewModel = itemViewModel;
            }

            if (newSelectedItemViewModel != null)
                SelectAndFocus(newSelectedItemViewModel);
            else
                SelectAndFocus(items.FirstOrDefault());
        }

        private void ReplaceCurrentNavigator(FilesystemNavigator newNavigator, FocusedItemData data)
        {
            navigator?.Dispose();

            navigator = newNavigator;

            navigator?.SetHandler(this);

            UpdateItems(data);

            OnPropertyChanged(nameof(Navigator));
        }

        private void SetHomeNavigator(FocusedItemData data)
        {
            var homeNavigator = new HomeNavigator(moduleService);            
            homeNavigator.NavigateToRoot();

            ReplaceCurrentNavigator(homeNavigator, data);
        }

        // IFilesystemNavigatorHandler implementation -------------------------

        void IFilesystemNavigatorHandler.NotifyChanged(FocusedItemData focusedItem)
        {
            UpdateItems(focusedItem);
        }

        void IFilesystemNavigatorHandler.RequestNavigateToAddress(string address)
        {
            int i = 0;
            while (i < moduleService.FilesystemModules.Count &&
                !moduleService.FilesystemModules[i].SupportsAddress(address))
                i++;

            if (i < moduleService.FilesystemModules.Count)
            {
                var module = moduleService.FilesystemModules[i];

                var newNavigator = module.CreateNavigator();

                try
                {
                    newNavigator.NavigateToAddress(address);
                    ReplaceCurrentNavigator(newNavigator, null);
                }
                catch (NavigationException e)
                {
                    messagingService.ShowError(String.Format(Resources.Controls.Pane.Strings.Message_AddressNavigationFailed, address, e));
                }
            }
            else
            {
                messagingService.ShowError(String.Format(Resources.Controls.Pane.Strings.Message_AddressUnsupported, address));
            }
        }

        void IFilesystemNavigatorHandler.RequestReplaceNavigator(FilesystemNavigator newNavigator, FocusedItemData focusedItem)
        {
            ReplaceCurrentNavigator(newNavigator, focusedItem);
        }

        void IFilesystemNavigatorHandler.RequestReturnHome(FocusedItemData focusedItem)
        {
            SetHomeNavigator(focusedItem);
        }

        // Public methods -----------------------------------------------------

        public PaneViewModel(IPaneHandler handler, IModuleService moduleService, IIconService iconService, IMessagingService messagingService)
        {
            this.handler = handler;
            this.moduleService = moduleService;
            this.iconService = iconService;
            this.messagingService = messagingService;
            this.accesses = new();

            items = new();
            
            SetHomeNavigator(null);
            UpdateItems(null);
        }

        public void ExecuteCurrentItem()
        {
            if (selectedItem != null)
            {
                try
                {
                    navigator.Execute(selectedItem.Item);
                }
                catch (ItemExecutionException e)
                {
                    messagingService.ShowError(String.Format(Resources.Controls.Pane.Strings.Message_CannotExecuteItem, e.Message));
                }
            }
        }

        public void AddAccess(IPaneAccess access)
        {
            if (access == null)
                throw new ArgumentNullException(nameof(access));

            accesses.Add(access);
        }

        public void RemoveAccess(IPaneAccess access)
        {
            if (access == null)
                throw new ArgumentNullException(nameof(access));

            accesses.Remove(access);
        }

        public void NotifyGotFocus()
        {
            handler.NotifyPaneFocused(this);
        }

        public void NotifyLostFocus()
        {
            handler.NotifyPaneUnfocused(this);
        }

        // Public properties --------------------------------------------------

        public ObservableCollection<ItemViewModel> Items => items;

        public ItemViewModel SelectedItem
        {
            get => selectedItem;
            set => Set(ref selectedItem, value);
        }

        public FilesystemNavigator Navigator => navigator;
    }
}

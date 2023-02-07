using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Execution;
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

namespace File.Manager.BusinessLogic.ViewModels.Pane
{
    public class PaneViewModel : BaseViewModel
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
            if (navigator != null)
                navigator.Dispose();

            navigator = newNavigator;
            UpdateItems(data);

            OnPropertyChanged(nameof(NavigatorCapabilities));
        }

        private void SetHomeNavigator(FocusedItemData data)
        {
            var homeNavigator = new HomeNavigator(moduleService);
            homeNavigator.NavigateToRoot();

            ReplaceCurrentNavigator(homeNavigator, data);
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
                var outcome = navigator.Execute(selectedItem.Item);

                switch (outcome)
                {
                    case Error error:
                        {
                            messagingService.ShowError(String.Format(Resources.Controls.Pane.Strings.Message_CannotExecuteItem, error.Message));
                            break;
                        }
                    case Handled handled:
                        {
                            // Handled means, that Navigator handled the whole
                            // execution on its own (e.g. started external process)
                            // Nothing else needs to be done here
                            break;
                        }
                    case NavigateToAddress navigateToAddress:
                        {
                            int i = 0;
                            while (i < moduleService.FilesystemModules.Count &&
                                !moduleService.FilesystemModules[i].SupportsAddress(navigateToAddress.Address))
                                i++;

                            if (i < moduleService.FilesystemModules.Count)
                            {
                                var module = moduleService.FilesystemModules[i];

                                var newNavigator = module.CreateNavigator();
                                var navigationOutcome = newNavigator.NavigateToAddress(navigateToAddress.Address);

                                if (navigationOutcome is NavigationSuccess)
                                {
                                    ReplaceCurrentNavigator(newNavigator, null);
                                }
                                else
                                {
                                    messagingService.ShowError(String.Format(Resources.Controls.Pane.Strings.Message_AddressNavigationFailed, navigateToAddress.Address));
                                }
                            }
                            else
                            {
                                messagingService.ShowError(String.Format(Resources.Controls.Pane.Strings.Message_AddressUnsupported, navigateToAddress.Address));
                            }

                            break;
                        }
                    case NeedsRefresh needsRefresh:
                        {
                            UpdateItems(needsRefresh.Data);
                            break;
                        }
                    case ReplaceNavigator replaceNavigator:
                        {
                            ReplaceCurrentNavigator(replaceNavigator.NewNavigator, replaceNavigator.Data);
                            break;
                        }
                    case ReturnHome returnHome:
                        {
                            SetHomeNavigator(returnHome.Data);
                            break;
                        }
                    default:
                        throw new InvalidOperationException("Unsupported execution outcome!");
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

        public IFilesystemNavigatorCapabilities NavigatorCapabilities => navigator;
    }
}

﻿using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Execution;
using File.Manager.API.Filesystem.Models.Items;
using File.Manager.API.Filesystem.Models.Navigation;
using File.Manager.API.Filesystem.Models.Selection;
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
        private readonly IPaneHandler handler;
        private readonly IModuleService moduleService;
        private readonly IIconService iconService;
        private readonly IMessagingService messagingService;

        private FilesystemNavigator navigator;
        private readonly ObservableCollection<ItemViewModel> items;
        private ItemViewModel selectedItem;

        private void SelectAndFocus(ItemViewModel itemViewModel)
        {
            SelectedItem = itemViewModel;
            Access?.FocusItem(itemViewModel);
        }

        private void UpdateItems(SelectionMemento selection)
        {
            items.Clear();

            Item newSelectedItem = navigator.ResolveSelectedItem(selection);
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

                    smallIcon ??= icons.smallIcon;
                    largeIcon ??= icons.largeIcon;
                }

                var itemViewModel = new ItemViewModel(name, smallIcon, largeIcon, item);
                items.Add(itemViewModel);

                if (item != null && item == newSelectedItem)
                    newSelectedItemViewModel = itemViewModel;
            }

            if (newSelectedItemViewModel != null)
                SelectAndFocus(newSelectedItemViewModel);
            else
                SelectAndFocus(items.FirstOrDefault());
        }

        private void ReplaceCurrentNavigator(FilesystemNavigator newNavigator, SelectionMemento selection)
        {
            if (navigator != null)
                navigator.Dispose();

            navigator = newNavigator;

            UpdateItems(selection);
        }

        private void SetHomeNavigator()
        {
            var homeNavigator = new HomeNavigator(moduleService);
            homeNavigator.NavigateToRoot();

            ReplaceCurrentNavigator(homeNavigator);
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
                                    ReplaceCurrentNavigator(newNavigator);
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
                            UpdateItems(needsRefresh.Selection);
                            break;
                        }
                    case ReplaceNavigator replaceNavigator:
                        {
                            ReplaceCurrentNavigator(replaceNavigator.NewNavigator, replaceNavigator.Selection);
                            break;
                        }
                    case ReturnHome:
                        {
                            SetHomeNavigator();
                            break;
                        }
                    default:
                        throw new InvalidOperationException("Unsupported execution outcome!");
                }
            }
        }

        public PaneViewModel(IPaneHandler handler, IModuleService moduleService, IIconService iconService, IMessagingService messagingService)
        {
            this.handler = handler;
            this.moduleService = moduleService;
            this.iconService = iconService;
            this.messagingService = messagingService;

            items = new();
            
            SetHomeNavigator();
            UpdateItems();
        }

        public ObservableCollection<ItemViewModel> Items => items;

        public IPaneAccess Access { get; set; }

        public ItemViewModel SelectedItem
        {
            get => selectedItem;
            set => Set(ref selectedItem, value);
        }
    }
}

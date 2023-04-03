using File.Manager.API.Filesystem;
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
using System.Windows.Input;
using Spooksoft.VisualStateManager.Commands;
using System.Windows.Data;
using System.ComponentModel;
using File.Manager.BusinessLogic.Models.Files;
using File.Manager.API.Filesystem.Models.Items.Listing;
using File.Manager.Common.Wpf.Collections;
using Spooksoft.Configuration;

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
        private readonly ObservableList<ItemViewModel> items;
        private readonly ListCollectionView collectionView;

        private IPaneAccess access;
        private bool active;

        private bool quickSearchVisible;
        private string quickSearchText;

        // Private methods ----------------------------------------------------

        private void UpdateItems(FocusedItemData data)
        {
            items.Clear();

            var newItems = new List<ItemViewModel>();

            Item newSelectedItem = null;
            if (data != null)
                newSelectedItem = navigator.ResolveFocusedItem(data);

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
                newItems.Add(itemViewModel);

                if (item != null && item == newSelectedItem)
                    newSelectedItemViewModel = itemViewModel;
            }

            foreach (var item in newItems)
                items.Add(item);

            //items.AddRange(newItems);

            if (newSelectedItemViewModel != null)
                collectionView.MoveCurrentTo(newSelectedItemViewModel);
            else
                collectionView.MoveCurrentToFirst();
        }

        private void ReplaceCurrentNavigator(FilesystemNavigator newNavigator, FocusedItemData data)
        {
            if (navigator != null)
            {
                navigator.AddressChanged -= HandleNavigatorAddressChanged;
                navigator.Dispose();
            }

            navigator = newNavigator;

            if (navigator != null)
            {
                navigator.SetHandler(this);
                navigator.AddressChanged += HandleNavigatorAddressChanged;
            }            

            UpdateItems(data);

            OnPropertyChanged(nameof(Navigator));
            OnPropertyChanged(nameof(Address));
        }

        private void HandleNavigatorAddressChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(Address));
        }

        private void SetHomeNavigator(FocusedItemData data)
        {
            var homeNavigator = new HomeNavigator(moduleService);            
            homeNavigator.NavigateToRoot();

            ReplaceCurrentNavigator(homeNavigator, data);
        }

        private void DoExecuteCurrentItem(object item)
        {
            if (item is not ItemViewModel itemViewModel)
                throw new InvalidOperationException("Invalid item!");

            try
            {
                navigator.Execute(itemViewModel.Item);
            }
            catch (ItemExecutionException e)
            {
                messagingService.ShowError(String.Format(Resources.Controls.Pane.Strings.Message_CannotExecuteItem, e.Message));
            }
        }

        private void DoNavigateToAddress(string address, FocusedItemData focusedItem)
        {
            // Special case, as there is no home module
            if (address == HomeNavigator.ROOT_ADDRESS)
            {
                SetHomeNavigator(null);
                return;
            }

            var module = moduleService.FilesystemModules.FirstOrDefault(s => s.SupportsAddress(address));

            if (module != null)
            {
                var newNavigator = module.CreateNavigator(address);

                try
                {
                    ReplaceCurrentNavigator(newNavigator, focusedItem);
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

        private void PerformQuickSearch(string quickSearchText)
        {
            int i = 0;

            var searchText = quickSearchText.ToLowerInvariant();

            // Pass 1. Search for item, which starts with given string

            while (i < items.Count && !items[i].Name.ToLowerInvariant().StartsWith(searchText))
                i++;

            if (i >= items.Count)
            {
                i = 0;

                // Pass 2. Search for item, which contains given string

                while (i < items.Count && !items[i].Name.ToLowerInvariant().Contains(searchText))
                    i++;
            }

            if (i >= items.Count)
            {
                // Fallback

                i = 0;
            }

            collectionView.MoveCurrentTo(items[i]);
        }

        private void HandleQuickSearchTextChanged()
        {
            if (QuickSearchVisible)
            {
                PerformQuickSearch(QuickSearchText);
            }
        }

        // IFilesystemNavigatorHandler implementation -------------------------

        void IFilesystemNavigatorHandler.NotifyChanged(FocusedItemData focusedItem)
        {            
            UpdateItems(focusedItem);
        }

        void IFilesystemNavigatorHandler.RequestNavigateToAddress(string address, FocusedItemData? focusedItem)
        {
            DoNavigateToAddress(address, focusedItem);
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

            items = new();
            collectionView = new(items);
            quickSearchVisible = false;

            ExecuteCurrentItemCommand = new AppCommand(DoExecuteCurrentItem);
            HideQuickSearchCommand = new AppCommand(obj => HideQuickSearch());

            SetHomeNavigator(null);
            UpdateItems(null);
        }
       
        public void NotifyTabPressed()
        {
            handler.RequestSwithPane();
        }

        public List<Item> GetSelectedItems()
        {
            var selectedItems = items
                .Where(i => i.IsSelected)
                .Select(i => i.Item)
                .ToList();

            // If user selected no items, choose focused one
            // as selected
            if (!selectedItems.Any() && collectionView.CurrentItem != null)
            {
                selectedItems.Add(((ItemViewModel)collectionView.CurrentItem).Item);
            }

            return selectedItems;
        }

        public Item GetFocusedItem()
        {
            return ((ItemViewModel)collectionView.CurrentItem).Item;
        }

        public void Refresh()
        {
            navigator.Refresh();
        }

        public void SelectItem(string name)
        {
            for (int i = 0; i < items.Count; i++) 
            { 
                if (items[i].Name == name)
                {
                    ItemsView.MoveCurrentTo(items[i]);
                    break;
                }
            }

            ItemsView.MoveCurrentToFirst();
        }

        public void Navigate(string address)
        {
            DoNavigateToAddress(address, null);
        }

        public void ToggleQuickSearch()
        {
            if (!QuickSearchVisible)
            {
                QuickSearchText = "";
                QuickSearchVisible = true;
            }
            else
            {
                QuickSearchVisible = false;
                QuickSearchText = "";
            }
        }

        public void HideQuickSearch()
        {
            QuickSearchVisible = false;
        }

        // Public properties --------------------------------------------------

        public IEnumerable<ItemViewModel> Items => items;

        public ICollectionView ItemsView => collectionView;

        public FilesystemNavigator Navigator => navigator;

        public string Address => Navigator?.Address ?? string.Empty;

        public IPaneAccess Access
        {
            get => access;
            set
            {
                if (access != null && value != null)
                    throw new InvalidOperationException("Only one access can be set at a time!");

                access = value;
            }            
        }
        
        public bool Active 
        {  
            get => active; 
            set => Set(ref active, value);
        }

        public ICommand ExecuteCurrentItemCommand { get; }

        public ICommand HideQuickSearchCommand { get; }

        public bool QuickSearchVisible
        {
            get => quickSearchVisible;
            set => Set(ref quickSearchVisible, value);
        }

        public string QuickSearchText
        {
            get => quickSearchText;
            set => Set(ref quickSearchText, value, changeHandler: HandleQuickSearchTextChanged);
        }
    }
}

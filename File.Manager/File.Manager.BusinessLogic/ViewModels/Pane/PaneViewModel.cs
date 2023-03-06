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
using Gstc.Collections.ObservableLists;
using File.Manager.API.Filesystem.Models.Items.Listing;

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
        private readonly CollectionView collectionView;

        private IPaneAccess access;
        private bool active;

        // Private methods ----------------------------------------------------

        private void UpdateItems(FocusedItemData data)
        {
            items.Clear();

            var newItems = new List<ItemViewModel>();

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
                newItems.Add(itemViewModel);

                if (item != null && item == newSelectedItem)
                    newSelectedItemViewModel = itemViewModel;
            }

            items.AddRange(newItems);

            if (newSelectedItemViewModel != null)
                collectionView.MoveCurrentTo(newSelectedItemViewModel);
            else
                collectionView.MoveCurrentToFirst();
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

        public void DoExecuteCurrentItem(object item)
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

            items = new();
            items.IsAddRangeResetEvent = true;
            collectionView = new CollectionView(items);            

            ExecuteCurrentItemCommand = new AppCommand(DoExecuteCurrentItem);

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

        // Public properties --------------------------------------------------

        public IEnumerable<ItemViewModel> Items => items;

        public ICollectionView ItemsView => collectionView;

        public FilesystemNavigator Navigator => navigator;

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
    }
}

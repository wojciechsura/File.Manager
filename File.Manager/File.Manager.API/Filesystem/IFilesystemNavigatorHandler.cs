using File.Manager.API.Filesystem.Models.Focus;

namespace File.Manager.API.Filesystem
{
    public interface IFilesystemNavigatorHandler
    {
        void NotifyChanged(FocusedItemData? focusedItem);
        void RequestNavigateToAddress(string address);
        void RequestReplaceNavigator(FilesystemNavigator newNavigator, FocusedItemData? focusedItem);
        void RequestReturnHome(FocusedItemData? focusedItem);
    }
}
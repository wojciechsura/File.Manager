using System.Windows.Media;

namespace File.Manager.BusinessLogic.Services.Icons
{
    public interface IIconService
    {
        (ImageSource smallIcon, ImageSource largeIcon) GetFolderIcon();
        (ImageSource smallIcon, ImageSource largeIcon) GetIconFor(string filename);
    }
}
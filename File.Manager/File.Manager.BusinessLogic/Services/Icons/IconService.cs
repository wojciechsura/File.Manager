using File.Manager.OsInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace File.Manager.BusinessLogic.Services.Icons
{
    internal class IconService : IIconService
    {
        private readonly Dictionary<string, (ImageSource smallImage, ImageSource largeImage)> iconCache = new();
        private (ImageSource smallImage, ImageSource largeImage)? folderImage;

        public (ImageSource smallIcon, ImageSource largeIcon) GetFolderIcon()
        {
            folderImage ??= OSServices.GetFolderIcon();
            return folderImage.Value;
        }

        public (ImageSource smallIcon, ImageSource largeIcon) GetIconFor(string filename)
        {
            var extension = System.IO.Path.GetExtension(filename).ToLowerInvariant();
            if (iconCache.TryGetValue(extension, out (ImageSource smallIcon, ImageSource largeIcon) icons))
                return icons;

            icons = OSServices.GetFileIcon(extension);
            iconCache.Add(extension, icons);

            return icons;
        }
    }
}

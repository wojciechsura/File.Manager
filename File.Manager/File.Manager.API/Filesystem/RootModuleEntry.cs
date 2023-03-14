using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace File.Manager.API.Filesystem
{
    public sealed class RootModuleEntry
    {
        public RootModuleEntry(int id, 
            string displayName, 
            ImageSource smallIcon, 
            ImageSource largeIcon, 
            object data)
        {
            Id = id;
            DisplayName = displayName;
            SmallIcon = smallIcon;
            LargeIcon = largeIcon;
            Data = data;
        }

        /// <summary>ID of the entry. Must be unique among other entries from the same module.</summary>
        public int Id { get; }

        /// <summary>Localized name shown to the user.</summary>
        public string DisplayName { get; }

        /// <summary>Small icon of the entry (16x16).</summary>
        public ImageSource SmallIcon { get; }

        /// <summary>Large icon of the entry (32x32).</summary>
        public ImageSource LargeIcon { get; }

        /// <summary>Any arbitrary data, which will be passed to the navigator when user
        /// chooses to open this entry.</summary>        
        public object Data { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Navigation;

namespace File.Manager.API.Filesystem.Models.Items.Listing
{
    /// <summary>
    /// Base class for items provided to File.Manager by the Navigator
    /// </summary>
    public abstract class Item
    {
        public const string NameKey = "Name";
        public const string SmallIconKey = "SmallIcon";
        public const string LargeIconKey = "LargeIcon";
        public const string SizeKey = "Size";
        public const string SizeDisplayKey = "SizeDisplay";
        public const string CreatedKey = "Created";
        public const string ModifiedKey = "Modified";
        public const string AttributesKey = "Attributes";

        private readonly Dictionary<string, object?> attributes;

        private T TryGet<T>(string key)
        {
            return attributes.ContainsKey(key) ? (T)attributes[key]! : default!;
        }

        private void Set(string key, object? value)
        {
            switch (key)
            {
                case NameKey:
                    attributes[NameKey] = (string)value!;
                    break;
                case SmallIconKey:
                    attributes[SmallIconKey] = (ImageSource?)value;
                    break;
                case LargeIconKey:
                    attributes[LargeIconKey] = (ImageSource?)value;
                    break;
                case SizeKey:
                    attributes[SizeKey] = (long?)value;
                    break;
                case SizeDisplayKey:
                    attributes[SizeDisplayKey] = (string?)value;
                    break;
                case CreatedKey:
                    attributes[CreatedKey] = (DateTime?)value;
                    break;
                case ModifiedKey:
                    attributes[ModifiedKey] = (DateTime?)value;
                    break;
                case AttributesKey:
                    attributes[AttributesKey] = (string?)value;
                    break;
                default:
                    attributes[key] = value;
                    break;
            }
        }

        private protected Item(string name)
        {
            attributes = new();
            attributes[NameKey] = name;
        }

        public string Name => TryGet<string>(NameKey)!;

        public ImageSource? SmallIcon
        {
            get => TryGet<ImageSource?>(SmallIconKey);
            set => Set(SmallIconKey, value);
        }

        public ImageSource? LargeIcon
        {
            get => TryGet<ImageSource?>(LargeIconKey);
            set => Set(LargeIconKey, value);
        }

        public long? Size
        {
            get => TryGet<long?>(SizeKey);
            set => Set(SizeKey, value);
        }

        public string? SizeDisplay
        {
            get => TryGet<string?>(SizeDisplayKey);
            set => Set(SizeDisplayKey, value);
        }

        public DateTime? Created
        {
            get => TryGet<DateTime?>(CreatedKey);
            set => Set(CreatedKey, value);
        }

        public DateTime? Modified
        {
            get => TryGet<DateTime?>(ModifiedKey);
            set => Set(ModifiedKey, value);
        }

        public string? Attributes
        {
            get => TryGet<string?>(AttributesKey);
            set => Set(AttributesKey, value);
        }

        public object? this[string key]
        {
            get => attributes.ContainsKey(key) ? attributes[key] : null;
            set
            {
                if (key == NameKey)
                    throw new InvalidOperationException("Name of the item is read-only!");

                Set(key, value);
            }
        }
    }
}

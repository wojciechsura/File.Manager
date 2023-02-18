using BindingEnums;
using File.Manager.BusinessLogic.TypeConverters;
using File.Manager.Resources.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Types
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum OverwritingOptions
    {
        [LocalizedDescription(nameof(Strings.OverwritingOptions_Ask), typeof(Strings))]
        Ask,
        [LocalizedDescription(nameof(Strings.OverwritingOptions_Overwrite), typeof(Strings))]
        Overwrite,
        [LocalizedDescription(nameof(Strings.OverwritingOptions_Skip), typeof(Strings))]
        Skip,
        [LocalizedDescription(nameof(Strings.OverwritingOptions_Rename), typeof(Strings))]
        Rename
    }
}

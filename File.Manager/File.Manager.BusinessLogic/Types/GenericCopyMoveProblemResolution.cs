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
    public enum GenericCopyMoveProblemResolution
    {
        [LocalizedDescription(nameof(Strings.GenericCopyMoveProblemResolution_Ask), typeof(Strings))]
        Ask,
        [LocalizedDescription(nameof(Strings.GenericCopyMoveProblemResolution_Overwrite), typeof(Strings))]
        Overwrite,
        [LocalizedDescription(nameof(Strings.GenericCopyMoveProblemResolution_Skip), typeof(Strings))]
        Skip,
        [LocalizedDescription(nameof(Strings.GenericCopyMoveProblemResolution_Rename), typeof(Strings))]
        Rename,
        [LocalizedDescription(nameof(Strings.GenericCopyMoveProblemResolution_Ignore), typeof(Strings))]
        Ignore,
        [LocalizedDescription(nameof(Strings.GenericCopyMoveProblemResolution_Abort), typeof(Strings))]
        Abort
    }
}

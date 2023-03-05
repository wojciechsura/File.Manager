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
    public enum GenericProblemResolution
    {
        [LocalizedDescription(nameof(Strings.GenericProblemResolution_Ask), typeof(Strings))]
        Ask,
        [LocalizedDescription(nameof(Strings.GenericProblemResolution_Overwrite), typeof(Strings))]
        Overwrite,
        [LocalizedDescription(nameof(Strings.GenericProblemResolution_Skip), typeof(Strings))]
        Skip,
        [LocalizedDescription(nameof(Strings.GenericProblemResolution_Rename), typeof(Strings))]
        Rename,
        [LocalizedDescription(nameof(Strings.GenericProblemResolution_Rename), typeof(Strings))]
        Ignore,
        [LocalizedDescription(nameof(Strings.GenericProblemResolution_Abort), typeof(Strings))]
        Abort
    }
}

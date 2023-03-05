using BindingEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File.Manager.Resources.Enums;
using File.Manager.BusinessLogic.TypeConverters;
using System.ComponentModel;

namespace File.Manager.BusinessLogic.Types
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum SingleProblemResolution
    {
        [LocalizedDescription(nameof(Strings.SingleProblemResolution_Skip), typeof(Strings))]
        Skip = 1,
        [LocalizedDescription(nameof(Strings.SingleProblemResolution_SkipAll), typeof(Strings))]
        SkipAll,
        [LocalizedDescription(nameof(Strings.SingleProblemResolution_Overwrite), typeof(Strings))]
        Overwrite,
        [LocalizedDescription(nameof(Strings.SingleProblemResolution_OverwriteAll), typeof(Strings))]
        OverwriteAll,
        [LocalizedDescription(nameof(Strings.SingleProblemResolution_Rename), typeof(Strings))]
        Rename,
        [LocalizedDescription(nameof(Strings.SingleProblemResolution_RenameAll), typeof(Strings))]
        RenameAll,
        [LocalizedDescription(nameof(Strings.SingleProblemResolution_Ignore), typeof(Strings))]
        Ignore,
        [LocalizedDescription(nameof(Strings.SingleProblemResolution_IgnoreAll), typeof(Strings))]
        IgnoreAll,
        [LocalizedDescription(nameof(Strings.SingleProblemResolution_Abort), typeof(Strings))]
        Abort
    }
}

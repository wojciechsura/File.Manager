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
    public enum SingleCopyMoveProblemResolution
    {
        [LocalizedDescription(nameof(Strings.SingleCopyMoveProblemResolution_Skip), typeof(Strings))]
        Skip = 1,
        [LocalizedDescription(nameof(Strings.SingleCopyMoveProblemResolution_SkipAll), typeof(Strings))]
        SkipAll,
        [LocalizedDescription(nameof(Strings.SingleCopyMoveProblemResolution_Overwrite), typeof(Strings))]
        Overwrite,
        [LocalizedDescription(nameof(Strings.SingleCopyMoveProblemResolution_OverwriteAll), typeof(Strings))]
        OverwriteAll,
        [LocalizedDescription(nameof(Strings.SingleCopyMoveProblemResolution_Rename), typeof(Strings))]
        Rename,
        [LocalizedDescription(nameof(Strings.SingleCopyMoveProblemResolution_RenameAll), typeof(Strings))]
        RenameAll,
        [LocalizedDescription(nameof(Strings.SingleCopyMoveProblemResolution_Ignore), typeof(Strings))]
        Ignore,
        [LocalizedDescription(nameof(Strings.SingleCopyMoveProblemResolution_IgnoreAll), typeof(Strings))]
        IgnoreAll,
        [LocalizedDescription(nameof(Strings.SingleCopyMoveProblemResolution_Abort), typeof(Strings))]
        Abort
    }
}

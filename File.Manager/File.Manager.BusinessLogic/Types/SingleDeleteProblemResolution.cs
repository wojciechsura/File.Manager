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
    public enum SingleDeleteProblemResolution
    {
        [LocalizedDescription(nameof(Strings.SingleDeleteProblemResolution_Skip), typeof(Strings))]
        Skip = 1,
        [LocalizedDescription(nameof(Strings.SingleDeleteProblemResolution_SkipAll), typeof(Strings))]
        SkipAll,
        [LocalizedDescription(nameof(Strings.SingleDeleteProblemResolution_Delete), typeof(Strings))]
        Delete,
        [LocalizedDescription(nameof(Strings.SingleDeleteProblemResolution_DeleteAll), typeof(Strings))]
        DeleteAll,
        [LocalizedDescription(nameof(Strings.SingleDeleteProblemResolution_Abort), typeof(Strings))]
        Abort
    }
}

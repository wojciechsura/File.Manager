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
    public enum GenericDeleteProblemResolution
    {
        [LocalizedDescription(nameof(Strings.GenericDeleteProblemResolution_Ask), typeof(Strings))]
        Ask,
        [LocalizedDescription(nameof(Strings.GenericDeleteProblemResolution_Delete), typeof(Strings))]
        Delete,
        [LocalizedDescription(nameof(Strings.GenericDeleteProblemResolution_Skip), typeof(Strings))]
        Skip,
        [LocalizedDescription(nameof(Strings.GenericDeleteProblemResolution_Abort), typeof(Strings))]
        Abort
    }
}

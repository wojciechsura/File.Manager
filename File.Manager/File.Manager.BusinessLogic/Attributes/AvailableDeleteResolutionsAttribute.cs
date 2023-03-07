using File.Manager.BusinessLogic.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class AvailableDeleteResolutionsAttribute : Attribute
    {
        public AvailableDeleteResolutionsAttribute(params SingleDeleteProblemResolution[] availableResolutions)
        {
            AvailableResolutions = availableResolutions;
        }

        public SingleDeleteProblemResolution[] AvailableResolutions { get; }
    }
}

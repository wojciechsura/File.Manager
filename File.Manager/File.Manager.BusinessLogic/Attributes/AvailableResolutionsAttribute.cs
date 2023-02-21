﻿using File.Manager.BusinessLogic.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class AvailableResolutionsAttribute : Attribute
    {
        public AvailableResolutionsAttribute(params SingleProblemResolution[] availableResolutions)
        {
            AvailableResolutions = availableResolutions;
        }

        public SingleProblemResolution[] AvailableResolutions { get; }
    }
}

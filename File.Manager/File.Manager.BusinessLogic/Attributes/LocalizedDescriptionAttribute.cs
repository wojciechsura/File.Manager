using System;
using System.ComponentModel;
using System.Resources;

namespace BindingEnums
{
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        ResourceManager resourceManager;
        string resourceKey;

        public LocalizedDescriptionAttribute(string resourceKey, Type resourceType)
        {
            resourceManager = new ResourceManager(resourceType);
            this.resourceKey = resourceKey;
        }

        public override string Description
        {
            get
            {
                string description = resourceManager.GetString(resourceKey);
                return string.IsNullOrWhiteSpace(description) ? string.Format("[[{0}]]", resourceKey) : description;
            }
        }
    }
}
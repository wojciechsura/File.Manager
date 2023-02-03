using File.Manager.BusinessLogic.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.ViewModels.Base.Validation
{
    public class ValidationDefinition<T>
        where T : BaseValidateableViewModel<T>
    {
        private List<BaseValidationRule<T>> rules;
        private HashSet<string> dependentProperties;

        public ValidationDefinition(List<BaseValidationRule<T>> rules, HashSet<string> dependentProperties)
        {
            this.rules = rules;
            this.dependentProperties = dependentProperties;
        }

        public List<BaseValidationRule<T>> Rules => rules;
        public HashSet<string> DependentProperties => dependentProperties;
    }
}

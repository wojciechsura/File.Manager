using File.Manager.BusinessLogic.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.ViewModels.Base.Validation
{
    public abstract class BaseValidationRule<T>
        where T : BaseValidateableViewModel<T>
    {
        public abstract IEnumerable<string> Validate(T baseValidateableViewModel, string propertyName);
    }
}

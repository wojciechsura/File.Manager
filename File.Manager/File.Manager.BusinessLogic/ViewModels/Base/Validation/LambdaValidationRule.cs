using File.Manager.BusinessLogic.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.ViewModels.Base.Validation
{
    public class LambdaValidationRule<T> : BaseValidationRule<T>
        where T : BaseValidateableViewModel<T>
    {
        private readonly Func<T, string, IEnumerable<string>> validationFunc;

        public LambdaValidationRule(Func<T, string, IEnumerable<string>> validationFunc)
        {
            this.validationFunc = validationFunc;
        }

        public override IEnumerable<string> Validate(T baseValidateableViewModel, string propertyName)
        {
            return validationFunc(baseValidateableViewModel, propertyName);
        }
    }
}

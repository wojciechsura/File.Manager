using File.Manager.BusinessLogic.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace File.Manager.BusinessLogic.ViewModels.Base.Validation.Builder
{
    public interface IDefinitionBuilder<T, TValue> where T : BaseValidateableViewModel<T>
    {
        IDefinitionBuilder<T, TValue> AddRule(Func<T, string, IEnumerable<string>> lambda);
        IDefinitionBuilder<T, TValue> AddValueRule(Func<TValue, bool> validation, string errorMessage);
        IDefinitionBuilder<T, TValue> AlsoValidate<TDependentValue>(Expression<Func<T, TDependentValue>> property);
    }
}
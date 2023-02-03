using File.Manager.BusinessLogic.ViewModels.Base.Validation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.ViewModels.Base
{
    public abstract class BaseValidateableViewModel<T> : BaseViewModel, INotifyDataErrorInfo
        where T : BaseValidateableViewModel<T>
    {
        private readonly Dictionary<string, IReadOnlyList<string>> errors = new();

        protected abstract Dictionary<string, ValidationDefinition<T>> ValidationDefinitions { get; }

        private void ValidateProperty(string propertyName, HashSet<string> alreadyValidated = null)
        {
            // This means, that property has already been validated in the
            // process of multiple-properties-at-once validation
            if (alreadyValidated?.Contains(propertyName) ?? false)
                return;

            if (ValidationDefinitions.TryGetValue(propertyName, out var definition))
            {
                errors.TryGetValue(propertyName.ToString(), out var currentErrors);

                var newErrors = definition.Rules.SelectMany(r => r.Validate((T)this, propertyName))
                    .ToList();

                errors[propertyName] = newErrors;

                if (newErrors.Any() || !newErrors.Any() && currentErrors != null && currentErrors.Any())
                    OnErrorsChanged(propertyName);

                if (definition.DependentProperties.Any())
                {
                    if (alreadyValidated == null)
                        alreadyValidated = new();

                    alreadyValidated.Add(propertyName);
                    foreach (var dependentPropertyName in definition.DependentProperties)
                        if (!alreadyValidated.Contains(dependentPropertyName))
                            ValidateProperty(dependentPropertyName, alreadyValidated);
                }
            }
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            ValidateProperty(propertyName);

            base.OnPropertyChanged(propertyName);
        }

        protected virtual void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (errors.ContainsKey(propertyName))
                return errors[propertyName];

            return Enumerable.Empty<string>();
        }

        public IReadOnlyDictionary<string, IReadOnlyList<string>> Errors => errors;

        public bool HasErrors => errors.Values.Any(v => v is not null && v.Any());

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
    }
}

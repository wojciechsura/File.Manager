using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.ViewModels.Base.Validation.Builder
{
    public class ValidationRuleBuilder<T>
        where T : BaseValidateableViewModel<T>
    {
        private class DefinitionBuilder<TValue> : IDefinitionBuilder<T, TValue>
        {
            private List<BaseValidationRule<T>> rules;
            private HashSet<string> dependentProperties;

            private static TValue GetPropertyValue(object obj, string propertyName)
            {
                var propInfo = obj.GetType().GetProperty(propertyName);
                if (propInfo == null)
                    throw new ArgumentException(propertyName);

                return (TValue)propInfo.GetValue(obj);
            }

            private static IEnumerable<string> ValidateValueRule(T obj, string propertyName, Func<TValue, bool> validation, string errorMessage)
            {
                TValue value = GetPropertyValue(obj, propertyName);
                if (!validation(value))
                    yield return errorMessage;

                yield break;
            }

            public DefinitionBuilder()
            {
                rules = new();
                dependentProperties = new();
            }

            public ValidationDefinition<T> BuildDefinition()
            {
                var result = new ValidationDefinition<T>(rules, dependentProperties);
                rules = null;
                dependentProperties = null;

                return result;
            }

            public IDefinitionBuilder<T, TValue> AddRule(Func<T, string, IEnumerable<string>> lambda)
            {
                rules.Add(new LambdaValidationRule<T>(lambda));

                return this;
            }

            public IDefinitionBuilder<T, TValue> AddValueRule(Func<TValue, bool> validation, string errorMessage)
            {
                rules.Add(new LambdaValidationRule<T>((obj, propName) => ValidateValueRule(obj, propName, validation, errorMessage)));

                return this;
            }

            public IDefinitionBuilder<T, TValue> AlsoValidate<TDependentValue>(Expression<Func<T, TDependentValue>> property)
            {
                if (property.Body is not MemberExpression memberExpression)
                    throw new ArgumentException("You must access a property!", nameof(property));

                var member = memberExpression.Member;
                if (member is not PropertyInfo propertyInfo)
                    throw new ArgumentException("You must access a property!", nameof(property));

                string dependentPropertyName = propertyInfo.Name;

                dependentProperties.Add(dependentPropertyName);

                return this;
            }
        }

        private Dictionary<string, ValidationDefinition<T>> definitions;

        public ValidationRuleBuilder()
        {
            definitions = new();
        }

        public ValidationRuleBuilder(Dictionary<string, ValidationDefinition<T>> existingDefinitions)
        {
            definitions = existingDefinitions;
        }

        public ValidationRuleBuilder<T> For<TValue>(Expression<Func<T, TValue>> property, Action<IDefinitionBuilder<T, TValue>> rules)
        {
            if (property.Body is not MemberExpression memberExpression)
                throw new ArgumentException("You must access a property!", nameof(property));

            var member = memberExpression.Member;
            if (member is not PropertyInfo propertyInfo)
                throw new ArgumentException("You must access a property!", nameof(property));

            string name = propertyInfo.Name;

            if (definitions.ContainsKey(name))
                throw new ArgumentException($"You already defined rules for property {name}!");

            DefinitionBuilder<TValue> rulesBuilder = new();
            rules(rulesBuilder);

            definitions.Add(name, rulesBuilder.BuildDefinition());

            return this;
        }

        public Dictionary<string, ValidationDefinition<T>> Build()
        {
            var result = definitions;
            definitions = null;

            return result;
        }
    }
}

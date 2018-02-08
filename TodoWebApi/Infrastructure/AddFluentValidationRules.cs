using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentValidation;
using FluentValidation.Attributes;
using FluentValidation.Validators;
using Swashbuckle.Swagger;


namespace WebApi.Infrastructure
{
    public class AddFluentValidationRules : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            IValidator validator = GetValidator(type);

            if (validator == null)
            {
                return;
            }

            schema.required = new List<string>();

            IValidatorDescriptor validatorDescriptor = validator.CreateDescriptor();

            foreach (string key in schema.properties.Keys)
            {
                foreach (IPropertyValidator propertyValidator in validatorDescriptor.GetValidatorsForMember(key))
                {
                    if (propertyValidator is NotEmptyValidator)
                    {
                        schema.required.Add(key);
                    }

                    if (propertyValidator is LengthValidator)
                    {
                        LengthValidator lengthValidator = (LengthValidator)propertyValidator;
                        if (lengthValidator.Max > 0)
                        {
                            schema.properties[key].maxLength = lengthValidator.Max;
                        }
                        if(lengthValidator.Min > 0)
                            schema.properties[key].minLength = lengthValidator.Min;
                    }

                    if (propertyValidator is RegularExpressionValidator)
                    {
                        RegularExpressionValidator regexExpressionValidator = (RegularExpressionValidator)propertyValidator;
                        schema.properties[key].pattern = regexExpressionValidator.Expression;
                    }
                    AddRule<GreaterThanOrEqualValidator>(propertyValidator, (number) =>
                    {
                        schema.properties[key].minimum = number;
                        schema.properties[key].exclusiveMinimum = false;
                    } );
                    AddRule<GreaterThanValidator>(propertyValidator, (number) =>
                    {
                        schema.properties[key].minimum = number;
                        schema.properties[key].exclusiveMinimum = true;
                    } );
                    AddRule<LessThanOrEqualValidator>(propertyValidator, (number) =>
                    {
                        schema.properties[key].maximum = number;
                        schema.properties[key].exclusiveMaximum = false;
                    } );
                    AddRule<LessThanValidator>(propertyValidator, (number) =>
                    {
                        schema.properties[key].maximum = number;
                        schema.properties[key].exclusiveMaximum = true;
                    } );
                }
            }
        }

        private static void AddRule<T>(IPropertyValidator propertyValidator, Action<int> action) where T: AbstractComparisonValidator
        {
            if (propertyValidator is T)
            {
                T gvalidator = (T)propertyValidator;
                if (gvalidator.ValueToCompare == null)
                    return;
                int parsed = 0;
                if (Int32.TryParse(gvalidator.ValueToCompare.ToString(), out parsed))
                {
                    action(parsed);
                }
            }
        }

        private IValidator GetValidator(Type t)
        {
            CustomAttributeData attr = t.CustomAttributes.FirstOrDefault(x => x.AttributeType.Equals(typeof(ValidatorAttribute)));
            if (attr == null)
                return null;
                CustomAttributeTypedArgument validatorType = attr.ConstructorArguments.FirstOrDefault();
            if (validatorType == null)
                return null;
            IValidator validator = (IValidator) System.Activator.CreateInstance((Type) validatorType.Value);
            return validator;

        }
    }
}
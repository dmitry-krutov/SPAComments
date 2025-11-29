using CSharpFunctionalExtensions;
using FluentValidation;
using SPAComments.SharedKernel;

namespace SPAComments.Core.Validation;

public static class CustomValidators
{
    public static IRuleBuilderOptionsConditions<T, TElement> MustBeValueObject<T, TElement, TValueObject>(
        this IRuleBuilder<T, TElement> ruleBuilder,
        Func<TElement, Result<TValueObject, Error>> factoryMethod)
    {
        return ruleBuilder.Custom((value, context) =>
        {
            var result = factoryMethod(value);

            if (result.IsFailure)
                context.AddFailure(result.Error.Serialize());
        });
    }

    public static IRuleBuilderOptionsConditions<T, TElement> MustBeValueObject<T, TElement, TValueObject>(
        this IRuleBuilder<T, TElement> ruleBuilder,
        Func<TElement, Result<TValueObject, Error>> factoryMethod,
        Action<T, TValueObject> assignToCommand)
    {
        return ruleBuilder.Custom((value, context) =>
        {
            var instance = (T)context.InstanceToValidate;
            var result = factoryMethod(value);

            if (result.IsSuccess)
                assignToCommand(instance, result.Value);
            else
                context.AddFailure(result.Error.Serialize());
        });
    }

    public static IRuleBuilderOptions<T, TProperty> WithError<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule, Error error)
    {
        return rule.WithMessage(error.Serialize());
    }
}
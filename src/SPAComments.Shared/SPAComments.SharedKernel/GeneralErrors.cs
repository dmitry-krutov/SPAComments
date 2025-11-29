namespace SPAComments.SharedKernel;

public class GeneralErrors
{
    public static class Validation
    {
        public static Error ValueTooSmall<T>(string name, T min)
            where T : struct, IComparable
        {
            return Error.Validation("value.too.small", $"{name} must be at least {min}");
        }

        public static Error ValueTooLong(string name, int max)
        {
            return Error.Validation("value.too.long", $"{name} must not exceed {max} characters");
        }

        public static Error ValueTooLarge<T>(string name, T max)
            where T : struct, IComparable
        {
            return Error.Validation("value.too.large", $"{name} must not exceed {max}");
        }

        public static Error ValueIsRequired(string? name = null)
        {
            var label = name ?? "value";
            return Error.Validation("value.is.required", $"{label} is required");
        }

        public static Error InvalidFormat(string name, string message)
        {
            return Error.Validation("value.invalid.format", $"{name} has invalid format: {message}");
        }
    }
}
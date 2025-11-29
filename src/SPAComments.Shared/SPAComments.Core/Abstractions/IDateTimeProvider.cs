namespace SPAComments.Core.Abstractions;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
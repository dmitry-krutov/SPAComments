using SPAComments.Core.Abstractions;

namespace SPAComments.CommentsModule.Infrastructure;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
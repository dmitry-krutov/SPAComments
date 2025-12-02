using SPAComments.SharedKernel;

namespace SPAComments.CommentsModule.Domain.Events;

public sealed record CommentCreatedDomainEvent(
    Guid CommentId,
    Guid? ParentId,
    string UserName,
    string Email,
    string? HomePage,
    string Text,
    DateTime CreatedAt,
    IReadOnlyCollection<Guid> AttachmentIds
) : IDomainEvent;
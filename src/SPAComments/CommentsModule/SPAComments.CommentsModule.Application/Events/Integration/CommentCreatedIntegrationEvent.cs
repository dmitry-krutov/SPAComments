namespace SPAComments.CommentsModule.Application.Events.Integration;

public record CommentCreatedIntegrationEvent(
    Guid CommentId,
    Guid? ParentId,
    string UserName,
    string Email,
    string? HomePage,
    string Text,
    DateTime CreatedAt,
    IReadOnlyCollection<Guid> AttachmentIds
);
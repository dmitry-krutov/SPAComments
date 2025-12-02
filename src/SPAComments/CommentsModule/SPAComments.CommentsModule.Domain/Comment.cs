using SPAComments.CommentsModule.Domain.Events;
using SPAComments.CommentsModule.Domain.ValueObjects;
using SPAComments.SharedKernel;
using SPAComments.SharedKernel.ValueObjects.Ids;

namespace SPAComments.CommentsModule.Domain;

public class Comment : DomainEntity<CommentId>
{
    private readonly List<CommentAttachment> _attachments = new();

    public Comment(
        CommentId id,
        CommentId? parentCommentId,
        UserName userName,
        Email email,
        HomePage? homePage,
        Text text,
        DateTime createdAt,
        IReadOnlyCollection<CommentAttachment>? attachments = null)
        : base(id)
    {
        ParentCommentId = parentCommentId;
        UserName = userName;
        Email = email;
        HomePage = homePage;
        Text = text;
        CreatedAt = createdAt;

        if (attachments is not null && attachments.Count > 0)
            _attachments.AddRange(attachments);

        AddDomainEvent(new CommentCreatedDomainEvent(
            id.Value,
            parentCommentId?.Value,
            userName.Value,
            email.Value,
            homePage?.Value,
            text.Value,
            createdAt,
            attachments?.Select(a => a.FileId).ToArray() ?? Array.Empty<Guid>()));
    }

    private Comment(CommentId id)
        : base(id)
    {
    }

    public CommentId? ParentCommentId { get; private set; }

    public UserName UserName { get; private set; }

    public Email Email { get; private set; }

    public HomePage? HomePage { get; private set; }

    public Text Text { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyCollection<CommentAttachment> Attachments => _attachments.AsReadOnly();
}
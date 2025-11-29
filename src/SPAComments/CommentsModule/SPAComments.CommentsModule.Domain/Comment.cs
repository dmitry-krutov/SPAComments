using SPAComments.CommentsModule.Domain.ValueObjects;
using SPAComments.SharedKernel;
using SPAComments.SharedKernel.ValueObjects.Ids;

namespace SPAComments.CommentsModule.Domain;

public class Comment : DomainEntity<CommentId>
{
    public Comment(
        CommentId id,
        CommentId? parentCommentId,
        UserName userName,
        Email email,
        HomePage? homePage,
        Text text,
        DateTime createdAt)
        : base(id)
    {
        ParentCommentId = parentCommentId;
        UserName = userName;
        Email = email;
        HomePage = homePage;
        Text = text;
        CreatedAt = createdAt;
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
}
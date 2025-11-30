using CSharpFunctionalExtensions;
using SPAComments.Core.Abstractions;
using SPAComments.Core.Security;
using SPAComments.SharedKernel;

namespace SPAComments.CommentsModule.Application.Common;

public sealed class HtmlSanitizeCommandHandlerDecorator<TResponse, TCommand>
    : ICommandHandler<TResponse, TCommand>
    where TCommand : ICommand, IHasCommentText
{
    private readonly ICommandHandler<TResponse, TCommand> _inner;
    private readonly IHtmlContentFilter _htmlContentFilter;

    public HtmlSanitizeCommandHandlerDecorator(
        ICommandHandler<TResponse, TCommand> inner,
        IHtmlContentFilter htmlContentFilter)
    {
        _inner = inner;
        _htmlContentFilter = htmlContentFilter;
    }

    public Task<Result<TResponse, ErrorList>> Handle(
        TCommand command,
        CancellationToken cancellationToken)
    {
        if (command is IHasCommentText withText)
        {
            withText.Text = _htmlContentFilter.Sanitize(withText.Text);
        }

        return _inner.Handle(command, cancellationToken);
    }
}
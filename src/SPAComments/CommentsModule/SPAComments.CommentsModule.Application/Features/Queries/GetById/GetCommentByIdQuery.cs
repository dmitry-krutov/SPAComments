using System;
using SPAComments.Core.Abstractions;

namespace SPAComments.CommentsModule.Application.Features.Queries.GetById;

public sealed class GetCommentByIdQuery : IQuery
{
    public Guid Id { get; init; }
}

using CSharpFunctionalExtensions;
using SPAComments.SharedKernel;

namespace SPAComments.Core.Abstractions;

public interface IQueryHandler<TResponse, in TQuery>
    where TQuery : IQuery
{
    public Task<TResponse> Handle(TQuery query, CancellationToken cancellationToken);
}

public interface IQueryHandlerWithResult<TResponse, in TQuery>
    where TQuery : IQuery
{
    public Task<Result<TResponse, ErrorList>> Handle(TQuery query, CancellationToken cancellationToken);
}
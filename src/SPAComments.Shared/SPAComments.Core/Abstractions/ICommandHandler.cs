using CSharpFunctionalExtensions;
using SPAComments.SharedKernel;

namespace SPAComments.Core.Abstractions;

public interface ICommandHandler<TResponse, in TCommand>
    where TCommand : ICommand
{
    public Task<Result<TResponse, ErrorList>> Handle(TCommand command, CancellationToken cancellationToken);
}

public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    public Task<UnitResult<ErrorList>> Handle(TCommand command, CancellationToken cancellationToken);
}
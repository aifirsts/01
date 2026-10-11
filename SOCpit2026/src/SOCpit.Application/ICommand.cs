// SOCpit - Security Operations Center
// CQRS command interface
// Based on MATRIX V3 with CQRS pattern

using System.Threading;
using System.Threading.Tasks;

namespace SOCpit.Application
{
    /// <summary>Base interface for commands.</summary>
    /// <typeparam name="TResult">The result type of the command.</typeparam>
    public interface ICommand<TResult>
    {
    }

    /// <summary>Base interface for command handlers.</summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
    {
        Task<TResult> Handle(TCommand command, CancellationToken cancellationToken);
    }

    /// <summary>Base interface for queries.</summary>
    /// <typeparam name="TResult">The result type of the query.</typeparam>
    public interface IQuery<TResult>
    {
    }

    /// <summary>Base interface for query handlers.</summary>
    /// <typeparam name="TQuery">The query type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
    {
        Task<TResult> Handle(TQuery query, CancellationToken cancellationToken);
    }
}

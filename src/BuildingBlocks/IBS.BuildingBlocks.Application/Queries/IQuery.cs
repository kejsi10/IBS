using MediatR;

namespace IBS.BuildingBlocks.Application.Queries;

/// <summary>
/// Marker interface for queries.
/// </summary>
/// <typeparam name="TResult">The type of result returned by the query.</typeparam>
public interface IQuery<TResult> : IRequest<Result<TResult>>
{
}

/// <summary>
/// Interface for query handlers.
/// </summary>
/// <typeparam name="TQuery">The type of query to handle.</typeparam>
/// <typeparam name="TResult">The type of result returned by the handler.</typeparam>
public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, Result<TResult>>
    where TQuery : IQuery<TResult>
{
}
